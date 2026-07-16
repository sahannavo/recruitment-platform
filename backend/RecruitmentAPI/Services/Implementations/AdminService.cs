using System.Security.Cryptography;
using System.Text;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Exceptions;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Interfaces;
using Notification = RecruitmentAPI.Models.Notification;

namespace RecruitmentAPI.Services.Implementations;

/// <summary>
/// Admin service handling user management, role assignment, user invitations,
/// notifications, audit logging, and dashboard statistics.
/// </summary>
public class AdminService : IAdminService
{
    private static readonly string[] AllowedRoles =
        ["Candidate", "Recruiter", "HiringManager", "Admin"];

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IUnitOfWork unitOfWork, ILogger<AdminService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // =========================================================================
    // Admin profile CRUD
    // =========================================================================

    /// <inheritdoc />
    public async Task<AdminResponseDto> GetAdminByIdAsync(int adminId)
    {
        var admin = await _unitOfWork.Admins.GetByIdAsync(adminId)
            ?? throw new NotFoundException($"Admin with ID {adminId} was not found.");

        var user = await _unitOfWork.Users.GetByIdAsync(admin.UserId)
            ?? throw new NotFoundException($"User linked to admin {adminId} was not found.");

        return MapToAdminResponse(admin, user);
    }

    /// <inheritdoc />
    public async Task<AdminResponseDto> GetAdminByUserIdAsync(int userId)
    {
        var admin = await _unitOfWork.Admins.FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new NotFoundException($"Admin record for user ID {userId} was not found.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} was not found.");

        return MapToAdminResponse(admin, user);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync()
    {
        var admins = await _unitOfWork.Admins.GetAllAsync();
        var results = new List<AdminResponseDto>();

        foreach (var admin in admins)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(admin.UserId);
            if (user is not null)
                results.Add(MapToAdminResponse(admin, user));
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<AdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request)
    {
        ValidateCreateAdminRequest(request);

        if (await _unitOfWork.Users.AnyAsync(u => u.Email == request.Email))
            throw new BadRequestException($"A user with email '{request.Email}' already exists.");

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var user = new User
            {
                Email    = request.Email,
                PasswordHash = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName  = request.LastName,
                Role      = "Admin",
                IsActive  = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var admin = new Admin
            {
                UserId      = user.UserId,
                Department  = request.Department,
                Permissions = request.Permissions
            };

            await _unitOfWork.Admins.AddAsync(admin);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Admin created: {Email} (AdminId: {AdminId})", request.Email, admin.AdminId);

            return MapToAdminResponse(admin, user);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<AdminResponseDto> UpdateAdminAsync(int adminId, UpdateAdminRequestDto request)
    {
        var admin = await _unitOfWork.Admins.GetByIdAsync(adminId)
            ?? throw new NotFoundException($"Admin with ID {adminId} was not found.");

        var user = await _unitOfWork.Users.GetByIdAsync(admin.UserId)
            ?? throw new NotFoundException($"User linked to admin {adminId} was not found.");

        if (request.FirstName  is not null) user.FirstName       = request.FirstName;
        if (request.LastName   is not null) user.LastName        = request.LastName;
        if (request.Department is not null) admin.Department     = request.Department;
        if (request.Permissions is not null) admin.Permissions  = request.Permissions;
        if (request.IsActive.HasValue)      user.IsActive       = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        _unitOfWork.Admins.Update(admin);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Admin updated: AdminId {AdminId}", adminId);

        return MapToAdminResponse(admin, user);
    }

    /// <inheritdoc />
    public async Task DeleteAdminAsync(int adminId)
    {
        var admin = await _unitOfWork.Admins.GetByIdAsync(adminId)
            ?? throw new NotFoundException($"Admin with ID {adminId} was not found.");

        var user = await _unitOfWork.Users.GetByIdAsync(admin.UserId);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            _unitOfWork.Admins.Remove(admin);
            if (user is not null)
                _unitOfWork.Users.Remove(user);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Admin deleted: AdminId {AdminId}", adminId);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    // =========================================================================
    // User management
    // =========================================================================

    /// <inheritdoc />
    public async Task<IEnumerable<UserManagementDto>> GetAllUsersAsync(
        string? role = null,
        bool? isActive = null)
    {
        var users = string.IsNullOrWhiteSpace(role)
            ? await _unitOfWork.AdminRepository.GetAllUsersAsync()
            : await _unitOfWork.AdminRepository.GetUsersByRoleAsync(role);

        var filtered = users.AsEnumerable();

        if (isActive.HasValue)
            filtered = filtered.Where(u => u.IsActive == isActive.Value);

        return filtered.Select(MapToUserManagement);
    }

    /// <inheritdoc />
    public async Task<UserManagementDto> UpdateUserRoleAsync(
        UserRoleUpdateDto request,
        int performedByUserId,
        string callerRole)
    {
        if (string.IsNullOrWhiteSpace(request.NewRole) ||
            !AllowedRoles.Contains(request.NewRole, StringComparer.OrdinalIgnoreCase))
            throw new BadRequestException(
                $"Invalid role '{request.NewRole}'. Allowed values: {string.Join(", ", AllowedRoles)}.");

        // Only SuperAdmin can promote someone to Admin
        if (request.NewRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
            !callerRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("Only a SuperAdmin can assign the Admin role.");

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User with ID {request.UserId} was not found.");

        var previousRole = user.Role;

        await _unitOfWork.AdminRepository.UpdateUserRoleAsync(request.UserId, request.NewRole);

        await WriteAuditLogAsync(
            performedByUserId,
            action: "UserRoleUpdated",
            entityType: "User",
            entityId: request.UserId,
            details: $"Role changed from '{previousRole}' to '{request.NewRole}'.");

        _logger.LogInformation(
            "User {UserId} role changed from {OldRole} to {NewRole} by admin {AdminId}",
            request.UserId, previousRole, request.NewRole, performedByUserId);

        // Re-read to return current state
        var updated = await _unitOfWork.Users.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User with ID {request.UserId} was not found after update.");

        return MapToUserManagement(updated);
    }

    /// <inheritdoc />
    public async Task<UserManagementDto> DisableUserAsync(int userId, int performedByUserId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} was not found.");

        if (!user.IsActive)
            throw new BadRequestException($"User {userId} is already inactive.");

        await _unitOfWork.AdminRepository.DisableUserAsync(userId);

        await WriteAuditLogAsync(
            performedByUserId,
            action: "UserDisabled",
            entityType: "User",
            entityId: userId,
            details: $"Account disabled for '{user.Email}'.");

        _logger.LogInformation("User {UserId} disabled by admin {AdminId}", userId, performedByUserId);

        var updated = await _unitOfWork.Users.GetByIdAsync(userId)!;
        return MapToUserManagement(updated!);
    }

    /// <inheritdoc />
    public async Task<UserManagementDto> EnableUserAsync(int userId, int performedByUserId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} was not found.");

        if (user.IsActive)
            throw new BadRequestException($"User {userId} is already active.");

        await _unitOfWork.AdminRepository.EnableUserAsync(userId);

        await WriteAuditLogAsync(
            performedByUserId,
            action: "UserEnabled",
            entityType: "User",
            entityId: userId,
            details: $"Account re-enabled for '{user.Email}'.");

        _logger.LogInformation("User {UserId} enabled by admin {AdminId}", userId, performedByUserId);

        var updated = await _unitOfWork.Users.GetByIdAsync(userId)!;
        return MapToUserManagement(updated!);
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(int userId, int performedByUserId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} was not found.");

        // Write audit log before deleting so the FK is still valid
        await WriteAuditLogAsync(
            performedByUserId,
            action: "UserDeleted",
            entityType: "User",
            entityId: userId,
            details: $"User '{user.Email}' (role: {user.Role}) permanently deleted.");

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            _unitOfWork.Users.Remove(user);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        _logger.LogWarning(
            "User {UserId} ({Email}) permanently deleted by admin {AdminId}",
            userId, user.Email, performedByUserId);
    }

    // =========================================================================
    // User invitation
    // =========================================================================

    /// <inheritdoc />
    public async Task<InviteUserResponseDto> InviteUserAsync(
        InviteUserRequestDto request,
        int performedByUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Role) ||
            !AllowedRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            throw new BadRequestException(
                $"Invalid role '{request.Role}'. Allowed values: {string.Join(", ", AllowedRoles)}.");

        if (await _unitOfWork.AdminRepository.GetUserByEmailAsync(request.Email) is not null)
            throw new BadRequestException($"A user with email '{request.Email}' already exists.");

        // Generate a cryptographically random invite token (URL-safe base64)
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token      = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        var expiresAt = DateTime.UtcNow.AddHours(48);

        // Create a placeholder user with a random unusable password hash,
        // IsActive = false until registration is completed.
        var placeholderHash = HashPassword(Guid.NewGuid().ToString());

        var user = new User
        {
            Email        = request.Email,
            PasswordHash = placeholderHash,
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Role         = request.Role,
            IsActive     = false,
            CreatedAt    = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Queue invite notification
        var notification = new Models.Notification
        {
            UserId  = user.UserId,
            Type    = "InviteEmail",
            Subject = "You have been invited to the Recruitment Portal",
            Content = $"Hello {request.FirstName},\n\n" +
                      $"You have been invited to join the Recruitment Portal as a {request.Role}.\n\n" +
                      $"Please complete your registration using the link below (expires in 48 hours):\n" +
                      $"https://portal.example.com/register?token={token}\n\n" +
                      $"If you did not expect this invitation, please ignore this message.",
            SentAt         = DateTime.UtcNow,
            DeliveryStatus = "Pending"
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        await WriteAuditLogAsync(
            performedByUserId,
            action: "UserInvited",
            entityType: "User",
            entityId: user.UserId,
            details: $"Invite sent to '{request.Email}' for role '{request.Role}'. Token expires at {expiresAt:u}.");

        _logger.LogInformation(
            "Invite issued to {Email} (role: {Role}) by admin {AdminId}",
            request.Email, request.Role, performedByUserId);

        return new InviteUserResponseDto
        {
            Email       = request.Email,
            Role        = request.Role,
            InviteToken = token,
            ExpiresAt   = expiresAt,
            Message     = $"Invitation email queued for {request.Email}."
        };
    }

    // =========================================================================
    // Notifications
    // =========================================================================

    /// <inheritdoc />
    public async Task<NotificationResponseDto> SendNotificationAsync(SendNotificationRequestDto request)
    {
        ValidateNotificationRequest(request);

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User with ID {request.UserId} was not found.");

        var notification = new Models.Notification
        {
            UserId         = request.UserId,
            Type           = request.Type,
            Subject        = request.Subject,
            Content        = request.Content,
            SentAt         = DateTime.UtcNow,
            DeliveryStatus = "Pending"
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Notification {NotificationId} queued for user {UserId}",
            notification.NotificationId, request.UserId);

        return MapToNotificationResponse(notification, user);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync(
        int? userId = null,
        string? deliveryStatus = null)
    {
        var notifications = await _unitOfWork.Notifications.GetAllAsync();
        var filtered      = notifications.AsEnumerable();

        if (userId.HasValue)
            filtered = filtered.Where(n => n.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(deliveryStatus))
            filtered = filtered.Where(n =>
                n.DeliveryStatus.Equals(deliveryStatus, StringComparison.OrdinalIgnoreCase));

        var results = new List<NotificationResponseDto>();

        foreach (var notification in filtered.OrderByDescending(n => n.SentAt))
        {
            var user = await _unitOfWork.Users.GetByIdAsync(notification.UserId);
            if (user is not null)
                results.Add(MapToNotificationResponse(notification, user));
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<NotificationResponseDto> UpdateNotificationStatusAsync(
        int notificationId,
        string deliveryStatus)
    {
        if (string.IsNullOrWhiteSpace(deliveryStatus))
            throw new BadRequestException("Delivery status is required.");

        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId)
            ?? throw new NotFoundException($"Notification with ID {notificationId} was not found.");

        var user = await _unitOfWork.Users.GetByIdAsync(notification.UserId)
            ?? throw new NotFoundException(
                $"User linked to notification {notificationId} was not found.");

        notification.DeliveryStatus = deliveryStatus;

        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        return MapToNotificationResponse(notification, user);
    }

    // =========================================================================
    // Audit logs
    // =========================================================================

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(
        int? performedByUserId = null,
        string? entityType = null)
    {
        var logs = await _unitOfWork.AdminRepository
            .GetAuditLogsAsync(performedByUserId, entityType);

        var results = new List<AuditLogResponseDto>();

        foreach (var log in logs)
        {
            var actor = await _unitOfWork.Users.GetByIdAsync(log.PerformedByUserId);
            results.Add(MapToAuditLogResponse(log, actor));
        }

        return results;
    }

    // =========================================================================
    // Dashboard
    // =========================================================================

    /// <inheritdoc />
    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var users         = (await _unitOfWork.Users.GetAllAsync()).ToList();
        var notifications = (await _unitOfWork.Notifications.GetAllAsync()).ToList();

        return new AdminDashboardDto
        {
            TotalUsers              = users.Count,
            ActiveUsers             = users.Count(u => u.IsActive),
            InactiveUsers           = users.Count(u => !u.IsActive),
            UsersByRole             = users
                .GroupBy(u => u.Role)
                .ToDictionary(g => g.Key, g => g.Count()),
            TotalNotificationsSent  = notifications.Count(n => n.DeliveryStatus == "Delivered"),
            PendingNotifications    = notifications.Count(n => n.DeliveryStatus == "Pending")
        };
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private async Task WriteAuditLogAsync(
        int performedByUserId,
        string action,
        string entityType,
        int? entityId,
        string? details)
    {
        await _unitOfWork.AdminRepository.AddAuditLogAsync(new AuditLog
        {
            PerformedByUserId = performedByUserId,
            Action            = action,
            EntityType        = entityType,
            EntityId          = entityId,
            Details           = details,
            PerformedAt       = DateTime.UtcNow
        });
    }

    private static void ValidateCreateAdminRequest(CreateAdminRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new BadRequestException("Password must be at least 8 characters.");
        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new BadRequestException("First name is required.");
        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new BadRequestException("Last name is required.");
        if (string.IsNullOrWhiteSpace(request.Department))
            throw new BadRequestException("Department is required.");
    }

    private static void ValidateNotificationRequest(SendNotificationRequestDto request)
    {
        if (request.UserId <= 0)
            throw new BadRequestException("A valid user ID is required.");
        if (string.IsNullOrWhiteSpace(request.Type))
            throw new BadRequestException("Notification type is required.");
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new BadRequestException("Notification subject is required.");
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BadRequestException("Notification content is required.");
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static AdminResponseDto MapToAdminResponse(Admin admin, User user) => new()
    {
        AdminId     = admin.AdminId,
        UserId      = user.UserId,
        Email       = user.Email,
        FirstName   = user.FirstName,
        LastName    = user.LastName,
        Department  = admin.Department,
        Permissions = admin.Permissions,
        IsActive    = user.IsActive,
        CreatedAt   = user.CreatedAt
    };

    private static UserManagementDto MapToUserManagement(User user) => new()
    {
        UserId    = user.UserId,
        Email     = user.Email,
        FirstName = user.FirstName,
        LastName  = user.LastName,
        Role      = user.Role,
        IsActive  = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    private static NotificationResponseDto MapToNotificationResponse(
        Models.Notification notification,
        User user) => new()
    {
        NotificationId = notification.NotificationId,
        UserId         = notification.UserId,
        UserEmail      = user.Email,
        Type           = notification.Type,
        Subject        = notification.Subject,
        Content        = notification.Content,
        SentAt         = notification.SentAt,
        DeliveryStatus = notification.DeliveryStatus
    };

    private static AuditLogResponseDto MapToAuditLogResponse(AuditLog log, User? actor) => new()
    {
        AuditLogId          = log.AuditLogId,
        PerformedByUserId   = log.PerformedByUserId,
        PerformedByEmail    = actor?.Email ?? "unknown",
        Action              = log.Action,
        EntityType          = log.EntityType,
        EntityId            = log.EntityId,
        Details             = log.Details,
        PerformedAt         = log.PerformedAt
    };

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}

