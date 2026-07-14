using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations
{
    /// <summary>
    /// Implementation of the User repository extending the generic repository.
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            // Note: EF Core handles the discriminator column automatically for TPH inheritance
            // However, to filter by specific derived types cleanly:
            return role.ToLower() switch
            {
                "candidate" => await _context.Set<Candidate>().ToListAsync(),
                "recruiter" => await _context.Set<Recruiter>().ToListAsync(),
                "hiringmanager" => await _context.Set<HiringManager>().ToListAsync(),
                "admin" => await _context.Set<Admin>().ToListAsync(),
                _ => await _dbSet.ToListAsync()
            };
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.IsActive).ToListAsync();
        }
    }
}