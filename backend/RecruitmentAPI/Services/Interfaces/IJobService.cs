using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Services.Interfaces
{
    /// <summary>
    /// Service interface for job operations
    /// </summary>
    public interface IJobService
    {
        /// <summary>
        /// Get all job postings with pagination
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="filters">Filter criteria</param>
        /// <returns>Paginated job list</returns>
        Task<JobListResponseDto> GetAllAsync(int pageNumber = 1, int pageSize = 10, JobFilterDto filters = null);

        /// <summary>
        /// Get job posting by ID
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Job response DTO</returns>
        Task<JobResponseDto> GetByIdAsync(int jobId);

        /// <summary>
        /// Create a new job posting
        /// </summary>
        /// <param name="jobPostDto">Job creation data</param>
        /// <param name="recruiterId">Recruiter ID</param>
        /// <returns>Created job response</returns>
        Task<JobResponseDto> CreateAsync(JobPostDto jobPostDto, int recruiterId);

        /// <summary>
        /// Update an existing job posting
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="jobUpdateDto">Job update data</param>
        /// <returns>Updated job response</returns>
        Task<JobResponseDto> UpdateAsync(int jobId, JobUpdateDto jobUpdateDto);

        /// <summary>
        /// Delete a job posting (soft delete)
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteAsync(int jobId);

        /// <summary>
        /// Get recommended jobs for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <param name="limit">Number of recommendations</param>
        /// <returns>List of recommended jobs</returns>
        Task<IEnumerable<JobResponseDto>> GetRecommendedJobsAsync(int candidateId, int limit = 10);

        /// <summary>
        /// Get active job postings
        /// </summary>
        /// <returns>List of active jobs</returns>
        Task<IEnumerable<JobResponseDto>> GetActiveJobsAsync();

        /// <summary>
        /// Get jobs by department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <returns>List of jobs in the department</returns>
        Task<IEnumerable<JobResponseDto>> GetByDepartmentAsync(string department);

        /// <summary>
        /// Get jobs by recruiter
        /// </summary>
        /// <param name="recruiterId">Recruiter ID</param>
        /// <returns>List of jobs posted by the recruiter</returns>
        Task<IEnumerable<JobResponseDto>> GetByRecruiterAsync(int recruiterId);

        /// <summary>
        /// Search jobs by keyword
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching jobs</returns>
        Task<IEnumerable<JobResponseDto>> SearchJobsAsync(string searchTerm);

        /// <summary>
        /// Update job status
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="status">New status</param>
        /// <returns>Updated job</returns>
        Task<JobResponseDto> UpdateStatusAsync(int jobId, string status);

        /// <summary>
        /// Get job statistics
        /// </summary>
        /// <returns>Job statistics</returns>
        Task<JobStatisticsDto> GetJobStatisticsAsync();

        /// <summary>
        /// Get jobs by location
        /// </summary>
        /// <param name="location">Location</param>
        /// <returns>List of jobs in the location</returns>
        Task<IEnumerable<JobResponseDto>> GetByLocationAsync(string location);

        /// <summary>
        /// Get jobs by employment type
        /// </summary>
        /// <param name="employmentType">Employment type</param>
        /// <returns>List of jobs with the employment type</returns>
        Task<IEnumerable<JobResponseDto>> GetByEmploymentTypeAsync(string employmentType);

        /// <summary>
        /// Get jobs by experience level
        /// </summary>
        /// <param name="experienceLevel">Experience level</param>
        /// <returns>List of jobs with the experience level</returns>
        Task<IEnumerable<JobResponseDto>> GetByExperienceLevelAsync(string experienceLevel);

        /// <summary>
        /// Get recent jobs
        /// </summary>
        /// <param name="days">Number of days</param>
        /// <returns>List of recent jobs</returns>
        Task<IEnumerable<JobResponseDto>> GetRecentJobsAsync(int days);

        /// <summary>
        /// Get jobs with most applications
        /// </summary>
        /// <param name="count">Number of jobs</param>
        /// <returns>List of top jobs</returns>
        Task<IEnumerable<JobResponseDto>> GetTopJobsByApplicationsAsync(int count);

        /// <summary>
        /// Get jobs by required skills
        /// </summary>
        /// <param name="skills">List of skills</param>
        /// <returns>List of matching jobs</returns>
        Task<IEnumerable<JobResponseDto>> GetByRequiredSkillsAsync(List<string> skills);

        /// <summary>
        /// Get jobs with no applications
        /// </summary>
        /// <returns>List of jobs with no applications</returns>
        Task<IEnumerable<JobResponseDto>> GetJobsWithNoApplicationsAsync();

        /// <summary>
        /// Check if job is active
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>True if job is active</returns>
        Task<bool> IsJobActiveAsync(int jobId);

        /// <summary>
        /// Get department job counts
        /// </summary>
        /// <returns>Dictionary of department and job counts</returns>
        Task<Dictionary<string, int>> GetDepartmentJobCountsAsync();

        /// <summary>
        /// Bulk update job status
        /// </summary>
        /// <param name="jobIds">List of job IDs</param>
        /// <param name="status">New status</param>
        /// <returns>Number of jobs updated</returns>
        Task<int> BulkUpdateStatusAsync(List<int> jobIds, string status);

        /// <summary>
        /// Get jobs expiring soon
        /// </summary>
        /// <param name="daysThreshold">Days threshold</param>
        /// <returns>List of jobs expiring soon</returns>
        Task<IEnumerable<JobResponseDto>> GetExpiringJobsAsync(int daysThreshold);

        /// <summary>
        /// Clone a job posting
        /// </summary>
        /// <param name="jobId">Job ID to clone</param>
        /// <param name="recruiterId">Recruiter ID</param>
        /// <returns>Cloned job</returns>
        Task<JobResponseDto> CloneJobAsync(int jobId, int recruiterId);

        /// <summary>
        /// Get job with application statistics
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Job with statistics</returns>
        Task<JobWithStatsDto> GetJobWithStatsAsync(int jobId);
    }

    /// <summary>
    /// DTO for job with statistics
    /// </summary>
    public class JobWithStatsDto : JobResponseDto
    {
        public Dictionary<string, int> ApplicationStatusCounts { get; set; }
        public List<ApplicationSummaryDto> RecentApplications { get; set; }
        public double AverageAIScore { get; set; }
        public double TopAIScore { get; set; }
        public int TotalApplicants { get; set; }
        public int ShortlistedCount { get; set; }
        public int InterviewedCount { get; set; }
        public int HiredCount { get; set; }
    }

    /// <summary>
    /// DTO for application summary
    /// </summary>
    public class ApplicationSummaryDto
    {
        public int ApplicationId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public double? AI_Score { get; set; }
    }
}