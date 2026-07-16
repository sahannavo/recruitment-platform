using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations
{
    /// <summary>
    /// Implementation of the User repository extending the generic repository.
    /// </summary>
    public class UserRepository : GenericRepository<User>, IUserRepository
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
            // Candidate, Recruiter, HiringManager all inherit from User.
            // Admin uses composition (has User navigation), not inheritance.
            return role.ToLower() switch
            {
                // Let EF Core filter the types at the database level directly from _dbSet!
                "candidate" => await _dbSet.OfType<Candidate>().Cast<User>().ToListAsync(),
                "recruiter" => await _dbSet.OfType<Recruiter>().Cast<User>().ToListAsync(),
                "hiringmanager" => await _dbSet.OfType<HiringManager>().Cast<User>().ToListAsync(),

                // For Admin, we must go through the Admin DbSet due to the composition model
                "admin" => await _context.Set<Admin>()
                    .Include(a => a.User)
                    .Select(a => a.User)
                    .ToListAsync(),

                _ => await _dbSet.ToListAsync()
            };
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.IsActive).ToListAsync();
        }
    }
}