using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repositories.Interfaces;

namespace RecruitmentAPI.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for JobPosting entity operations
    /// </summary>
    public class JobRepository : Repository<JobPosting>, IJobRepository
    {
        public JobRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get all job postings by department
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetByDepartmentAsync(string department)
        {
            return await _context.JobPostings
                .Where(j => j.Department.ToLower() == department.ToLower())
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get all active job postings (Open or Published status)
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetActiveJobsAsync()
        {
            return await _context.JobPostings
                .Where(j => j.Status == JobStatus.Open || j.Status == JobStatus.Published)
                .Where(j => !j.ExpiryDate.HasValue || j.ExpiryDate.Value > DateTime.UtcNow)
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get all job postings posted by a specific recruiter
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetByRecruiterAsync(int recruiterId)
        {
            return await _context.JobPostings
                .Where(j => j.PostedBy == recruiterId)
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings by status
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetByStatusAsync(JobStatus status)
        {
            return await _context.JobPostings
                .Where(j => j.Status == status)
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings with application count and candidate details
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetJobsWithApplicationStatsAsync()
        {
            return await _context.JobPostings
                .Include(j => j.Applications)
                .Include(j => j.Recruiter)
                .OrderByDescending(j => j.Applications.Count)
                .ToListAsync();
        }

        /// <summary>
        /// Get job posting by ID with related entities
        /// </summary>
        public async Task<JobPosting> GetJobWithDetailsAsync(int jobId)
        {
            return await _context.JobPostings
                .Where(j => j.JobId == jobId)
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                    .ThenInclude(a => a.Candidate)
                .Include(j => j.Applications)
                    .ThenInclude(a => a.Interviews)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Search job postings by title, description, or requirements
        /// </summary>
        public async Task<IEnumerable<JobPosting>> SearchJobsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var term = searchTerm.ToLower();
            return await _context.JobPostings
                .Where(j => j.Title.ToLower().Contains(term) ||
                            j.Description.ToLower().Contains(term) ||
                            j.Requirements.ToLower().Contains(term) ||
                            j.Department.ToLower().Contains(term) ||
                            j.Location.ToLower().Contains(term))
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings by location
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetByLocationAsync(string location)
        {
            return await _context.JobPostings
                .Where(j => j.Location.ToLower().Contains(location.ToLower()) ||
                            (j.IsRemote && location.ToLower() == "remote"))
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings by employment type
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetByEmploymentTypeAsync(string employmentType)
        {
            return await _context.JobPostings
                .Where(j => j.EmploymentType.ToLower() == employmentType.ToLower())
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings by experience level
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetByExperienceLevelAsync(string experienceLevel)
        {
            return await _context.JobPostings
                .Where(j => j.ExperienceLevel.ToLower() == experienceLevel.ToLower())
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings posted within a date range
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.JobPostings
                .Where(j => j.CreatedAt >= startDate && j.CreatedAt <= endDate)
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get recent job postings (last N days)
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetRecentJobsAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _context.JobPostings
                .Where(j => j.CreatedAt >= cutoffDate)
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings that are expiring soon
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetExpiringJobsAsync(int daysThreshold)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysThreshold);
            return await _context.JobPostings
                .Where(j => j.ExpiryDate.HasValue && j.ExpiryDate.Value <= cutoffDate && j.ExpiryDate.Value > DateTime.UtcNow)
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderBy(j => j.ExpiryDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get top N jobs with most applications
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetTopJobsByApplicationsAsync(int count)
        {
            return await _context.JobPostings
                .Include(j => j.Applications)
                .OrderByDescending(j => j.Applications.Count)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings by required skills
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetByRequiredSkillsAsync(List<string> skills)
        {
            if (skills == null || !skills.Any())
                return new List<JobPosting>();

            return await _context.JobPostings
                .Where(j => skills.Any(s => j.RequiredSkills.ToLower().Contains(s.ToLower())))
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get job postings with no applications yet
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetJobsWithNoApplicationsAsync()
        {
            return await _context.JobPostings
                .Where(j => !j.Applications.Any())
                .Include(j => j.Recruiter)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get jobs recommended for a candidate based on skills
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetRecommendedJobsForCandidateAsync(List<string> candidateSkills, int limit = 10)
        {
            if (candidateSkills == null || !candidateSkills.Any())
                return await GetActiveJobsAsync();

            var activeJobs = await GetActiveJobsAsync();

            return activeJobs
                .Select(j => new
                {
                    Job = j,
                    MatchCount = candidateSkills.Count(s =>
                        j.RequiredSkills.ToLower().Contains(s.ToLower()))
                })
                .Where(x => x.MatchCount > 0)
                .OrderByDescending(x => x.MatchCount)
                .Select(x => x.Job)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Check if a job exists and is active
        /// </summary>
        public async Task<bool> IsJobActiveAsync(int jobId)
        {
            var job = await GetByIdAsync(jobId);
            return job != null &&
                   (job.Status == JobStatus.Open || job.Status == JobStatus.Published) &&
                   (!job.ExpiryDate.HasValue || job.ExpiryDate.Value > DateTime.UtcNow);
        }

        /// <summary>
        /// Update job status (with validation)
        /// </summary>
        public async Task<JobPosting> UpdateJobStatusAsync(int jobId, JobStatus newStatus)
        {
            var job = await GetByIdAsync(jobId);
            if (job == null)
                throw new KeyNotFoundException($"Job with ID {jobId} not found");

            // Validate status transition
            if (!IsValidStatusTransition(job.Status, newStatus))
                throw new InvalidOperationException($"Invalid status transition from {job.Status} to {newStatus}");

            job.Status = newStatus;
            job.UpdatedAt = DateTime.UtcNow;

            _context.JobPostings.Update(job);
            await _context.SaveChangesAsync();

            return job;
        }

        /// <summary>
        /// Get distinct departments with job count
        /// </summary>
        public async Task<Dictionary<string, int>> GetDepartmentJobCountsAsync()
        {
            return await _context.JobPostings
                .GroupBy(j => j.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Department, x => x.Count);
        }

        /// <summary>
        /// Validate status transition
        /// </summary>
        private bool IsValidStatusTransition(JobStatus current, JobStatus next)
        {
            var validTransitions = new Dictionary<JobStatus, List<JobStatus>>
            {
                { JobStatus.Draft, new List<JobStatus> { JobStatus.Published, JobStatus.Archived } },
                { JobStatus.Published, new List<JobStatus> { JobStatus.Open, JobStatus.Closed, JobStatus.Archived } },
                { JobStatus.Open, new List<JobStatus> { JobStatus.Closed, JobStatus.OnHold, JobStatus.Filled, JobStatus.Archived } },
                { JobStatus.Closed, new List<JobStatus> { JobStatus.Open, JobStatus.Archived } },
                { JobStatus.OnHold, new List<JobStatus> { JobStatus.Open, JobStatus.Closed, JobStatus.Archived } },
                { JobStatus.Filled, new List<JobStatus> { JobStatus.Archived } },
                { JobStatus.Archived, new List<JobStatus>() }
            };

            return validTransitions.ContainsKey(current) && validTransitions[current].Contains(next);
        }
    }
}