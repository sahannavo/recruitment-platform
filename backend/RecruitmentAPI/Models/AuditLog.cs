namespace RecruitmentAPI.Models;

/// <summary>
/// Tracks every significant admin action: who did what, to which entity, and when.
/// Immutable by convention — entries are never updated or deleted.
/// </summary>
public class AuditLog
{
    public int AuditLogId { get; set; }

    /// <summary>ID of the admin user who performed the action.</summary>
    public int PerformedByUserId { get; set; }

    /// <summary>
    /// Short action label, e.g. "UserRoleUpdated", "UserDisabled",
    /// "UserInvited", "UserDeleted", "AdminCreated".
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The entity type the action was applied to,
    /// e.g. "User", "Admin", "JobPosting".
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the affected entity (nullable for bulk actions).</summary>
    public int? EntityId { get; set; }

    /// <summary>Free-text detail, JSON payload, or human-readable description.</summary>
    public string? Details { get; set; }

    /// <summary>UTC timestamp when the action was recorded.</summary>
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User PerformedBy { get; set; } = null!;
}

