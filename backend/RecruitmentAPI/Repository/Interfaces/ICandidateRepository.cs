using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces
{
    /// <summary>
    /// Contract for repository operations handling Candidate profiles.
    /// </summary>
    public interface ICandidateRepository : IGenericRepository<Candidate>
    {
        Task<Candidate?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Candidate>> GetCandidatesBySkillAsync(string skill);
    }
}