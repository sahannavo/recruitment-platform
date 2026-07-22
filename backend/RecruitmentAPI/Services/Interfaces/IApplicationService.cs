using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

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
        Task<ApplicationResponseDto> SubmitApplicationAsync(ApplicationSubmitDto submitDto);

        /// <summary>
        /// Get all applications by candidate
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetByCandidateAsync(int candidateId);

        /// <summary>
        /// Get all applications for a specific job
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetByJobAsync(int jobId);

        /// <summary>
        /// Update application status
        /// </summary>
        Task<ApplicationResponseDto> UpdateStatusAsync(int applicationId, ApplicationStatusUpdateDto updateDto);

        /// <summary>
        /// Withdraw an application
        /// </summary>
        Task<bool> WithdrawAsync(int applicationId, int userId);

        /// <summary>
        /// Get application by ID
        /// </summary>
        Task<ApplicationResponseDto> GetByIdAsync(int applicationId);

        /// <summary>
        /// Get applications by status
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetByStatusAsync(ApplicationStatus status);

        /// <summary>
        /// Get applications with AI score above threshold
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetWithHighAIScoreAsync(double threshold);

        /// <summary>
        /// Get application statistics for dashboard
        /// </summary>
        Task<RecruitmentAPI.DTOs.ApplicationStatistics> GetApplicationStatisticsAsync();

        /// <summary>
        /// Get applications by date range
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get applications for a recruiter
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetByRecruiterAsync(int userId);

        /// <summary>
        /// Get recent applications
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetRecentApplicationsAsync(int days);

        /// <summary>
        /// Get applications pending review
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetPendingReviewApplicationsAsync();

        /// <summary>
        /// Get applications shortlisted and pending manager review
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetManagerReviewApplicationsAsync();

        /// <summary>
        /// Get applications with interviews scheduled
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetApplicationsWithInterviewsAsync();

        /// <summary>
        /// Search applications by candidate name or job title
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> SearchApplicationsAsync(string searchTerm);

        /// <summary>
        /// Bulk update application status
        /// </summary>
        Task<int> BulkUpdateStatusAsync(List<int> applicationIds, ApplicationStatus status, string notes = null);

        /// <summary>
        /// Get application with full details including interviews and feedback
        /// </summary>
        Task<ApplicationWithInterviewDto> GetApplicationWithDetailsAsync(int applicationId);

        /// <summary>
        /// Get application count by status for a job
        /// </summary>
        Task<Dictionary<ApplicationStatus, int>> GetApplicationCountByStatusAsync(int jobId);

        /// <summary>
        /// Get applications by AI score range
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetByAIScoreRangeAsync(double minScore, double maxScore);

        /// <summary>
        /// Get application with status history
        /// </summary>
        Task<ApplicationResponseDto> GetApplicationWithStatusHistoryAsync(int applicationId);

        /// <summary>
        /// Check if candidate has applied to a job
        /// </summary>
        Task<bool> HasCandidateAppliedAsync(int candidateId, int jobId);

        /// <summary>
        /// Get total application count for a job
        /// </summary>
        Task<int> GetApplicationCountForJobAsync(int jobId);

        /// <summary>
        /// Get active applications for a candidate
        /// </summary>
        Task<IEnumerable<ApplicationResponseDto>> GetActiveApplicationsForCandidateAsync(int candidateId);

        /// <summary>
        /// Get application statistics by department
        /// </summary>
        Task<Dictionary<string, RecruitmentAPI.DTOs.ApplicationStatistics>> GetStatisticsByDepartmentAsync();

        /// <summary>
        /// Get application timeline for a candidate
        /// </summary>
        Task<IEnumerable<ApplicationTimelineDto>> GetApplicationTimelineAsync(int candidateId);

        /// <summary>
        /// Recalculate AI scores for all applications of a job
        /// </summary>
        Task<int> RecalculateAIScoresForJobAsync(int jobId);
    }
}