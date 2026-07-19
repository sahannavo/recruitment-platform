using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations
{
    public class CandidateRepository : GenericRepository<Candidate>, ICandidateRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Candidate?> GetByUserIdAsync(int userId)
        {
            return await _context.Candidates
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Candidate?> GetByEmailAsync(string email)
        {
            return await _context.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Email == email);
        }

        public async Task<IEnumerable<Candidate>> GetCandidatesBySkillAsync(string skill)
        {
            return await _context.Candidates
                .Where(c => c.SkillsSummary != null && c.SkillsSummary.Contains(skill))
                .ToListAsync();
        }
    }
}