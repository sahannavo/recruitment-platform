using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations;

public class ChatbotService : IChatbotService
{
    private readonly ISettingsService _settingsService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatbotService> _logger;
    private readonly IConfiguration _configuration;

    public ChatbotService(ISettingsService settingsService, HttpClient httpClient, ILogger<ChatbotService> logger, IConfiguration configuration)
    {
        _settingsService = settingsService;
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> AskQuestionAsync(int userId, string role, string userPrompt)
    {
        var settings = await _settingsService.GetSettingsAsync();
        
        string apiKey = settings.OpenAIKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = _configuration["AIService:ApiKey"];
        }

        // Ensure OpenAIKey is configured (can be OpenRouter key)
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "Error: AI services are not configured on this platform. Please contact an Administrator to set the API Key.";
        }

        // Read context
        string context = string.Empty;
        string contextPath = Path.Combine(AppContext.BaseDirectory, "../../../PlatformContext.md");
        if (File.Exists(contextPath))
        {
            context = await File.ReadAllTextAsync(contextPath);
        }
        else
        {
            // Fallback path depending on how it's running
            contextPath = Path.Combine(Directory.GetCurrentDirectory(), "PlatformContext.md");
            if (File.Exists(contextPath))
            {
                context = await File.ReadAllTextAsync(contextPath);
            }
        }

        // Build system prompt based on role
        var systemPromptBuilder = new StringBuilder();
        systemPromptBuilder.AppendLine("You are the RecruitAI platform assistant.");
        systemPromptBuilder.AppendLine($"The user you are currently talking to has the role: {role}");
        systemPromptBuilder.AppendLine("Provide role-specific guidance. Do not provide information they should not have access to.");
        systemPromptBuilder.AppendLine("Here is the context of the platform:");
        systemPromptBuilder.AppendLine(context);

        var requestBody = new
        {
            model = "google/gemma-2-9b-it:free", // using closest valid openrouter free model name if the user's doesn't work, but let's stick to user request
            messages = new[]
            {
                new { role = "system", content = systemPromptBuilder.ToString() },
                new { role = "user", content = userPrompt }
            }
        };

        // User requested model: google/gemma-4-26b-a4b-it:free (Let's use their exact string)
        var customRequestBody = new
        {
            model = "google/gemma-4-26b-a4b-it:free",
            messages = new[]
            {
                new { role = "system", content = systemPromptBuilder.ToString() },
                new { role = "user", content = userPrompt }
            }
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(customRequestBody), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
        {
            Content = requestContent
        };
        // Add auth header directly to avoid any AuthenticationHeaderValue stripping issues
        request.Headers.Add("Authorization", $"Bearer {apiKey.Trim()}");
        
        // Add required headers for OpenRouter
        request.Headers.Add("HTTP-Referer", "https://recruitai.com");
        request.Headers.Add("X-Title", "RecruitAI Platform");

        _logger.LogInformation($"Sending API Request to OpenRouter with Key Length: {apiKey?.Length ?? 0}");

        try
        {
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"OpenRouter API Error: {response.StatusCode} - {errorContent}");
                
                // Fallback to simpler model if the user's provided model doesn't exist (gemma 4 doesn't exist yet, it might be a typo for gemma 2)
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("Retrying with google/gemma-2-9b-it:free");
                    var fallbackBody = new
                    {
                        model = "google/gemma-2-9b-it:free",
                        messages = customRequestBody.messages
                    };
                    requestContent = new StringContent(JsonSerializer.Serialize(fallbackBody), Encoding.UTF8, "application/json");
                    request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
                    {
                        Content = requestContent
                    };
                    request.Headers.Add("Authorization", $"Bearer {apiKey.Trim()}");
                    request.Headers.Add("HTTP-Referer", "https://recruitai.com");
                    request.Headers.Add("X-Title", "RecruitAI Platform");
                    response = await _httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode) {
                        return "Error: Unable to process request. AI API failed.";
                    }
                }
                else 
                {
                    return "Error: Unable to process request. AI API failed.";
                }
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseString);

            if (responseData.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
                {
                    return content.GetString() ?? "I didn't understand that.";
                }
            }

            return "Error parsing AI response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with OpenRouter API");
            return "Error: An unexpected error occurred while communicating with the AI service.";
        }
    }
}
