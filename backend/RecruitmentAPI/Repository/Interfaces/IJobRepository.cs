// Repository/Interfaces/IJobRepository.cs
using RecruitmentAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecruitmentAPI.Repository.Interfaces
{
    public interface IJobRepository : IGenericRepository<JobPosting>
    {
        Task<JobPosting?> GetJobWithDetailsAsync(int jobId);
        Task<IEnumerable<JobPosting>> GetActiveJobsAsync();
        Task<IEnumerable<JobPosting>> GetByDepartmentAsync(string department);
        Task<IEnumerable<JobPosting>> GetByRecruiterAsync(int recruiterId);
        Task<IEnumerable<JobPosting>> GetByLocationAsync(string location);
        Task<IEnumerable<JobPosting>> GetByEmploymentTypeAsync(string employmentType);
        Task<IEnumerable<JobPosting>> GetByExperienceLevelAsync(string experienceLevel);
        Task<IEnumerable<JobPosting>> SearchJobsAsync(string searchTerm);
        Task<IEnumerable<JobPosting>> GetRecommendedJobsForCandidateAsync(List<string> skills, int limit);
        Task<IEnumerable<JobPosting>> GetRecentJobsAsync(int days);
        Task<IEnumerable<JobPosting>> GetTopJobsByApplicationsAsync(int count);
        Task<IEnumerable<JobPosting>> GetByRequiredSkillsAsync(List<string> skills);
        Task<IEnumerable<JobPosting>> GetJobsWithNoApplicationsAsync();
        Task<IEnumerable<JobPosting>> GetExpiringJobsAsync(int daysThreshold);
        Task<JobPosting> UpdateJobStatusAsync(int jobId, JobStatus status);
        Task<bool> IsJobActiveAsync(int jobId);
        Task<Dictionary<string, int>> GetDepartmentJobCountsAsync();
    }
}