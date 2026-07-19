using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces
{
    public interface IAdminRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User?> GetUserByEmailAsync(string email);
        Task UpdateUserRoleAsync(int userId, string newRole);
        Task DisableUserAsync(int userId);
        Task EnableUserAsync(int userId);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int? performedByUserId = null, string? entityType = null);
        Task AddAuditLogAsync(AuditLog log);

        /// <summary>✅ ADDED: Add a new admin</summary>
        Task AddAsync(Admin admin);
    }
}