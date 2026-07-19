using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations
{
    public class RecruiterRepository : GenericRepository<Recruiter>, IRecruiterRepository
    {
        public RecruiterRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Recruiter?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);
        }

        public async Task<IEnumerable<Recruiter>> GetByDepartmentAsync(string department)
        {
            return await _dbSet
                .Include(r => r.User)
                .Where(r => r.Department.ToLower() == department.ToLower())
                .ToListAsync();
        }
    }
}