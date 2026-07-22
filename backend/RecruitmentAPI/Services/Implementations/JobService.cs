using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.DTOs.Application;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.AI;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations
{
    /// <summary>
    /// Service implementation for job operations
    /// </summary>
    public class JobService : IJobService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;
        private readonly ILogger<JobService> _logger;

        public JobService(
            IUnitOfWork unitOfWork,
            IAIService aiService,
            ILogger<JobService> logger)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Get all job postings with pagination
        /// </summary>
        public async Task<JobListResponseDto> GetAllAsync(int pageNumber = 1, int pageSize = 10, JobFilterDto filters = null)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetAllAsync();
                var jobList = jobs.ToList();

                // Apply filters
                if (filters != null)
                {
                    if (!string.IsNullOrEmpty(filters.Department))
                        jobList = jobList.Where(j => j.Department.ToLower() == filters.Department.ToLower()).ToList();

                    if (!string.IsNullOrEmpty(filters.Location))
                        jobList = jobList.Where(j => j.Location.ToLower().Contains(filters.Location.ToLower())).ToList();

                    if (!string.IsNullOrEmpty(filters.Status))
                    {
                        if (Enum.TryParse<JobStatus>(filters.Status, true, out var status))
                            jobList = jobList.Where(j => j.Status == status).ToList();
                    }

                    if (!string.IsNullOrEmpty(filters.EmploymentType))
                        jobList = jobList.Where(j => j.EmploymentType.ToLower() == filters.EmploymentType.ToLower()).ToList();

                    if (!string.IsNullOrEmpty(filters.ExperienceLevel))
                        jobList = jobList.Where(j => j.ExperienceLevel.ToLower() == filters.ExperienceLevel.ToLower()).ToList();

                    if (filters.IsRemote.HasValue)
                        jobList = jobList.Where(j => j.IsRemote == filters.IsRemote.Value).ToList();

                    if (!string.IsNullOrEmpty(filters.SearchTerm))
                    {
                        var term = filters.SearchTerm.ToLower();
                        jobList = jobList.Where(j => j.Title.ToLower().Contains(term) ||
                                                     j.Description.ToLower().Contains(term) ||
                                                     j.Requirements.ToLower().Contains(term)).ToList();
                    }

                    if (filters.PostedAfter.HasValue)
                        jobList = jobList.Where(j => j.CreatedAt >= filters.PostedAfter.Value).ToList();

                    if (filters.PostedBefore.HasValue)
                        jobList = jobList.Where(j => j.CreatedAt <= filters.PostedBefore.Value).ToList();
                }

                var totalCount = jobList.Count;

                // Apply pagination
                var pagedJobs = jobList
                    .OrderByDescending(j => j.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var summaries = new List<JobSummaryDto>();
                foreach (var job in pagedJobs)
                {
                    var applications = await _unitOfWork.Applications.GetByJobAsync(job.JobId);
                    summaries.Add(new JobSummaryDto
                    {
                        JobId = job.JobId,
                        Title = job.Title,
                        Department = job.Department,
                        Location = job.Location,
                        Status = job.Status.ToString(),
                        SalaryRange = job.SalaryRange,
                        CreatedAt = job.CreatedAt,
                        ApplicantsCount = applications.Count(),
                        IsRemote = job.IsRemote,
                        EmploymentType = job.EmploymentType,
                        ExperienceLevel = job.ExperienceLevel,
                        RequiredSkills = string.IsNullOrEmpty(job.RequiredSkills) ? new List<string>() : job.RequiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList()
                    });
                }

                return new JobListResponseDto
                {
                    Jobs = summaries,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all jobs");
                throw;
            }
        }

        /// <summary>
        /// Get job posting by ID
        /// </summary>
        public async Task<JobResponseDto> GetByIdAsync(int jobId)
        {
            try
            {
                var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
                if (job == null)
                    throw new KeyNotFoundException($"Job with ID {jobId} not found");

                var applications = await _unitOfWork.Applications.GetByJobAsync(jobId);
                var applicationsList = applications.ToList();

                return new JobResponseDto
                {
                    JobId = job.JobId,
                    Title = job.Title,
                    Description = job.Description,
                    Requirements = job.Requirements,
                    Department = job.Department,
                    Location = job.Location,
                    SalaryRange = job.SalaryRange,
                    Status = job.Status.ToString(),
                    PostedBy = job.RecruiterId,
                    PostedByName = job.Recruiter != null ? $"{job.Recruiter.User.FirstName} {job.Recruiter.User.LastName}" : "Unknown",
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.UpdatedAt,
                    ApplicantsCount = applicationsList.Count,
                    ActiveApplicantsCount = applicationsList.Count(a => a.Status != ApplicationStatus.Rejected
                                                                        && a.Status != ApplicationStatus.Withdrawn),
                    ShortlistedCount = applicationsList.Count(a => a.Status == ApplicationStatus.Shortlisted),
                    HiredCount = applicationsList.Count(a => a.Status == ApplicationStatus.Hired),
                    PositionsAvailable = job.PositionsAvailable,
                    ExperienceLevel = job.ExperienceLevel,
                    EmploymentType = job.EmploymentType,
                    IsRemote = job.IsRemote,
                    RequiredSkills = string.IsNullOrEmpty(job.RequiredSkills)
                        ? new List<string>()
                        : job.RequiredSkills.Split(',').Select(s => s.Trim()).ToList(),
                    IsAcceptingApplications = job.Status == JobStatus.Open || job.Status == JobStatus.Published,
                    ExpiryDate = job.ExpiresAt,
                    AverageAIScore = applicationsList.Any(a => a.AI_Score.HasValue)
                        ? applicationsList.Where(a => a.AI_Score.HasValue).Average(a => a.AI_Score.Value)
                        : null,
                    TopAIScore = applicationsList.Any(a => a.AI_Score.HasValue)
                        ? applicationsList.Where(a => a.AI_Score.HasValue).Max(a => a.AI_Score.Value)
                        : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job by ID {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Create a new job posting
        /// </summary>
        public async Task<JobResponseDto> CreateAsync(JobPostDto jobPostDto, int recruiterId)
        {
            try
            {
                var recruiter = await _unitOfWork.Recruiters.FirstOrDefaultAsync(r => r.UserId == recruiterId);
                if (recruiter == null)
                    throw new KeyNotFoundException($"Recruiter profile for User ID {recruiterId} not found");

                var job = new JobPosting
                {
                    Title = jobPostDto.Title,
                    Description = jobPostDto.Description,
                    Requirements = jobPostDto.Requirements,
                    Department = jobPostDto.Department,
                    Location = jobPostDto.Location,
                    SalaryRange = jobPostDto.SalaryRange,
                    Status = Enum.TryParse<JobStatus>(jobPostDto.Status, true, out var status) ? status : JobStatus.Draft,
                    RecruiterId = recruiter.RecruiterId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiresAt = jobPostDto.ExpiryDate,
                    PositionsAvailable = jobPostDto.PositionsAvailable,
                    ExperienceLevel = jobPostDto.ExperienceLevel,
                    EmploymentType = jobPostDto.EmploymentType,
                    IsRemote = jobPostDto.IsRemote,
                    RequiredSkills = jobPostDto.RequiredSkills
                };

                await _unitOfWork.Jobs.AddAsync(job);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Job created with ID {JobId} by recruiter {RecruiterId}", job.JobId, recruiterId);

                return await GetByIdAsync(job.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job by recruiter {RecruiterId}", recruiterId);
                throw;
            }
        }

        /// <summary>
        /// Update an existing job posting
        /// </summary>
        public async Task<JobResponseDto> UpdateAsync(int jobId, JobUpdateDto jobUpdateDto)
        {
            try
            {
                var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
                if (job == null)
                    throw new KeyNotFoundException($"Job with ID {jobId} not found");

                if (!string.IsNullOrEmpty(jobUpdateDto.Title))
                    job.Title = jobUpdateDto.Title;

                if (!string.IsNullOrEmpty(jobUpdateDto.Description))
                    job.Description = jobUpdateDto.Description;

                if (!string.IsNullOrEmpty(jobUpdateDto.Requirements))
                    job.Requirements = jobUpdateDto.Requirements;

                if (!string.IsNullOrEmpty(jobUpdateDto.Department))
                    job.Department = jobUpdateDto.Department;

                if (!string.IsNullOrEmpty(jobUpdateDto.Location))
                    job.Location = jobUpdateDto.Location;

                if (!string.IsNullOrEmpty(jobUpdateDto.SalaryRange))
                    job.SalaryRange = jobUpdateDto.SalaryRange;

                if (!string.IsNullOrEmpty(jobUpdateDto.Status))
                {
                    if (Enum.TryParse<JobStatus>(jobUpdateDto.Status, true, out var status))
                        job.Status = status;
                }

                if (jobUpdateDto.ExpiryDate.HasValue)
                    job.ExpiresAt = jobUpdateDto.ExpiryDate;

                if (jobUpdateDto.PositionsAvailable > 0)
                    job.PositionsAvailable = jobUpdateDto.PositionsAvailable.Value;

                if (!string.IsNullOrEmpty(jobUpdateDto.ExperienceLevel))
                    job.ExperienceLevel = jobUpdateDto.ExperienceLevel;

                if (!string.IsNullOrEmpty(jobUpdateDto.EmploymentType))
                    job.EmploymentType = jobUpdateDto.EmploymentType;

                if (jobUpdateDto.IsRemote.HasValue)
                    job.IsRemote = jobUpdateDto.IsRemote.Value;

                if (!string.IsNullOrEmpty(jobUpdateDto.RequiredSkills))
                    job.RequiredSkills = jobUpdateDto.RequiredSkills;

                job.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} updated", jobId);

                return await GetByIdAsync(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Delete a job posting (soft delete)
        /// </summary>
        public async Task<bool> DeleteAsync(int jobId)
        {
            try
            {
                var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
                if (job == null)
                    return false;
                _unitOfWork.Jobs.Remove(job);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} permanently deleted", jobId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Get recommended jobs for a candidate
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetRecommendedJobsAsync(int candidateId, int limit = 10)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(candidateId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {candidateId} not found");

                var skills = candidate.SkillsSummary?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();
                var jobs = await _unitOfWork.Jobs.GetRecommendedJobsForCandidateAsync(skills, limit);

                var result = new List<JobResponseDto>();
                foreach (var job in jobs)
                {
                    var jobDto = await GetByIdAsync(job.JobId);
                    result.Add(jobDto);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended jobs for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        /// <summary>
        /// Get active job postings
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetActiveJobsAsync()
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetActiveJobsAsync();
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active jobs");
                throw;
            }
        }

        /// <summary>
        /// Get jobs by department
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetByDepartmentAsync(string department)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetByDepartmentAsync(department);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by department {Department}", department);
                throw;
            }
        }

        /// <summary>
        /// Get jobs by recruiter
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetByRecruiterAsync(int recruiterId)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetByRecruiterAsync(recruiterId);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by recruiter {RecruiterId}", recruiterId);
                throw;
            }
        }

        /// <summary>
        /// Search jobs by keyword
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> SearchJobsAsync(string searchTerm)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.SearchJobsAsync(searchTerm);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching jobs with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Update job status
        /// </summary>
        public async Task<JobResponseDto> UpdateStatusAsync(int jobId, string status)
        {
            try
            {
                if (!Enum.TryParse<JobStatus>(status, true, out var newStatus))
                    throw new ArgumentException($"Invalid status: {status}");

                var job = await _unitOfWork.Jobs.UpdateJobStatusAsync(jobId, newStatus);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} status updated to {Status}", jobId, status);

                return await GetByIdAsync(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job {JobId} status", jobId);
                throw;
            }
        }

        /// <summary>
        /// Get job statistics
        /// </summary>
        public async Task<JobStatisticsDto> GetJobStatisticsAsync()
        {
            try
            {
                var allJobs = await _unitOfWork.Jobs.GetAllAsync();
                var jobList = allJobs.ToList();

                var stats = new JobStatisticsDto
                {
                    TotalJobs = jobList.Count,
                    ActiveJobs = jobList.Count(j => j.Status == JobStatus.Open || j.Status == JobStatus.Published),
                    DraftJobs = jobList.Count(j => j.Status == JobStatus.Draft),
                    ClosedJobs = jobList.Count(j => j.Status == JobStatus.Closed),
                    FilledJobs = jobList.Count(j => j.Status == JobStatus.Filled),
                    ArchivedJobs = jobList.Count(j => j.Status == JobStatus.Archived),
                    JobsByDepartment = await _unitOfWork.Jobs.GetDepartmentJobCountsAsync(),
                    JobsByStatus = jobList.GroupBy(j => j.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                    JobsByEmploymentType = jobList.GroupBy(j => j.EmploymentType ?? "Unknown").ToDictionary(g => g.Key, g => g.Count())
                };

                var totalApplications = 0;
                var jobsWithNoApplications = 0;
                foreach (var job in jobList)
                {
                    var count = await _unitOfWork.Applications.GetApplicationCountForJobAsync(job.JobId);
                    totalApplications += count;
                    if (count == 0) jobsWithNoApplications++;
                }

                stats.TotalApplications = totalApplications;
                stats.JobsWithNoApplications = jobsWithNoApplications;
                stats.AverageApplicationsPerJob = jobList.Any() ? (double)totalApplications / jobList.Count : 0;

                if (jobList.Any())
                {
                    stats.OldestJobDate = jobList.Min(j => j.CreatedAt);
                    stats.NewestJobDate = jobList.Max(j => j.CreatedAt);
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job statistics");
                throw;
            }
        }

        /// <summary>
        /// Get jobs by location
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetByLocationAsync(string location)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetByLocationAsync(location);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by location {Location}", location);
                throw;
            }
        }

        /// <summary>
        /// Get jobs by employment type
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetByEmploymentTypeAsync(string employmentType)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetByEmploymentTypeAsync(employmentType);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by employment type {EmploymentType}", employmentType);
                throw;
            }
        }

        /// <summary>
        /// Get jobs by experience level
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetByExperienceLevelAsync(string experienceLevel)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetByExperienceLevelAsync(experienceLevel);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by experience level {ExperienceLevel}", experienceLevel);
                throw;
            }
        }

        /// <summary>
        /// Get recent jobs
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetRecentJobsAsync(int days)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetRecentJobsAsync(days);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent jobs for {Days} days", days);
                throw;
            }
        }

        /// <summary>
        /// Get jobs with most applications
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetTopJobsByApplicationsAsync(int count)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetTopJobsByApplicationsAsync(count);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top {Count} jobs by applications", count);
                throw;
            }
        }

        /// <summary>
        /// Get jobs by required skills
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetByRequiredSkillsAsync(List<string> skills)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetByRequiredSkillsAsync(skills);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by required skills");
                throw;
            }
        }

        /// <summary>
        /// Get jobs with no applications
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetJobsWithNoApplicationsAsync()
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetJobsWithNoApplicationsAsync();
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs with no applications");
                throw;
            }
        }

        /// <summary>
        /// Check if job is active
        /// </summary>
        public async Task<bool> IsJobActiveAsync(int jobId)
        {
            try
            {
                return await _unitOfWork.Jobs.IsJobActiveAsync(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if job {JobId} is active", jobId);
                throw;
            }
        }

        /// <summary>
        /// Get department job counts
        /// </summary>
        public async Task<Dictionary<string, int>> GetDepartmentJobCountsAsync()
        {
            try
            {
                return await _unitOfWork.Jobs.GetDepartmentJobCountsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department job counts");
                throw;
            }
        }

        /// <summary>
        /// Bulk update job status
        /// </summary>
        public async Task<int> BulkUpdateStatusAsync(List<int> jobIds, string status)
        {
            try
            {
                if (!Enum.TryParse<JobStatus>(status, true, out var newStatus))
                    throw new ArgumentException($"Invalid status: {status}");

                var count = 0;
                foreach (var jobId in jobIds)
                {
                    try
                    {
                        await _unitOfWork.Jobs.UpdateJobStatusAsync(jobId, newStatus);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update job {JobId} status", jobId);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Bulk updated {Count} jobs to status {Status}", count, status);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk update job status");
                throw;
            }
        }

        /// <summary>
        /// Get jobs expiring soon
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetExpiringJobsAsync(int daysThreshold)
        {
            try
            {
                var jobs = await _unitOfWork.Jobs.GetExpiringJobsAsync(daysThreshold);
                var result = new List<JobResponseDto>();

                foreach (var job in jobs)
                {
                    result.Add(await GetByIdAsync(job.JobId));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring jobs with threshold {DaysThreshold}", daysThreshold);
                throw;
            }
        }

        /// <summary>
        /// Clone a job posting
        /// </summary>
        public async Task<JobResponseDto> CloneJobAsync(int jobId, int recruiterId)
        {
            try
            {
                var originalJob = await _unitOfWork.Jobs.GetByIdAsync(jobId);
                if (originalJob == null)
                    throw new KeyNotFoundException($"Job with ID {jobId} not found");

                var clonedJob = new JobPosting
                {
                    Title = $"{originalJob.Title} (Clone)",
                    Description = originalJob.Description,
                    Requirements = originalJob.Requirements,
                    Department = originalJob.Department,
                    Location = originalJob.Location,
                    SalaryRange = originalJob.SalaryRange,
                    Status = JobStatus.Draft,
                    RecruiterId = recruiterId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiresAt = originalJob.ExpiresAt,
                    PositionsAvailable = originalJob.PositionsAvailable,
                    ExperienceLevel = originalJob.ExperienceLevel,
                    EmploymentType = originalJob.EmploymentType,
                    IsRemote = originalJob.IsRemote,
                    RequiredSkills = originalJob.RequiredSkills
                };

                await _unitOfWork.Jobs.AddAsync(clonedJob);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} cloned to {ClonedJobId} by recruiter {RecruiterId}",
                    jobId, clonedJob.JobId, recruiterId);

                return await GetByIdAsync(clonedJob.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning job {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Get job with application statistics
        /// </summary>
        public async Task<JobWithStatsDto> GetJobWithStatsAsync(int jobId)
        {
            try
            {
                var job = await GetByIdAsync(jobId);
                var applications = await _unitOfWork.Applications.GetByJobAsync(jobId);
                var applicationsList = applications.ToList();

                var statusCounts = applicationsList
                    .GroupBy(a => a.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                var recentApps = applicationsList
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(5)
                    .Select(a => new JobApplicationSummaryDto
                    {
                        ApplicationId = a.ApplicationId,
                        CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                        CandidateEmail = a.Candidate.User.Email,
                        Status = a.Status.ToString(),
                        AppliedAt = a.AppliedAt,
                        AI_Score = a.AI_Score
                    }).ToList();

                return new JobWithStatsDto
                {
                    JobId = job.JobId,
                    Title = job.Title,
                    Description = job.Description,
                    Requirements = job.Requirements,
                    Department = job.Department,
                    Location = job.Location,
                    SalaryRange = job.SalaryRange,
                    Status = job.Status,
                    PostedBy = job.PostedBy,
                    PostedByName = job.PostedByName,
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.UpdatedAt,
                    ApplicantsCount = job.ApplicantsCount,
                    PositionsAvailable = job.PositionsAvailable,
                    ExperienceLevel = job.ExperienceLevel,
                    EmploymentType = job.EmploymentType,
                    IsRemote = job.IsRemote,
                    IsAcceptingApplications = job.IsAcceptingApplications,
                    ApplicationStatusCounts = statusCounts.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                    RecentApplications = recentApps,
                    AverageAIScore = job.AverageAIScore ?? 0,
                    TopAIScore = job.TopAIScore ?? 0,
                    TotalApplicants = job.ApplicantsCount,
                    ShortlistedCount = statusCounts.GetValueOrDefault(ApplicationStatus.Shortlisted, 0),
                    InterviewedCount = statusCounts.GetValueOrDefault(ApplicationStatus.Interviewed, 0),
                    HiredCount = statusCounts.GetValueOrDefault(ApplicationStatus.Hired, 0)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job with stats for {JobId}", jobId);
                throw;
            }
        }
    }
}