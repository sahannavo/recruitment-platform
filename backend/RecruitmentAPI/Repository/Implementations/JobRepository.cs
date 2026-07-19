using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations
{
    public class JobRepository : GenericRepository<JobPosting>, IJobRepository
    {
        public JobRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<JobPosting>> GetActiveJobsAsync()
        {
            return await _dbSet
                .Where(j => j.Status == JobStatus.Open || j.Status == JobStatus.Published)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetByDepartmentAsync(string department)
        {
            return await _dbSet
                .Where(j => j.Department.ToLower() == department.ToLower())
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetByRecruiterAsync(int recruiterId)
        {
            return await _dbSet
                .Where(j => j.RecruiterId == recruiterId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<JobPosting?> GetJobWithDetailsAsync(int jobId)
        {
            return await _dbSet
                .Include(j => j.Recruiter)
                .ThenInclude(r => r!.User)
                .Include(j => j.HiringManager)
                .ThenInclude(h => h!.User)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<IEnumerable<JobPosting>> SearchJobsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(j => j.Title.ToLower().Contains(term) ||
                            j.Description.ToLower().Contains(term) ||
                            j.Requirements.ToLower().Contains(term) ||
                            j.Department.ToLower().Contains(term))
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<JobPosting> UpdateJobStatusAsync(int jobId, JobStatus status)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job != null)
            {
                job.Status = status;
                job.UpdatedAt = DateTime.UtcNow;
            }
            return job!;
        }

        public async Task<Dictionary<string, int>> GetDepartmentJobCountsAsync()
        {
            return await _dbSet
                .GroupBy(j => j.Department)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<JobPosting>> GetRecentJobsAsync(int days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);
            return await _dbSet
                .Where(j => j.CreatedAt >= cutoff && (j.Status == JobStatus.Open || j.Status == JobStatus.Published))
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetTopJobsByApplicationsAsync(int count)
        {
            return await _dbSet
                .Where(j => j.Status == JobStatus.Open || j.Status == JobStatus.Published)
                .OrderByDescending(j => j.Applications.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetByLocationAsync(string location)
        {
            return await _dbSet
                .Where(j => j.Location.ToLower().Contains(location.ToLower()))
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetByEmploymentTypeAsync(string employmentType)
        {
            return await _dbSet
                .Where(j => j.EmploymentType.ToLower() == employmentType.ToLower())
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetByExperienceLevelAsync(string experienceLevel)
        {
            return await _dbSet
                .Where(j => j.ExperienceLevel.ToLower() == experienceLevel.ToLower())
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetRecommendedJobsForCandidateAsync(List<string> skills, int limit)
        {
            if (skills == null || skills.Count == 0)
            {
                return await GetActiveJobsAsync();
            }

            var skillPatterns = skills.Select(s => s.ToLower()).ToList();

            return await _dbSet
                .Where(j => (j.Status == JobStatus.Open || j.Status == JobStatus.Published) &&
                            (j.RequiredSkills != null &&
                             skillPatterns.Any(s => j.RequiredSkills.ToLower().Contains(s))))
                .OrderByDescending(j => j.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetByRequiredSkillsAsync(List<string> skills)
        {
            if (skills == null || skills.Count == 0)
                return await GetActiveJobsAsync();

            var skillPatterns = skills.Select(s => s.ToLower()).ToList();

            return await _dbSet
                .Where(j => j.RequiredSkills != null &&
                            skillPatterns.Any(s => j.RequiredSkills.ToLower().Contains(s)))
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetJobsWithNoApplicationsAsync()
        {
            return await _dbSet
                .Where(j => !j.Applications.Any())
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsJobActiveAsync(int jobId)
        {
            var job = await _dbSet.FindAsync(jobId);
            return job != null && (job.Status == JobStatus.Open || job.Status == JobStatus.Published);
        }

        public async Task<IEnumerable<JobPosting>> GetExpiringJobsAsync(int daysThreshold)
        {
            var threshold = DateTime.UtcNow.AddDays(daysThreshold);
            return await _dbSet
                .Where(j => j.ExpiresAt.HasValue && j.ExpiresAt.Value <= threshold &&
                            (j.Status == JobStatus.Open || j.Status == JobStatus.Published))
                .OrderBy(j => j.ExpiresAt)
                .ToListAsync();
        }

        public override async Task<IEnumerable<JobPosting>> GetAllAsync()
        {
            return await _dbSet
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }
    }
}