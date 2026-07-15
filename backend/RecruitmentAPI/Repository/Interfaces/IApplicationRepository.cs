using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Application entity operations
    /// </summary>
    public interface IApplicationRepository : IRepository<Application>
    {
        /// <summary>
        /// Get all applications by candidate ID
        /// </summary>
        /// <param name="candidateId">Candidate user ID</param>
        /// <returns>List of applications by the candidate</returns>
        Task<IEnumerable<Application>> GetByCandidateAsync(int candidateId);

        /// <summary>
        /// Get all applications for a specific job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>List of applications for the job</returns>
        Task<IEnumerable<Application>> GetByJobAsync(int jobId);

        /// <summary>
        /// Get applications by status
        /// </summary>
        /// <param name="status">Application status</param>
        /// <returns>List of applications with the specified status</returns>
        Task<IEnumerable<Application>> GetByStatusAsync(ApplicationStatus status);

        /// <summary>
        /// Get applications with AI score greater than threshold
        /// </summary>
        /// <param name="threshold">Minimum AI score</param>
        /// <returns>List of applications with high AI scores</returns>
        Task<IEnumerable<Application>> GetWithAI_ScoreAsync(double threshold);

        /// <summary>
        /// Get application by candidate and job (check if already applied)
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <param name="jobId">Job ID</param>
        /// <returns>Application if exists, null otherwise</returns>
        Task<Application> GetByCandidateAndJobAsync(int candidateId, int jobId);

        /// <summary>
        /// Get application with full details (Candidate, Job, Interviews)
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <returns>Application with related data</returns>
        Task<Application> GetApplicationWithDetailsAsync(int applicationId);

        /// <summary>
        /// Get applications by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of applications within the date range</returns>
        Task<IEnumerable<Application>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get applications for a recruiter's jobs
        /// </summary>
        /// <param name="recruiterId">Recruiter ID</param>
        /// <returns>List of applications for jobs posted by the recruiter</returns>
        Task<IEnumerable<Application>> GetByRecruiterAsync(int recruiterId);

        /// <summary>
        /// Get recent applications (last N days)
        /// </summary>
        /// <param name="days">Number of days</param>
        /// <returns>List of recent applications</returns>
        Task<IEnumerable<Application>> GetRecentApplicationsAsync(int days);

        /// <summary>
        /// Get applications by AI score range
        /// </summary>
        /// <param name="minScore">Minimum score</param>
        /// <param name="maxScore">Maximum score</param>
        /// <returns>List of applications within the score range</returns>
        Task<IEnumerable<Application>> GetByAIScoreRangeAsync(double minScore, double maxScore);

        /// <summary>
        /// Get applications with status history
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <returns>Application with status history</returns>
        Task<Application> GetApplicationWithStatusHistoryAsync(int applicationId);

        /// <summary>
        /// Get count of applications by status for a job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Dictionary of status and count</returns>
        Task<Dictionary<ApplicationStatus, int>> GetApplicationCountByStatusAsync(int jobId);

        /// <summary>
        /// Get applications by source
        /// </summary>
        /// <param name="source">Application source (LinkedIn, Website, etc.)</param>
        /// <returns>List of applications from the source</returns>
        Task<IEnumerable<Application>> GetBySourceAsync(string source);

        /// <summary>
        /// Get applications that are pending review
        /// </summary>
        /// <returns>List of applications pending review</returns>
        Task<IEnumerable<Application>> GetPendingReviewApplicationsAsync();

        /// <summary>
        /// Get applications with interview scheduled
        /// </summary>
        /// <returns>List of applications with interviews scheduled</returns>
        Task<IEnumerable<Application>> GetApplicationsWithInterviewsAsync();

        /// <summary>
        /// Get applications by hiring manager
        /// </summary>
        /// <param name="hiringManagerId">Hiring manager ID</param>
        /// <returns>List of applications for jobs under the hiring manager</returns>
        Task<IEnumerable<Application>> GetByHiringManagerAsync(int hiringManagerId);

        /// <summary>
        /// Search applications by candidate name or job title
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching applications</returns>
        Task<IEnumerable<Application>> SearchApplicationsAsync(string searchTerm);

        /// <summary>
        /// Get application statistics for dashboard
        /// </summary>
        /// <returns>Application statistics summary</returns>
        Task<ApplicationStatistics> GetApplicationStatisticsAsync();

        /// <summary>
        /// Bulk update application status
        /// </summary>
        /// <param name="applicationIds">List of application IDs</param>
        /// <param name="newStatus">New status</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>Number of applications updated</returns>
        Task<int> BulkUpdateStatusAsync(List<int> applicationIds, ApplicationStatus newStatus, string notes = null);

        /// <summary>
        /// Get applications by department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <returns>List of applications for jobs in the department</returns>
        Task<IEnumerable<Application>> GetByDepartmentAsync(string department);

        /// <summary>
        /// Get applications with AI scores for a specific job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="threshold">Minimum AI score threshold</param>
        /// <returns>List of applications with AI scores</returns>
        Task<IEnumerable<Application>> GetJobApplicationsWithAIScoresAsync(int jobId, double? threshold = null);

        /// <summary>
        /// Get applications for a candidate with job details
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>List of applications with job details</returns>
        Task<IEnumerable<Application>> GetCandidateApplicationsWithJobDetailsAsync(int candidateId);

        /// <summary>
        /// Get total application count for a job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Total number of applications</returns>
        Task<int> GetApplicationCountForJobAsync(int jobId);

        /// <summary>
        /// Get active applications for a candidate (not rejected or withdrawn)
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>List of active applications</returns>
        Task<IEnumerable<Application>> GetActiveApplicationsForCandidateAsync(int candidateId);

        /// <summary>
        /// Get applications with feedback
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <returns>Application with feedback</returns>
        Task<Application> GetApplicationWithFeedbackAsync(int applicationId);

        /// <summary>
        /// Check if candidate has applied to job
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <param name="jobId">Job ID</param>
        /// <returns>True if candidate has applied</returns>
        Task<bool> HasCandidateAppliedAsync(int candidateId, int jobId);
    }

    /// <summary>
    /// DTO for application statistics
    /// </summary>
    public class ApplicationStatistics
    {
        public int TotalApplications { get; set; }
        public int Submitted { get; set; }
        public int UnderReview { get; set; }
        public int Shortlisted { get; set; }
        public int InterviewScheduled { get; set; }
        public int Interviewed { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
        public int Withdrawn { get; set; }
        public int OnHold { get; set; }
        public double AverageAIScore { get; set; }
        public double HighestAIScore { get; set; }
        public double LowestAIScore { get; set; }
        public DateTime LastApplicationDate { get; set; }
        public Dictionary<string, int> ApplicationsByDepartment { get; set; }
        public Dictionary<string, int> ApplicationsBySource { get; set; }
        public Dictionary<string, int> ApplicationsByMonth { get; set; }
    }
}