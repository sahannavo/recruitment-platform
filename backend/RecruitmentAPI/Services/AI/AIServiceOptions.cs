namespace RecruitmentAPI.Services.AI
{
    /// <summary>Bind this to the "AIService" section of appsettings.json.</summary>
    public class AIServiceOptions
    {
        public const string SectionName = "AIService";

        public string Provider { get; set; } = "OpenRouter"; // "OpenRouter" | "OpenAI" | "AzureFormRecognizer" | "None"
        public string? ApiKey { get; set; }
        public string? Endpoint { get; set; }
        public string Model { get; set; } = "google/gemma-4-26b-a4b-it:free";
        public int TimeoutSeconds { get; set; } = 30;
        public string? BaseUrl { get; set; } // For OpenRouter: "https://openrouter.ai/api/v1"
    }
}
