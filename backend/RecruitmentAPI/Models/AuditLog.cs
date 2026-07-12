using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentPlatform.Models
{
    /// <summary>
    /// Tracks who did what and when for admin/privileged actions
    /// (role changes, user disable/enable/delete, invites).
    /// Not explicitly listed in the reference table list, but required
    /// by the "audit log should track all admin actions" requirement.
    /// </summary>
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        [Required]
        public int PerformedByUserId { get; set; }

        /// <summary>e.g. "RoleUpdated", "UserDisabled", "UserEnabled", "UserDeleted", "UserInvited"</summary>
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        /// <summary>e.g. "User#42"</summary>
        [MaxLength(200)]
        public string? EntityAffected { get; set; }

        /// <summary>Free-form JSON/text describing the change (old value -> new value)</summary>
        public string? Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(PerformedByUserId))]
        public virtual User? PerformedByUser { get; set; }
    }
}

