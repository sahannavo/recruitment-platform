using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for JobPosting entity operations
    /// </summary>
    public interface IJobRepository : IRepository<JobPosting>
    {
        /// <summary>
        /// Get all job postings by department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <returns>List of job postings in the department</returns>
        Task<IEnumerable<JobPosting>> GetByDepartmentAsync(string department);

        /// <summary>
        /// Get all active job postings (Open or Published status)
        /// </summary>
        /// <returns>List of active job postings</returns>
        Task<IEnumerable<JobPosting>> GetActiveJobsAsync();

        /// <summary>
        /// Get all job postings posted by a specific recruiter
        /// </summary>
        /// <param name="recruiterId">Recruiter user ID</param>
        /// <returns>List of job postings by the recruiter</returns>
        Task<IEnumerable<JobPosting>> GetByRecruiterAsync(int recruiterId);

        /// <summary>
        /// Get job postings by status
        /// </summary>
        /// <param name="status">Job status</param>
        /// <returns>List of job postings with the specified status</returns>
        Task<IEnumerable<JobPosting>> GetByStatusAsync(JobStatus status);

        /// <summary>
        /// Get job postings with application count and candidate details
        /// </summary>
        /// <returns>List of job postings with application statistics</returns>
        Task<IEnumerable<JobPosting>> GetJobsWithApplicationStatsAsync();

        /// <summary>
        /// Get job posting by ID with related entities (Applications, Recruiter)
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Job posting with related data</returns>
        Task<JobPosting> GetJobWithDetailsAsync(int jobId);

        /// <summary>
        /// Search job postings by title, description, or requirements
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching job postings</returns>
        Task<IEnumerable<JobPosting>> SearchJobsAsync(string searchTerm);

        /// <summary>
        /// Get job postings by location
        /// </summary>
        /// <param name="location">Location (city, country, or remote)</param>
        /// <returns>List of job postings in the location</returns>
        Task<IEnumerable<JobPosting>> GetByLocationAsync(string location);

        /// <summary>
        /// Get job postings by employment type
        /// </summary>
        /// <param name="employmentType">Employment type (Full-time, Part-time, etc.)</param>
        /// <returns>List of job postings with the employment type</returns>
        Task<IEnumerable<JobPosting>> GetByEmploymentTypeAsync(string employmentType);

        /// <summary>
        /// Get job postings by experience level
        /// </summary>
        /// <param name="experienceLevel">Experience level (Entry, Mid, Senior, etc.)</param>
        /// <returns>List of job postings with the experience level</returns>
        Task<IEnumerable<JobPosting>> GetByExperienceLevelAsync(string experienceLevel);

        /// <summary>
        /// Get job postings posted within a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of job postings within the date range</returns>
        Task<IEnumerable<JobPosting>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get recent job postings (last N days)
        /// </summary>
        /// <param name="days">Number of days</param>
        /// <returns>List of recent job postings</returns>
        Task<IEnumerable<JobPosting>> GetRecentJobsAsync(int days);

        /// <summary>
        /// Get job postings that are expiring soon
        /// </summary>
        /// <param name="daysThreshold">Number of days before expiry</param>
        /// <returns>List of jobs expiring soon</returns>
        Task<IEnumerable<JobPosting>> GetExpiringJobsAsync(int daysThreshold);

        /// <summary>
        /// Get top N jobs with most applications
        /// </summary>
        /// <param name="count">Number of jobs to return</param>
        /// <returns>List of top jobs by application count</returns>
        Task<IEnumerable<JobPosting>> GetTopJobsByApplicationsAsync(int count);

        /// <summary>
        /// Get job postings by required skills
        /// </summary>
        /// <param name="skills">List of required skills</param>
        /// <returns>List of job postings matching the skills</returns>
        Task<IEnumerable<JobPosting>> GetByRequiredSkillsAsync(List<string> skills);

        /// <summary>
        /// Get job postings with no applications yet
        /// </summary>
        /// <returns>List of jobs with zero applications</returns>
        Task<IEnumerable<JobPosting>> GetJobsWithNoApplicationsAsync();

        /// <summary>
        /// Get jobs recommended for a candidate based on skills
        /// </summary>
        /// <param name="candidateSkills">Candidate's skills</param>
        /// <param name="limit">Maximum number of recommendations</param>
        /// <returns>List of recommended jobs</returns>
        Task<IEnumerable<JobPosting>> GetRecommendedJobsForCandidateAsync(List<string> candidateSkills, int limit = 10);

        /// <summary>
        /// Check if a job exists and is active
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>True if job exists and is active</returns>
        Task<bool> IsJobActiveAsync(int jobId);

        /// <summary>
        /// Update job status (with validation)
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="newStatus">New status</param>
        /// <returns>Updated job posting</returns>
        Task<JobPosting> UpdateJobStatusAsync(int jobId, JobStatus newStatus);

        /// <summary>
        /// Get distinct departments with job count
        /// </summary>
        /// <returns>Dictionary of departments and job counts</returns>
        Task<Dictionary<string, int>> GetDepartmentJobCountsAsync();
    }
}