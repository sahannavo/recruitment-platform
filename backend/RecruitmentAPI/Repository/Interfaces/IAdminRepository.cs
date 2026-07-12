using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces;

/// <summary>
/// Specialised repository for admin-level user management queries
/// that go beyond the generic CRUD surface.
/// </summary>
public interface IAdminRepository
{
    /// <summary>Returns every user in the system, ordered by last name.</summary>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>Returns all users that belong to the specified role.</summary>
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role);

    /// <summary>Returns all users whose <c>IsActive</c> flag is <c>true</c>.</summary>
    Task<IEnumerable<User>> GetActiveUsersAsync();

    /// <summary>
    /// Sets <c>IsActive = false</c> on the given user and persists the change.
    /// </summary>
    /// <param name="userId">ID of the user to disable.</param>
    Task DisableUserAsync(int userId);

    /// <summary>
    /// Sets <c>IsActive = true</c> on the given user and persists the change.
    /// </summary>
    /// <param name="userId">ID of the user to enable.</param>
    Task EnableUserAsync(int userId);

    /// <summary>
    /// Updates <c>User.Role</c> to <paramref name="newRole"/> and persists the change.
    /// </summary>
    Task UpdateUserRoleAsync(int userId, string newRole);

    /// <summary>
    /// Returns a single user by email address, or <c>null</c> when not found.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>Returns all audit log entries, newest first.</summary>
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int? performedByUserId = null, string? entityType = null);

    /// <summary>Appends a new audit log entry.</summary>
    Task AddAuditLogAsync(AuditLog entry);
}

