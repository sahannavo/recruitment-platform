using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces
{
    /// <summary>
    /// Repository interface specific to User entity operations.
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(string role);
        Task<IEnumerable<User>> GetActiveUsersAsync();
    }
}