using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Controllers;

/// <summary>
/// Admin management endpoints: user administration, role assignment,
/// user invitations, notifications, audit logs, and dashboard.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger       = logger;
    }

    // =========================================================================
    // Dashboard
    // =========================================================================

    /// <summary>
    /// Returns an overview dashboard with user counts and notification statistics.
    /// </summary>
    /// <response code="200">Dashboard data returned successfully.</response>
    /// <response code="401">Caller is not authenticated.</response>
    /// <response code="403">Caller lacks admin privileges.</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Ok(dashboard);
    }

    // =========================================================================
    // User management  –  GET /api/admin/users
    // =========================================================================

    /// <summary>
    /// Returns all platform users as full management objects.
    /// Supports optional filtering by role and active status.
    /// </summary>
    /// <param name="role">Optional role filter (Candidate, Recruiter, HiringManager, Admin).</param>
    /// <param name="isActive">Optional active-status filter.</param>
    /// <response code="200">User list returned successfully.</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserManagementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetAllUsers(
        [FromQuery] string? role,
        [FromQuery] bool?   isActive)
    {
        var users = await _adminService.GetAllUsersAsync(role, isActive);
        return Ok(users);
    }

    // =========================================================================
    // Role update  –  PUT /api/admin/users/:id/role
    // =========================================================================

    /// <summary>
    /// Changes the role of the specified user.
    /// Promoting to <c>Admin</c> requires the caller to have the <c>SuperAdmin</c> role.
    /// An audit log entry is written on every successful change.
    /// </summary>
    /// <param name="id">ID of the user whose role will be changed.</param>
    /// <param name="dto">New role details.</param>
    /// <response code="200">Role updated successfully.</response>
    /// <response code="400">Invalid role value.</response>
    /// <response code="403">Caller is not a SuperAdmin and attempted to assign Admin role.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("users/{id:int}/role")]
    [ProducesResponseType(typeof(UserManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserManagementDto>> UpdateUserRole(
        int id,
        [FromBody] UserRoleUpdateDto dto)
    {
        // Override UserId with the route id to prevent body/route mismatch
        dto.UserId = id;

        var performedBy = GetCallerUserId();
        var callerRole  = GetCallerRole();

        var updated = await _adminService.UpdateUserRoleAsync(dto, performedBy, callerRole);

        _logger.LogInformation(
            "User {UserId} role updated to {NewRole} by admin {AdminId}",
            id, dto.NewRole, performedBy);

        return Ok(updated);
    }

    // =========================================================================
    // Disable / Enable  –  PUT /api/admin/users/:id/disable|enable
    // =========================================================================

    /// <summary>
    /// Disables the specified user account (sets IsActive = false).
    /// An audit log entry is written.
    /// </summary>
    /// <param name="id">User ID to disable.</param>
    /// <response code="200">User disabled successfully.</response>
    /// <response code="400">User is already inactive.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("users/{id:int}/disable")]
    [ProducesResponseType(typeof(UserManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserManagementDto>> DisableUser(int id)
    {
        var performedBy = GetCallerUserId();
        var user        = await _adminService.DisableUserAsync(id, performedBy);

        _logger.LogInformation("User {UserId} disabled by admin {AdminId}", id, performedBy);

        return Ok(user);
    }

    /// <summary>
    /// Re-enables the specified user account (sets IsActive = true).
    /// An audit log entry is written.
    /// </summary>
    /// <param name="id">User ID to enable.</param>
    /// <response code="200">User enabled successfully.</response>
    /// <response code="400">User is already active.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("users/{id:int}/enable")]
    [ProducesResponseType(typeof(UserManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserManagementDto>> EnableUser(int id)
    {
        var performedBy = GetCallerUserId();
        var user        = await _adminService.EnableUserAsync(id, performedBy);

        _logger.LogInformation("User {UserId} enabled by admin {AdminId}", id, performedBy);

        return Ok(user);
    }

    // =========================================================================
    // Delete user  –  DELETE /api/admin/users/:id
    // =========================================================================

    /// <summary>
    /// Permanently deletes the specified user account.
    /// An audit log entry is written before deletion.
    /// </summary>
    /// <param name="id">User ID to delete.</param>
    /// <response code="204">User deleted successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpDelete("users/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var performedBy = GetCallerUserId();
        await _adminService.DeleteUserAsync(id, performedBy);

        _logger.LogWarning("User {UserId} deleted by admin {AdminId}", id, performedBy);

        return NoContent();
    }

    // =========================================================================
    // Invite user  –  POST /api/admin/users/invite
    // =========================================================================

    /// <summary>
    /// Issues an invitation to a new user by email.
    /// Creates a placeholder account, generates a time-limited registration token,
    /// and queues an invite notification email.
    /// </summary>
    /// <param name="request">Invitee details (email, name, intended role).</param>
    /// <response code="201">Invitation created and queued successfully.</response>
    /// <response code="400">Email already registered or invalid role.</response>
    [HttpPost("users/invite")]
    [ProducesResponseType(typeof(InviteUserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InviteUserResponseDto>> InviteUser(
        [FromBody] InviteUserRequestDto request)
    {
        var performedBy = GetCallerUserId();
        var result      = await _adminService.InviteUserAsync(request, performedBy);

        _logger.LogInformation(
            "Invite issued to {Email} (role: {Role}) by admin {AdminId}",
            request.Email, request.Role, performedBy);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    // =========================================================================
    // Admin profile CRUD  (internal admin management)
    // =========================================================================

    /// <summary>Returns all admin profiles.</summary>
    /// <response code="200">Admin list returned.</response>
    [HttpGet("admins")]
    [ProducesResponseType(typeof(IEnumerable<AdminResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AdminResponseDto>>> GetAllAdmins()
    {
        var admins = await _adminService.GetAllAdminsAsync();
        return Ok(admins);
    }

    /// <summary>Returns a single admin by admin record ID.</summary>
    /// <param name="id">Admin record ID.</param>
    /// <response code="200">Admin found.</response>
    /// <response code="404">Admin not found.</response>
    [HttpGet("admins/{id:int}")]
    [ProducesResponseType(typeof(AdminResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminResponseDto>> GetAdminById(int id)
    {
        var admin = await _adminService.GetAdminByIdAsync(id);
        return Ok(admin);
    }

    /// <summary>Creates a new admin user account (User + Admin rows).</summary>
    /// <param name="request">Admin creation details.</param>
    /// <response code="201">Admin created successfully.</response>
    /// <response code="400">Validation failed or email already exists.</response>
    [HttpPost("admins")]
    [ProducesResponseType(typeof(AdminResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminResponseDto>> CreateAdmin(
        [FromBody] CreateAdminRequestDto request)
    {
        var admin = await _adminService.CreateAdminAsync(request);

        _logger.LogInformation("Admin created via API: AdminId {AdminId}", admin.AdminId);

        return CreatedAtAction(nameof(GetAdminById), new { id = admin.AdminId }, admin);
    }

    /// <summary>Updates mutable fields on an existing admin profile.</summary>
    /// <param name="id">Admin record ID.</param>
    /// <param name="request">Fields to update.</param>
    /// <response code="200">Admin updated.</response>
    /// <response code="404">Admin not found.</response>
    [HttpPut("admins/{id:int}")]
    [ProducesResponseType(typeof(AdminResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminResponseDto>> UpdateAdmin(
        int id,
        [FromBody] UpdateAdminRequestDto request)
    {
        var admin = await _adminService.UpdateAdminAsync(id, request);
        return Ok(admin);
    }

    /// <summary>Deletes an admin user and their linked User row.</summary>
    /// <param name="id">Admin record ID.</param>
    /// <response code="204">Admin deleted.</response>
    /// <response code="404">Admin not found.</response>
    [HttpDelete("admins/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        await _adminService.DeleteAdminAsync(id);
        return NoContent();
    }

    // =========================================================================
    // Notifications
    // =========================================================================

    /// <summary>
    /// Sends a notification to the specified user and queues it for delivery.
    /// </summary>
    /// <param name="request">Notification details (userId, type, subject, content).</param>
    /// <response code="201">Notification queued successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Target user not found.</response>
    [HttpPost("notifications")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> SendNotification(
        [FromBody] SendNotificationRequestDto request)
    {
        var notification = await _adminService.SendNotificationAsync(request);
        return StatusCode(StatusCodes.Status201Created, notification);
    }

    /// <summary>
    /// Returns notifications with optional user-ID and delivery-status filters.
    /// </summary>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="deliveryStatus">Optional status filter (Pending, Delivered, Failed).</param>
    /// <response code="200">Notifications returned.</response>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications(
        [FromQuery] int?    userId,
        [FromQuery] string? deliveryStatus)
    {
        var notifications = await _adminService.GetNotificationsAsync(userId, deliveryStatus);
        return Ok(notifications);
    }

    /// <summary>Updates the delivery status of a notification.</summary>
    /// <param name="id">Notification ID.</param>
    /// <param name="deliveryStatus">New delivery status (Pending, Delivered, Failed).</param>
    /// <response code="200">Status updated.</response>
    /// <response code="400">Status value missing.</response>
    /// <response code="404">Notification not found.</response>
    [HttpPatch("notifications/{id:int}/status")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> UpdateNotificationStatus(
        int id,
        [FromQuery] string deliveryStatus)
    {
        var notification = await _adminService.UpdateNotificationStatusAsync(id, deliveryStatus);
        return Ok(notification);
    }

    // =========================================================================
    // Audit logs
    // =========================================================================

    /// <summary>
    /// Returns the admin audit trail.
    /// Optionally scoped by the actor's user ID or the entity type.
    /// </summary>
    /// <param name="performedByUserId">Filter by the admin who performed the action.</param>
    /// <param name="entityType">Filter by entity type (e.g. "User", "Admin").</param>
    /// <response code="200">Audit log entries returned.</response>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(IEnumerable<AuditLogResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetAuditLogs(
        [FromQuery] int?    performedByUserId,
        [FromQuery] string? entityType)
    {
        var logs = await _adminService.GetAuditLogsAsync(performedByUserId, entityType);
        return Ok(logs);
    }

    // =========================================================================
    // JWT claim helpers
    // =========================================================================

    /// <summary>Extracts the caller's UserId from the JWT NameIdentifier claim.</summary>
    private int GetCallerUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : 0;
    }

    /// <summary>Extracts the caller's Role from the JWT Role claim.</summary>
    private string GetCallerRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}

