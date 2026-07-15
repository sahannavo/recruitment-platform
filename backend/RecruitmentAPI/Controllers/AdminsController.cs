using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Controllers;

/// <summary>
/// Admin management endpoints for user administration, notifications, and dashboard.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminsController> _logger;

    public AdminsController(IAdminService adminService, ILogger<AdminsController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the admin dashboard overview with user and notification statistics.
    /// </summary>
    /// <returns>Dashboard summary data.</returns>
    /// <response code="200">Dashboard data retrieved successfully.</response>
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

    /// <summary>
    /// Gets all admin users.
    /// </summary>
    /// <returns>List of admin profiles.</returns>
    /// <response code="200">Admins retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AdminResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AdminResponseDto>>> GetAllAdmins()
    {
        var admins = await _adminService.GetAllAdminsAsync();
        return Ok(admins);
    }

    /// <summary>
    /// Gets an admin by their admin ID.
    /// </summary>
    /// <param name="id">Admin ID.</param>
    /// <returns>Admin profile details.</returns>
    /// <response code="200">Admin found.</response>
    /// <response code="404">Admin not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AdminResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminResponseDto>> GetAdminById(int id)
    {
        var admin = await _adminService.GetAdminByIdAsync(id);
        return Ok(admin);
    }

    /// <summary>
    /// Creates a new admin user account.
    /// </summary>
    /// <param name="request">Admin creation details.</param>
    /// <returns>The created admin profile.</returns>
    /// <response code="201">Admin created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AdminResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminResponseDto>> CreateAdmin([FromBody] CreateAdminRequestDto request)
    {
        var admin = await _adminService.CreateAdminAsync(request);
        _logger.LogInformation("Admin created via API: AdminId {AdminId}", admin.AdminId);
        return CreatedAtAction(nameof(GetAdminById), new { id = admin.AdminId }, admin);
    }

    /// <summary>
    /// Updates an existing admin profile.
    /// </summary>
    /// <param name="id">Admin ID.</param>
    /// <param name="request">Fields to update.</param>
    /// <returns>Updated admin profile.</returns>
    /// <response code="200">Admin updated successfully.</response>
    /// <response code="404">Admin not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AdminResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminResponseDto>> UpdateAdmin(int id, [FromBody] UpdateAdminRequestDto request)
    {
        var admin = await _adminService.UpdateAdminAsync(id, request);
        return Ok(admin);
    }

    /// <summary>
    /// Deletes an admin user and their linked account.
    /// </summary>
    /// <param name="id">Admin ID.</param>
    /// <response code="204">Admin deleted successfully.</response>
    /// <response code="404">Admin not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        await _adminService.DeleteAdminAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Gets all platform users with optional role and active-status filters.
    /// </summary>
    /// <param name="role">Optional role filter (e.g. Candidate, Recruiter).</param>
    /// <param name="isActive">Optional active status filter.</param>
    /// <returns>List of user summaries.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetAllUsers(
        [FromQuery] string? role,
        [FromQuery] bool? isActive)
    {
        var users = await _adminService.GetAllUsersAsync(role, isActive);
        return Ok(users);
    }

    /// <summary>
    /// Disables a user account (sets IsActive = false). An audit log entry is written.
    /// </summary>
    /// <param name="userId">User ID to disable.</param>
    /// <response code="200">User disabled successfully.</response>
    /// <response code="400">User is already inactive.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("users/{userId:int}/disable")]
    [ProducesResponseType(typeof(UserManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserManagementDto>> DisableUser(int userId)
    {
        // performedByUserId = 0 when called without a JWT claim (should not happen in prod)
        var user = await _adminService.DisableUserAsync(userId, performedByUserId: 0);
        return Ok(user);
    }

    /// <summary>
    /// Re-enables a user account (sets IsActive = true). An audit log entry is written.
    /// </summary>
    /// <param name="userId">User ID to enable.</param>
    /// <response code="200">User enabled successfully.</response>
    /// <response code="400">User is already active.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("users/{userId:int}/enable")]
    [ProducesResponseType(typeof(UserManagementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserManagementDto>> EnableUser(int userId)
    {
        var user = await _adminService.EnableUserAsync(userId, performedByUserId: 0);
        return Ok(user);
    }

    /// <summary>
    /// Sends a notification to a user.
    /// </summary>
    /// <param name="request">Notification details.</param>
    /// <returns>The created notification record.</returns>
    /// <response code="201">Notification queued successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Target user not found.</response>
    [HttpPost("notifications")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> SendNotification([FromBody] SendNotificationRequestDto request)
    {
        var notification = await _adminService.SendNotificationAsync(request);
        return CreatedAtAction(nameof(GetNotifications), new { userId = notification.UserId }, notification);
    }

    /// <summary>
    /// Gets notifications with optional user and delivery-status filters.
    /// </summary>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="deliveryStatus">Optional delivery status filter (Pending, Delivered, Failed).</param>
    /// <returns>List of notifications.</returns>
    /// <response code="200">Notifications retrieved successfully.</response>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications(
        [FromQuery] int? userId,
        [FromQuery] string? deliveryStatus)
    {
        var notifications = await _adminService.GetNotificationsAsync(userId, deliveryStatus);
        return Ok(notifications);
    }

    /// <summary>
    /// Updates the delivery status of a notification.
    /// </summary>
    /// <param name="notificationId">Notification ID.</param>
    /// <param name="deliveryStatus">New delivery status.</param>
    /// <returns>Updated notification.</returns>
    /// <response code="200">Status updated successfully.</response>
    /// <response code="404">Notification not found.</response>
    [HttpPatch("notifications/{notificationId:int}/status")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> UpdateNotificationStatus(
        int notificationId,
        [FromQuery] string deliveryStatus)
    {
        var notification = await _adminService.UpdateNotificationStatusAsync(notificationId, deliveryStatus);
        return Ok(notification);
    }
}
