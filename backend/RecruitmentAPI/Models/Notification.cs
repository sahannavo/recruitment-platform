namespace RecruitmentAPI.Models;

/// <summary>
/// User notification record for email and in-app delivery tracking.
/// </summary>
public class Notification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string DeliveryStatus { get; set; } = "Pending";

    public User User { get; set; } = null!;
}

