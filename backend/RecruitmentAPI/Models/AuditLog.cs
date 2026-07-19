using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Models;

public class AuditLog
{
    [Key]
    public int AuditLogId { get; set; }

    [Required]
    public int PerformedByUserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [StringLength(2000)]
    public string? Details { get; set; }

    /// <summary> IP Address for audit trail</summary>
    [StringLength(45)]
    public string? IpAddress { get; set; }

    /// <summary> User Agent for audit trail</summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>When the action was performed</summary>
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("PerformedByUserId")]
    public virtual User? PerformedBy { get; set; }
}