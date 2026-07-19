using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations
{
	public class HiringManagerRepository : GenericRepository<HiringManager>, IHiringManagerRepository
	{
		public HiringManagerRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task<HiringManager?> GetByUserIdAsync(int userId)
		{
			return await _dbSet
				.Include(h => h.User)
				.FirstOrDefaultAsync(h => h.UserId == userId);
		}

		public async Task<IEnumerable<HiringManager>> GetByDepartmentAsync(string department)
		{
			return await _dbSet
				.Include(h => h.User)
				.Where(h => h.Department.ToLower() == department.ToLower())
				.ToListAsync();
		}
	}
}