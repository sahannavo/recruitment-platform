using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces
{
    public interface IHiringManagerRepository : IGenericRepository<HiringManager>
    {
        Task<HiringManager?> GetByUserIdAsync(int userId);
        Task<IEnumerable<HiringManager>> GetByDepartmentAsync(string department);
    }
}