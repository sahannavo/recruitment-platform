namespace RecruitmentAPI.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// Admin Profile DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Response DTO for admin profile details.
/// </summary>
public class AdminResponseDto
{
    public int AdminId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a new admin user.
/// </summary>
public class CreateAdminRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for updating an existing admin user.
/// </summary>
public class UpdateAdminRequestDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public string? Permissions { get; set; }
    public bool? IsActive { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// User Management DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Comprehensive user management DTO used in admin user listings and detail views.
/// Includes all fields needed for user administration tasks.
/// </summary>
public class UserManagementDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Lightweight user summary used in paginated admin tables.
/// </summary>
public class UserSummaryDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for changing a user's role.
/// Only SuperAdmin callers may promote a user to the Admin role.
/// </summary>
public class UserRoleUpdateDto
{
    /// <summary>ID of the user whose role will be changed.</summary>
    public int UserId { get; set; }

    /// <summary>
    /// Target role. Allowed values: Candidate, Recruiter, HiringManager, Admin.
    /// Promotion to Admin requires SuperAdmin privileges.
    /// </summary>
    public string NewRole { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
// Invite DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request DTO for inviting a new user to the platform via email.
/// </summary>
public class InviteUserRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Intended role for the invitee (Candidate, Recruiter, HiringManager).</summary>
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO returned after a user invite is issued.
/// </summary>
public class InviteUserResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string InviteToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
// Notification DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request DTO for sending a notification to a user.
/// </summary>
public class SendNotificationRequestDto
{
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for notification details.
/// </summary>
public class NotificationResponseDto
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
// Audit Log DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Response DTO for a single audit log entry.
/// </summary>
public class AuditLogResponseDto
{
    public int AuditLogId { get; set; }
    public int PerformedByUserId { get; set; }
    public string PerformedByEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime PerformedAt { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Dashboard DTO
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Response DTO for admin dashboard overview statistics.
/// </summary>
public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public int TotalNotificationsSent { get; set; }
    public int PendingNotifications { get; set; }
}

public class PlatformSettingsDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? OpenAIKey { get; set; }
    public string? AWSKey { get; set; }
    public string? EmailTemplate { get; set; }
    public decimal Creativity { get; set; }
    public decimal Precision { get; set; }
    public decimal Penalty { get; set; }
    public bool SystemAlerts { get; set; }
    public bool WeeklyReport { get; set; }
    public bool ApiWarnings { get; set; }
}

public class PublicPlatformSettingsDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
}
