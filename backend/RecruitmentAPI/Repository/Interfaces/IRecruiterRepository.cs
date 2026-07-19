using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces
{
    public interface IRecruiterRepository : IGenericRepository<Recruiter>
    {
        Task<Recruiter?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Recruiter>> GetByDepartmentAsync(string department);
    }
}