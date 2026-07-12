namespace RecruitmentAPI.Services.AI
{
    /// <summary>Bind this to the "AIService" section of appsettings.json.</summary>
    public class AIServiceOptions
    {
        public const string SectionName = "AIService";

        public string Provider { get; set; } = "OpenAI"; // "OpenAI" | "AzureFormRecognizer" | "None"
        public string? ApiKey { get; set; }
        public string? Endpoint { get; set; }
        public string Model { get; set; } = "gpt-4o-mini";
        public int TimeoutSeconds { get; set; } = 20;
    }
}
