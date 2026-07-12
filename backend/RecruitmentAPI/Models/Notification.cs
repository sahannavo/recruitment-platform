using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentPlatform.Models
{
    /// <summary>
    /// Represents a notification (email, SMS, in-app) sent to a user.
    /// Used by AdminService for invite emails and by the wider platform for
    /// application status updates, interview reminders, etc.
    /// </summary>
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        /// <summary>Email, SMS, InApp, Push</summary>
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>Pending, Sent, Failed, Delivered</summary>
        [Required]
        [MaxLength(50)]
        public string DeliveryStatus { get; set; } = "Pending";

        // Navigation property - assumes User entity exists in another module (Users base table)
        public virtual User? User { get; set; }
    }
}

