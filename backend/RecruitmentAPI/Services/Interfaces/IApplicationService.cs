using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Services.Interfaces
{
    /// <summary>
    /// Service interface for application operations
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Submit a new job application
        /// </summary>
        /// <param name="submitDto">Application submission data</param>
        /// <returns>Application response</returns>
        Task<ApplicationResponseDto> SubmitApplicationAsync(ApplicationSubmitDto submitDto);

        /// <summary>
        /// Get all applications by candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>List of applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetByCandidateAsync(int candidateId);

        /// <summary>
        /// Get all applications for a specific job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>List of applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetByJobAsync(int jobId);

        /// <summary>
        /// Update application status
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <param name="updateDto">Status update data</param>
        /// <returns>Updated application</returns>
        Task<ApplicationResponseDto> UpdateStatusAsync(int applicationId, ApplicationStatusUpdateDto updateDto);

        /// <summary>
        /// Withdraw an application
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <param name="candidateId">Candidate ID (for authorization)</param>
        /// <returns>True if withdrawn successfully</returns>
        Task<bool> WithdrawAsync(int applicationId, int candidateId);

        /// <summary>
        /// Get application by ID
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <returns>Application response</returns>
        Task<ApplicationResponseDto> GetByIdAsync(int applicationId);

        /// <summary>
        /// Get applications by status
        /// </summary>
        /// <param name="status">Application status</param>
        /// <returns>List of applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetByStatusAsync(ApplicationStatus status);

        /// <summary>
        /// Get applications with AI score above threshold
        /// </summary>
        /// <param name="threshold">Minimum AI score</param>
        /// <returns>List of applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetWithHighAIScoreAsync(double threshold);

        /// <summary>
        /// Get application statistics for dashboard
        /// </summary>
        /// <returns>Application statistics</returns>
        Task<ApplicationStatistics> GetApplicationStatisticsAsync();

        /// <summary>
        /// Get applications by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get applications for a recruiter
        /// </summary>
        /// <param name="recruiterId">Recruiter ID</param>
        /// <returns>List of applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetByRecruiterAsync(int recruiterId);

        /// <summary>
        /// Get recent applications
        /// </summary>
        /// <param name="days">Number of days</param>
        /// <returns>List of recent applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetRecentApplicationsAsync(int days);

        /// <summary>
        /// Get applications pending review
        /// </summary>
        /// <returns>List of applications pending review</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetPendingReviewApplicationsAsync();

        /// <summary>
        /// Get applications with interviews scheduled
        /// </summary>
        /// <returns>List of applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetApplicationsWithInterviewsAsync();

        /// <summary>
        /// Search applications by candidate name or job title
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> SearchApplicationsAsync(string searchTerm);

        /// <summary>
        /// Bulk update application status
        /// </summary>
        /// <param name="applicationIds">List of application IDs</param>
        /// <param name="status">New status</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>Number of applications updated</returns>
        Task<int> BulkUpdateStatusAsync(List<int> applicationIds, ApplicationStatus status, string notes = null);

        /// <summary>
        /// Get application with full details including interviews and feedback
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <returns>Application with details</returns>
        Task<ApplicationWithInterviewDto> GetApplicationWithDetailsAsync(int applicationId);

        /// <summary>
        /// Get application count by status for a job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Dictionary of status and count</returns>
        Task<Dictionary<ApplicationStatus, int>> GetApplicationCountByStatusAsync(int jobId);

        /// <summary>
        /// Get applications by AI score range
        /// </summary>
        /// <param name="minScore">Minimum score</param>
        /// <param name="maxScore">Maximum score</param>
        /// <returns>List of applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetByAIScoreRangeAsync(double minScore, double maxScore);

        /// <summary>
        /// Get application with status history
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <returns>Application with status history</returns>
        Task<ApplicationResponseDto> GetApplicationWithStatusHistoryAsync(int applicationId);

        /// <summary>
        /// Check if candidate has applied to a job
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <param name="jobId">Job ID</param>
        /// <returns>True if applied</returns>
        Task<bool> HasCandidateAppliedAsync(int candidateId, int jobId);

        /// <summary>
        /// Get total application count for a job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Total applications count</returns>
        Task<int> GetApplicationCountForJobAsync(int jobId);

        /// <summary>
        /// Get active applications for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>List of active applications</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetActiveApplicationsForCandidateAsync(int candidateId);

        /// <summary>
        /// Get application statistics by department
        /// </summary>
        /// <returns>Department statistics</returns>
        Task<Dictionary<string, ApplicationStatistics>> GetStatisticsByDepartmentAsync();

        /// <summary>
        /// Get application timeline for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate ID</param>
        /// <returns>Application timeline</returns>
        Task<IEnumerable<ApplicationTimelineDto>> GetApplicationTimelineAsync(int candidateId);

        /// <summary>
        /// Recalculate AI scores for all applications of a job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Number of applications updated</returns>
        Task<int> RecalculateAIScoresForJobAsync(int jobId);
    }

    /// <summary>
    /// DTO for application timeline
    /// </summary>
    public class ApplicationTimelineDto
    {
        public int ApplicationId { get; set; }
        public string JobTitle { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ShortlistedAt { get; set; }
        public DateTime? InterviewedAt { get; set; }
        public DateTime? HiredAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string CurrentStatus { get; set; }
        public List<StatusChangeDto> StatusChanges { get; set; }
    }

    /// <summary>
    /// DTO for status change
    /// </summary>
    public class StatusChangeDto
    {
        public string FromStatus { get; set; }
        public string ToStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
        public string Notes { get; set; }
    }
}