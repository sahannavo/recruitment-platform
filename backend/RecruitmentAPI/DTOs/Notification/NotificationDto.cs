using System;

namespace RecruitmentAPI.DTOs.Notification
{
    public enum NotificationType
    {
        Email,
        Sms
    }

    /// <summary>Generic notification request payload.</summary>
    public class NotificationDto
    {
        public int UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? TemplateKey { get; set; }
        public object? TemplateData { get; set; }
    }

    public class NotificationResultDto
    {
        public bool Success { get; set; }
        public string? ProviderMessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
    }
}
