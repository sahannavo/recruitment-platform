namespace RecruitmentAPI.Services.Notification
{
    /// <summary>Bind this to the "Notifications" section of appsettings.json.</summary>
    public class NotificationServiceOptions
    {
        public const string SectionName = "Notifications";

        public string SendGridApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "no-reply@recruitmentplatform.com";
        public string FromName { get; set; } = "Recruitment Platform";

        public string TwilioAccountSid { get; set; } = string.Empty;
        public string TwilioAuthToken { get; set; } = string.Empty;
        public string TwilioFromNumber { get; set; } = string.Empty;
    }
}
