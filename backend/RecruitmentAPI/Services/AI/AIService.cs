using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecruitmentAPI.DTOs.AI;

namespace RecruitmentAPI.Services.AI
{
    /// <summary>
    /// AI-powered resume parsing and candidate/job matching.
    /// Calls an external LLM provider (OpenAI-compatible chat completions endpoint) when configured,
    /// and always falls back to deterministic keyword matching if the call fails, times out,
    /// or no provider is configured. This guarantees the recruitment pipeline never blocks on AI
    /// availability.
    /// </summary>
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceOptions _options;
        private readonly ILogger<AIService> _logger;

        // A small, extensible seed dictionary used by the keyword fallback.
        // In production this would be backed by a Skills reference table.
        private static readonly string[] KnownSkills = new[]
        {
            "c#", ".net", "asp.net core", "entity framework", "sql server", "javascript", "typescript",
            "react", "angular", "vue", "node.js", "python", "java", "spring boot", "docker", "kubernetes",
            "azure", "aws", "gcp", "rest api", "graphql", "microservices", "ci/cd", "git", "agile", "scrum",
            "unit testing", "xunit", "nunit", "moq", "sql", "nosql", "mongodb", "redis", "html", "css",
            "machine learning", "data analysis", "project management", "communication", "leadership"
        };

        // Common degree keywords for education extraction fallback.
        private static readonly string[] DegreeKeywords = new[]
        {
            "bachelor", "master", "phd", "doctorate", "associate", "diploma",
            "bsc", "msc", "b.sc", "m.sc", "b.eng", "m.eng", "mba", "b.a.", "m.a.",
            "btech", "mtech", "b.tech", "m.tech"
        };

        /// <summary>
        /// Initialises a new instance of <see cref="AIService"/>.
        /// </summary>
        /// <param name="httpClient">HTTP client for communicating with the AI provider.</param>
        /// <param name="options">AI service configuration options.</param>
        /// <param name="logger">Logger for diagnostics and fallback tracing.</param>
        public AIService(HttpClient httpClient, IOptions<AIServiceOptions> options, ILogger<AIService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ResumeParseResult> ParseResumeAsync(string resumeText)
        {
            if (string.IsNullOrWhiteSpace(resumeText))
            {
                _logger.LogDebug("ParseResumeAsync called with empty resume text");
                return new ResumeParseResult { ParsedSuccessfully = false, ParseEngine = "None" };
            }

            if (IsProviderConfigured())
            {
                try
                {
                    var aiResult = await ParseResumeWithProviderAsync(resumeText);
                    if (aiResult is not null)
                    {
                        aiResult.ParsedSuccessfully = true;
                        aiResult.ParseEngine = _options.Provider;
                        return aiResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI provider resume parse failed, falling back to keyword parsing");
                }
            }

            return ParseResumeWithKeywordFallback(resumeText);
        }

        /// <inheritdoc />
        public async Task<List<string>> ExtractSkillsAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogDebug("ExtractSkillsAsync called with empty text, returning empty skill list");
                return new List<string>();
            }

            if (IsProviderConfigured())
            {
                try
                {
                    var skills = await ExtractSkillsWithProviderAsync(text);
                    if (skills is { Count: > 0 }) return skills;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI provider skill extraction failed, falling back to keyword matching");
                }
            }

            return ExtractSkillsWithKeywordFallback(text);
        }

        /// <inheritdoc />
        public async Task<MatchScoreDto> MatchCandidateToJobAsync(int candidateId, string candidateSkillsText,
            int jobId, string jobRequirementsText)
        {
            var candidateSkills = await ExtractSkillsAsync(candidateSkillsText);
            var jobSkills = await ExtractSkillsAsync(jobRequirementsText);

            return CalculateMatchScore(candidateId, jobId, candidateSkills, jobSkills, candidateSkillsText);
        }

        /// <inheritdoc />
        public async Task<List<CandidateRankingDto>> RankCandidatesAsync(int jobId, string jobRequirementsText,
            IDictionary<int, string> candidateSkillsById)
        {
            var jobSkills = await ExtractSkillsAsync(jobRequirementsText);
            var results = new List<CandidateRankingDto>();

            foreach (var (candidateId, skillsText) in candidateSkillsById)
            {
                var candidateSkills = await ExtractSkillsAsync(skillsText);
                var match = CalculateMatchScore(candidateId, jobId, candidateSkills, jobSkills, skillsText);

                // Extract a candidate name from the skills text if it starts with a name-like
                // pattern (e.g., "John Doe - C#, ASP.NET Core"). This is a best-effort extraction;
                // callers should overwrite CandidateName from their own User lookup when available.
                var candidateName = ExtractCandidateNameHeuristic(skillsText);

                results.Add(new CandidateRankingDto
                {
                    CandidateId = candidateId,
                    CandidateName = candidateName,
                    Score = match.Score
                });
            }

            var ranked = results.OrderByDescending(r => r.Score).ToList();
            for (int i = 0; i < ranked.Count; i++)
            {
                ranked[i].Rank = i + 1;
            }

            return ranked;
        }

        /// <inheritdoc />
        public async Task<AIFeedbackDto> GenerateFeedbackAsync(string interviewNotes, int technicalScore,
            int behavioralScore, int communicationScore)
        {
            // Try the LLM provider for richer narrative summaries when configured.
            if (IsProviderConfigured())
            {
                try
                {
                    var llmFeedback = await GenerateFeedbackWithProviderAsync(
                        interviewNotes, technicalScore, behavioralScore, communicationScore);
                    if (llmFeedback is not null) return llmFeedback;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI provider feedback generation failed, falling back to deterministic logic");
                }
            }

            // Deterministic fallback — always available.
            return GenerateFeedbackDeterministic(interviewNotes, technicalScore, behavioralScore, communicationScore);
        }

        /// <inheritdoc />
        public async Task<string> GenerateCandidateProfileSummaryAsync(string skills, string biography)
        {
            if (string.IsNullOrWhiteSpace(skills) && string.IsNullOrWhiteSpace(biography))
            {
                return "This candidate has not provided any skills or biography information.";
            }

            if (IsProviderConfigured())
            {
                try
                {
                    // Construct the prompt
                    var prompt = $@"
You are a professional technical recruiter writing an objective summary of a candidate.
Based on the following profile information provided by the candidate, write a comprehensive summary (3-4 paragraphs) summarizing their profile. 
Focus on their key strengths and background.
You MUST output your response in HTML format using tags like <b>, <ul>, <li>, and <br>.
You MUST include bold headers (e.g., <b>Professional Background</b><br>, <b>Core Skills</b><br>, etc.) to structure the summary nicely.
At the very end of your response, add this exact HTML snippet:
<br><br><span style='font-size: 12px; font-style: italic; color: gray;'>Note: This is an AI-generated description based on the candidate's profile.</span>

Candidate Profile Information:
Skills: {skills ?? "None provided"}
Biography/Summary: {biography ?? "None provided"}
";

                    using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
                    request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
                    
                    var payload = new
                    {
                        model = _options.Model,
                        messages = new[]
                        {
                            new { role = "system", content = "You are an expert HR assistant." },
                            new { role = "user", content = prompt }
                        },
                        temperature = 0.5,
                        max_tokens = 1000
                    };

                    request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using var document = System.Text.Json.JsonDocument.Parse(content);
                        if (document.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var message = choices[0].GetProperty("message").GetProperty("content").GetString();
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                return message.Trim();
                            }
                        }
                    }
                    
                    _logger.LogWarning("OpenAI API call for candidate profile summary failed or returned empty. Status: {StatusCode}", response.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate AI candidate profile summary.");
                }
            }

            // Fallback when AI fails or is not configured
            var summaryText = "Candidate Profile Summary:\n";
            if (!string.IsNullOrWhiteSpace(biography))
                summaryText += $"- Biography: {biography}\n";
            if (!string.IsNullOrWhiteSpace(skills))
                summaryText += $"- Key Skills: {skills}";
            
            return summaryText;
        }

        // ---------- Internal helpers ----------

        /// <summary>
        /// Checks whether an external AI provider is configured and available.
        /// </summary>
        private bool IsProviderConfigured() =>
            !string.IsNullOrWhiteSpace(_options.ApiKey) &&
            !string.Equals(_options.Provider, "None", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Calculates a weighted match score from overlapping skills and an experience heuristic.
        /// </summary>
        private MatchScoreDto CalculateMatchScore(int candidateId, int jobId, List<string> candidateSkills,
            List<string> jobSkills, string candidateSkillsText)
        {
            var normalizedCandidate = candidateSkills.Select(s => s.ToLowerInvariant()).ToHashSet();
            var normalizedJob = jobSkills.Select(s => s.ToLowerInvariant()).ToHashSet();

            var matched = normalizedJob.Intersect(normalizedCandidate).ToList();
            var missing = normalizedJob.Except(normalizedCandidate).ToList();

            double skillsMatch = normalizedJob.Count == 0
                ? 0
                : Math.Round((double)matched.Count / normalizedJob.Count * 100, 2);

            // Simple experience heuristic: count of years mentioned in candidate text, capped at 10 -> 100%
            int years = ExtractYearsOfExperience(candidateSkillsText);
            double experienceMatch = Math.Min(years / 10.0 * 100, 100);

            double overall = Math.Round((skillsMatch * 0.7) + (experienceMatch * 0.3), 2);

            return new MatchScoreDto
            {
                CandidateId = candidateId,
                JobId = jobId,
                Score = overall,
                SkillsMatch = skillsMatch,
                ExperienceMatch = experienceMatch,
                MatchedSkills = matched,
                MissingSkills = missing
            };
        }

        /// <summary>
        /// Extracts years of experience from free text using regex.
        /// </summary>
        private static int ExtractYearsOfExperience(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            var match = Regex.Match(text, @"(\d+)\+?\s*(years|yrs)", RegexOptions.IgnoreCase);
            return match.Success && int.TryParse(match.Groups[1].Value, out var years) ? years : 0;
        }

        /// <summary>
        /// Keyword fallback for resume parsing. Extracts skills, experience years,
        /// basic education entries, and experience entries from the resume text.
        /// </summary>
        private ResumeParseResult ParseResumeWithKeywordFallback(string resumeText)
        {
            var skills = ExtractSkillsWithKeywordFallback(resumeText);
            var education = ExtractEducationFromText(resumeText);
            var experience = ExtractExperienceFromText(resumeText);

            return new ResumeParseResult
            {
                ExtractedText = resumeText,
                Skills = skills,
                Experience = experience,
                Education = education,
                YearsOfExperience = ExtractYearsOfExperience(resumeText),
                ParsedSuccessfully = true,
                ParseEngine = "KeywordFallback"
            };
        }

        /// <summary>
        /// Extracts skills from text by matching against the known skills dictionary.
        /// </summary>
        private List<string> ExtractSkillsWithKeywordFallback(string text)
        {
            var lower = text.ToLowerInvariant();
            return KnownSkills
                .Where(skill => lower.Contains(skill))
                .Select(skill => CultureFriendlyCase(skill))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Extracts basic education entries from resume text using regex patterns
        /// for common degree keywords and institution patterns.
        /// </summary>
        private static List<EducationEntry> ExtractEducationFromText(string text)
        {
            var entries = new List<EducationEntry>();
            var lower = text.ToLowerInvariant();

            foreach (var degreeKw in DegreeKeywords)
            {
                if (!lower.Contains(degreeKw)) continue;

                // Try to match a pattern like "BSc in Computer Science" or "Bachelor of Engineering"
                var pattern = $@"({Regex.Escape(degreeKw)})\s*(?:of|in|\.?\s*)?\s*([A-Za-z\s]{{2,40}})";
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var degree = match.Value.Trim();
                    // Try to extract a year nearby
                    var yearMatch = Regex.Match(text.Substring(Math.Max(0, match.Index - 20),
                        Math.Min(match.Length + 40, text.Length - Math.Max(0, match.Index - 20))),
                        @"(19|20)\d{2}");

                    entries.Add(new EducationEntry
                    {
                        Degree = degree,
                        Year = yearMatch.Success ? yearMatch.Value : null,
                        Institution = null // Cannot reliably extract institution names without NER
                    });
                }
            }

            return entries.Take(5).ToList(); // Cap at 5 to avoid noise
        }

        /// <summary>
        /// Extracts basic experience entries from resume text using patterns for
        /// job-title-like phrases and duration mentions.
        /// </summary>
        private static List<ExperienceEntry> ExtractExperienceFromText(string text)
        {
            var entries = new List<ExperienceEntry>();

            // Match patterns like "Software Engineer at Google" or "Senior Developer - Microsoft"
            var titlePatterns = Regex.Matches(text,
                @"((?:senior|junior|lead|principal|staff|associate)?\s*(?:software|full[\s-]?stack|front[\s-]?end|back[\s-]?end|devops|data|cloud|qa|test)\s*(?:engineer|developer|architect|analyst|manager|consultant))\s*(?:at|@|-|–|,)\s*([A-Za-z0-9\s&.]+)",
                RegexOptions.IgnoreCase);

            foreach (Match m in titlePatterns)
            {
                if (m.Groups.Count >= 3)
                {
                    var durationMatch = Regex.Match(
                        text.Substring(m.Index, Math.Min(m.Length + 50, text.Length - m.Index)),
                        @"(\d+)\+?\s*(?:years|yrs|months|mos)", RegexOptions.IgnoreCase);

                    entries.Add(new ExperienceEntry
                    {
                        Title = m.Groups[1].Value.Trim(),
                        Company = m.Groups[2].Value.Trim().TrimEnd('.', ','),
                        Duration = durationMatch.Success ? durationMatch.Value.Trim() : null
                    });
                }
            }

            return entries.Take(10).ToList(); // Cap at 10
        }

        /// <summary>
        /// Best-effort extraction of a candidate name from the start of their profile text.
        /// Returns "Candidate {id}" style placeholder if no name-like pattern is detected.
        /// </summary>
        private static string ExtractCandidateNameHeuristic(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Check if text starts with a name-like pattern (two capitalized words)
            var nameMatch = Regex.Match(text, @"^([A-Z][a-z]+\s+[A-Z][a-z]+)");
            return nameMatch.Success ? nameMatch.Groups[1].Value : string.Empty;
        }

        /// <summary>
        /// Deterministic feedback generation, used as fallback when no AI provider is available.
        /// </summary>
        private static AIFeedbackDto GenerateFeedbackDeterministic(string interviewNotes,
            int technicalScore, int behavioralScore, int communicationScore)
        {
            var average = (technicalScore + behavioralScore + communicationScore) / 3.0;
            var decision = average >= 7 ? "Advance" : average >= 5 ? "Hold" : "Reject";

            var strengths = new List<string>();
            var concerns = new List<string>();

            if (technicalScore >= 7) strengths.Add("Strong technical proficiency");
            else if (technicalScore < 5) concerns.Add("Technical skills below expectations");

            if (behavioralScore >= 7) strengths.Add("Good cultural/behavioral fit");
            else if (behavioralScore < 5) concerns.Add("Behavioral fit concerns raised");

            if (communicationScore >= 7) strengths.Add("Clear and confident communicator");
            else if (communicationScore < 5) concerns.Add("Communication needs improvement");

            var summary = string.IsNullOrWhiteSpace(interviewNotes)
                ? $"Overall average score {average:F1}/10. Recommendation: {decision}."
                : $"Overall average score {average:F1}/10. Recommendation: {decision}. Notes reviewed.";

            return new AIFeedbackDto
            {
                Summary = summary,
                Strengths = strengths,
                Concerns = concerns,
                RecommendedDecision = decision
            };
        }

        /// <summary>
        /// Converts known skill identifiers to their display-friendly casing.
        /// </summary>
        private static string CultureFriendlyCase(string skill)
        {
            // Keep well-known acronyms/casing readable (C#, .NET, SQL Server, etc.)
            return skill switch
            {
                "c#" => "C#",
                ".net" => ".NET",
                "asp.net core" => "ASP.NET Core",
                "sql server" => "SQL Server",
                "sql" => "SQL",
                "nosql" => "NoSQL",
                "ci/cd" => "CI/CD",
                _ => System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(skill)
            };
        }

        /// <summary>
        /// Calls an OpenAI-compatible chat completions endpoint and asks it to return
        /// strict JSON matching ResumeParseResult. Throws on any failure so the caller can fall back.
        /// </summary>
        private async Task<ResumeParseResult?> ParseResumeWithProviderAsync(string resumeText)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var requestBody = new
            {
                model = _options.Model,
                messages = new object[]
                {
            new { role = "system", content = "You extract structured resume data. Respond ONLY with JSON: {\"skills\":[],\"experience\":[{\"company\":\"\",\"title\":\"\",\"duration\":\"\"}],\"education\":[{\"institution\":\"\",\"degree\":\"\",\"year\":\"\"}],\"yearsOfExperience\":0}" },
            new { role = "user", content = resumeText }
                },
                temperature = 0.1
            };

            var response = await SendAIRequestAsync(requestBody, cts.Token);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cts.Token);
            var content = payload
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content)) return null;

            // Robust JSON extraction
            var startIndex = content.IndexOf('{');
            var endIndex = content.LastIndexOf('}');
            if (startIndex >= 0 && endIndex > startIndex)
            {
                content = content.Substring(startIndex, endIndex - startIndex + 1);
            }
            
            var cleaned = content.Replace("```json", "").Replace("```", "").Trim();
            var parsed = JsonSerializer.Deserialize<ResumeParseResult>(cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed != null) parsed.ExtractedText = resumeText;
            return parsed;
        }

        /// <summary>
        /// Calls an OpenAI-compatible chat completions endpoint to extract skills as a JSON array.
        /// Throws on failure so the caller can fall back.
        /// </summary>
        private async Task<List<string>?> ExtractSkillsWithProviderAsync(string text)
        {
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var requestBody = new
            {
                model = _options.Model,
                messages = new object[]
                {
                    new { role = "system", content = "Extract a JSON array of technical and soft skills mentioned in the text. Respond ONLY with a JSON array of strings." },
                    new { role = "user", content = text }
                },
                temperature = 0.1
            };

            var response = await _httpClient.PostAsJsonAsync(_options.Endpoint, requestBody, cts.Token);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cts.Token);
            var content = payload
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content)) return null;

            var startIndex = content.IndexOf('[');
            var endIndex = content.LastIndexOf(']');
            if (startIndex >= 0 && endIndex > startIndex)
            {
                content = content.Substring(startIndex, endIndex - startIndex + 1);
            }

            var cleaned = content.Replace("```json", "").Replace("```", "").Trim();
            return JsonSerializer.Deserialize<List<string>>(cleaned);
        }

        /// <summary>
        /// Calls an OpenAI-compatible chat completions endpoint to generate a rich narrative
        /// feedback summary from interview scores and notes. Falls back to deterministic
        /// generation on failure.
        /// </summary>
        private async Task<AIFeedbackDto?> GenerateFeedbackWithProviderAsync(string interviewNotes,
            int technicalScore, int behavioralScore, int communicationScore)
        {
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var prompt = $@"You are an HR interview assessment analyst. Given the following interview data, generate a structured JSON response.

Interview Notes: {interviewNotes}
Technical Score: {technicalScore}/10
Behavioral Score: {behavioralScore}/10
Communication Score: {communicationScore}/10

Respond ONLY with JSON in this format:
{{""summary"":""..."",""strengths"":[""...""],""concerns"":[""...""],""recommendedDecision"":""Advance|Hold|Reject""}}";

            var requestBody = new
            {
                model = _options.Model,
                messages = new object[]
                {
                    new { role = "system", content = "You are an HR interview assessment analyst. Respond ONLY with valid JSON." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.3
            };

            var response = await _httpClient.PostAsJsonAsync(_options.Endpoint, requestBody, cts.Token);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cts.Token);
            var content = payload
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content)) return null;

            var startIndex = content.IndexOf('{');
            var endIndex = content.LastIndexOf('}');
            if (startIndex >= 0 && endIndex > startIndex)
            {
                content = content.Substring(startIndex, endIndex - startIndex + 1);
            }

            var cleaned = content.Replace("```json", "").Replace("```", "").Trim();
            var result = JsonSerializer.Deserialize<AIFeedbackDto>(cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result;
        }
        /// <summary>
        /// Sends a request to the AI provider with proper authentication and headers.
        /// </summary>
        private async Task<HttpResponseMessage> SendAIRequestAsync(object requestBody, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
            {
                Content = JsonContent.Create(requestBody)
            };

            // Set authentication header
            if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
            }

            // OpenRouter-specific headers
            if (_options.Provider?.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase) == true)
            {
                request.Headers.Add("HTTP-Referer", "https://recruitai.com");
                request.Headers.Add("X-Title", "RecruitAI Platform");
            }

            return await _httpClient.SendAsync(request, ct);
        }
    }
}
