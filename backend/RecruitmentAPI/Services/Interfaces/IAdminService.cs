using RecruitmentAPI.DTOs;

namespace RecruitmentAPI.Services.Interfaces;

/// <summary>
/// Service contract for admin user management, role assignment,
/// user invitations, notifications, audit logging, and dashboard operations.
/// </summary>
public interface IAdminService
{
    // ── Admin profile CRUD ────────────────────────────────────────────────────

    /// <summary>Returns a single admin by their admin record ID.</summary>
    Task<AdminResponseDto> GetAdminByIdAsync(int adminId);

    /// <summary>Returns the admin record linked to a given user ID.</summary>
    Task<AdminResponseDto> GetAdminByUserIdAsync(int userId);

    /// <summary>Returns all admin profiles in the system.</summary>
    Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync();

    /// <summary>Creates a new admin user (User + Admin rows inside a transaction).</summary>
    Task<AdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request);

    /// <summary>Updates mutable fields on an existing admin profile.</summary>
    Task<AdminResponseDto> UpdateAdminAsync(int adminId, UpdateAdminRequestDto request);

    /// <summary>Deletes an admin user and their linked User row.</summary>
    Task DeleteAdminAsync(int adminId);

    // ── User management ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns all platform users as <see cref="UserManagementDto"/> objects,
    /// with optional role and active-status filters.
    /// </summary>
    Task<IEnumerable<UserManagementDto>> GetAllUsersAsync(string? role = null, bool? isActive = null);

    /// <summary>
    /// Changes a user's role.
    /// Promotion to <c>Admin</c> is only allowed when
    /// <paramref name="callerRole"/> equals <c>SuperAdmin</c>.
    /// An audit log entry is written for every successful change.
    /// </summary>
    /// <param name="request">Target user ID and desired new role.</param>
    /// <param name="performedByUserId">ID of the admin executing the action.</param>
    /// <param name="callerRole">Role of the admin executing the action.</param>
    Task<UserManagementDto> UpdateUserRoleAsync(
        UserRoleUpdateDto request,
        int performedByUserId,
        string callerRole);

    /// <summary>
    /// Disables a user account (sets IsActive = false).
    /// Writes an audit log entry.
    /// </summary>
    Task<UserManagementDto> DisableUserAsync(int userId, int performedByUserId);

    /// <summary>
    /// Re-enables a previously disabled user account (sets IsActive = true).
    /// Writes an audit log entry.
    /// </summary>
    Task<UserManagementDto> EnableUserAsync(int userId, int performedByUserId);

    /// <summary>
    /// Permanently deletes a user account.
    /// Writes an audit log entry.
    /// </summary>
    Task DeleteUserAsync(int userId, int performedByUserId);

    // ── User invitation ───────────────────────────────────────────────────────

    /// <summary>
    /// Generates a time-limited invite token, persists a pending user record,
    /// and queues a notification email containing the registration link.
    /// </summary>
    Task<InviteUserResponseDto> InviteUserAsync(InviteUserRequestDto request, int performedByUserId);

    // ── Notifications ─────────────────────────────────────────────────────────

    /// <summary>Sends a notification to a single user and returns the created record.</summary>
    Task<NotificationResponseDto> SendNotificationAsync(SendNotificationRequestDto request);

    /// <summary>Returns notifications with optional user-ID and delivery-status filters.</summary>
    Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync(
        int? userId = null,
        string? deliveryStatus = null);

    /// <summary>Updates the delivery status of a notification record.</summary>
    Task<NotificationResponseDto> UpdateNotificationStatusAsync(int notificationId, string deliveryStatus);

    // ── Audit logs ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns audit log entries, optionally scoped by the actor or entity type.
    /// </summary>
    Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(
        int? performedByUserId = null,
        string? entityType = null);

    // ── Dashboard ─────────────────────────────────────────────────────────────

    /// <summary>Returns aggregate counts and statistics for the admin dashboard.</summary>
    Task<AdminDashboardDto> GetDashboardAsync();
}

