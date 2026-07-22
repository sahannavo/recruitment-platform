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
using RecruitmentAPI.Services.Notification;

namespace RecruitmentAPI.Services.Implementations;

/// <summary>
/// Service implementation for application operations
/// </summary>
public class ApplicationService : IApplicationService
{
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ApplicationService> _logger;

        public ApplicationService(
            IUnitOfWork unitOfWork,
            IAIService aiService,
            INotificationService notificationService,
            ILogger<ApplicationService> logger)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Submit a new job application
        /// </summary>
        public async Task<ApplicationResponseDto> SubmitApplicationAsync(ApplicationSubmitDto submitDto)
        {
            try
            {
                // Validate candidate exists
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(submitDto.CandidateId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {submitDto.CandidateId} not found");

                // Validate job exists and is open
                var job = await _unitOfWork.Jobs.GetByIdAsync(submitDto.JobId);
                if (job == null)
                    throw new KeyNotFoundException($"Job with ID {submitDto.JobId} not found");

                if (job.Status != JobStatus.Open && job.Status != JobStatus.Published)
                    throw new InvalidOperationException("This job is no longer accepting applications");

                // Check if candidate already applied
                var existingApplication = await _unitOfWork.Applications
                    .GetByCandidateAndJobAsync(candidate.CandidateId, submitDto.JobId);
                if (existingApplication != null)
                    throw new InvalidOperationException("You have already applied for this position");

                // Create the application
                var application = new Application
                {
                    CandidateId = candidate.CandidateId,
                    JobId = submitDto.JobId,
                    Status = ApplicationStatus.Submitted,
                    AppliedAt = DateTime.UtcNow,
                    Notes = submitDto.Notes ?? "",
                    Source = submitDto.Source ?? "Direct",
                    RejectionReason = "",
                    ExpectedSalary = string.IsNullOrEmpty(submitDto.ExpectedSalary) ? null : decimal.Parse(submitDto.ExpectedSalary),
                    AvailableFrom = submitDto.AvailableFrom
                };

                // Calculate AI score
                var candidateSkillsText = candidate.SkillsSummary ?? "";
                var jobRequirementsText = job.RequiredSkills ?? "";
                
                var aiScore = await _aiService.MatchCandidateToJobAsync(
                    candidate.CandidateId, 
                    candidateSkillsText,
                    submitDto.JobId, 
                    jobRequirementsText);
                application.AI_Score = aiScore.Score;

                await _unitOfWork.Applications.AddAsync(application);
                await _unitOfWork.SaveChangesAsync();

                // Send notification
                await _notificationService.SendApplicationSubmittedAsync(candidate.User.Email, job.Title);

                _logger.LogInformation("Application {ApplicationId} submitted for job {JobId} by candidate {CandidateId}",
                    application.ApplicationId, submitDto.JobId, submitDto.CandidateId);

                return await GetByIdAsync(application.ApplicationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting application for job {JobId} by candidate {CandidateId}",
                    submitDto.JobId, submitDto.CandidateId);
                throw;
            }
        }

        /// <summary>
        /// Get application by ID
        /// </summary>
        public async Task<ApplicationResponseDto> GetByIdAsync(int applicationId)
        {
            try
            {
                var application = await _unitOfWork.Applications.GetApplicationWithDetailsAsync(applicationId);
                if (application == null)
                    throw new KeyNotFoundException($"Application with ID {applicationId} not found");

                return new ApplicationResponseDto
                {
                    ApplicationId = application.ApplicationId,
                    JobId = application.JobId,
                    JobTitle = application.Job.Title,
                    Company = application.Job.Department,
                    Department = application.Job.Department,
                    Location = application.Job.Location,
                    Status = application.Status.ToString(),
                    AppliedAt = application.AppliedAt,
                    UpdatedAt = application.UpdatedAt,
                    AI_Score = application.AI_Score,
                    CandidateId = application.CandidateId,
                    CandidateName = $"{application.Candidate.User.FirstName} {application.Candidate.User.LastName}",
                    CandidateEmail = application.Candidate.User.Email,
                    CandidateResumeUrl = application.Candidate.Documents?.Any(d => (d.DocumentType == "CV" || d.DocumentType == "Resume") && d.IsActive) == true ? $"/api/applications/{application.ApplicationId}/resume/download" : null,
                    Notes = application.Notes,
                    Source = application.Source,
                    ExpectedSalary = application.ExpectedSalary?.ToString(),
                    AvailableFrom = application.AvailableFrom,
                    Success = true,
                    Message = "Application retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application by ID {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Get all applications by candidate
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetByCandidateAsync(int candidateId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(candidateId);
                if (candidate == null) return new List<ApplicationResponseDto>();
                
                var applications = await _unitOfWork.Applications.GetCandidateApplicationsWithJobDetailsAsync(candidate.CandidateId);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Department = a.Job.Department,
                    Location = a.Job.Location,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    UpdatedAt = a.UpdatedAt,
                    AI_Score = a.AI_Score,
                    CandidateId = a.CandidateId,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Notes = a.Notes,
                    Source = a.Source,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications for candidate {CandidateId}", candidateId);
                throw;
            }
        }



        /// <summary>
        /// Get all applications for a specific job
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetByJobAsync(int jobId)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetByJobAsync(jobId);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Department = a.Job.Department,
                    Location = a.Job.Location,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    UpdatedAt = a.UpdatedAt,
                    AI_Score = a.AI_Score,
                    CandidateId = a.CandidateId,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Notes = a.Notes,
                    Source = a.Source,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications for job {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Update application status
        /// </summary>
        public async Task<ApplicationResponseDto> UpdateStatusAsync(int applicationId, ApplicationStatusUpdateDto updateDto)
        {
            try
            {
                var application = await _unitOfWork.Applications.GetApplicationWithDetailsAsync(applicationId);
                if (application == null)
                    throw new KeyNotFoundException($"Application with ID {applicationId} not found");

                if (!Enum.TryParse<ApplicationStatus>(updateDto.Status, true, out var newStatus))
                    throw new ArgumentException($"Invalid status: {updateDto.Status}");

                // Validate status transition
                if (!IsValidStatusTransition(application.Status, newStatus))
                    throw new InvalidOperationException($"Invalid status transition from {application.Status} to {newStatus}");

                application.Status = newStatus;
                application.UpdatedAt = DateTime.UtcNow;

                // Update timestamps based on status
                switch (newStatus)
                {
                    case ApplicationStatus.UnderReview:
                        application.ReviewedAt = DateTime.UtcNow;
                        application.ReviewedBy = updateDto.ReviewedBy;
                        break;
                    case ApplicationStatus.Shortlisted:
                        application.ShortlistedAt = DateTime.UtcNow;
                        break;
                    case ApplicationStatus.Interviewed:
                        application.InterviewedAt = DateTime.UtcNow;
                        break;
                    case ApplicationStatus.Hired:
                        application.HiredAt = DateTime.UtcNow;
                        break;
                    case ApplicationStatus.Rejected:
                        application.RejectedAt = DateTime.UtcNow;
                        application.RejectionReason = updateDto.RejectionReason;
                        break;
                }

                if (!string.IsNullOrEmpty(updateDto.Notes))
                    application.Notes = updateDto.Notes;

                await _unitOfWork.SaveChangesAsync();

                // Send notification based on status
                await SendStatusNotificationAsync(application);

                _logger.LogInformation("Application {ApplicationId} status updated to {NewStatus}",
                    applicationId, newStatus);

                return await GetByIdAsync(applicationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application {ApplicationId} status", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Withdraw an application
        /// </summary>
        public async Task<bool> WithdrawAsync(int applicationId, int userId)
        {
            try
            {
                var application = await _unitOfWork.Applications.GetApplicationWithDetailsAsync(applicationId);
                if (application == null)
                    return false;

                // Verify ownership (compare against UserId since the parameter is the user's ID)
                if (application.Candidate == null || application.Candidate.UserId != userId)
                    throw new UnauthorizedAccessException("You can only withdraw your own applications");

                if (application.Status == ApplicationStatus.Hired || application.Status == ApplicationStatus.Rejected)
                    throw new InvalidOperationException($"Cannot withdraw application with status {application.Status}");

                application.Status = ApplicationStatus.Withdrawn;
                application.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                // Send notification
                await _notificationService.SendApplicationWithdrawnAsync(
                    application.Candidate.User.Email,
                    application.Job.Title);

                _logger.LogInformation("Application {ApplicationId} withdrawn by user {UserId}",
                    applicationId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing application {ApplicationId} by user {UserId}",
                    applicationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Get applications by status
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetByStatusAsync(ApplicationStatus status)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetByStatusAsync(status);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications by status {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Get applications with AI score above threshold
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetWithHighAIScoreAsync(double threshold)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetWithAI_ScoreAsync(threshold);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications with high AI score above {Threshold}", threshold);
                throw;
            }
        }

        /// <summary>
        /// Get application statistics for dashboard
        /// </summary>
        public async Task<RecruitmentAPI.DTOs.ApplicationStatistics> GetApplicationStatisticsAsync()
        {
            try
            {
                return await _unitOfWork.Applications.GetApplicationStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application statistics");
                throw;
            }
        }

        /// <summary>
        /// Get applications by date range
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetByDateRangeAsync(startDate, endDate);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications by date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        /// <summary>
        /// Get applications for a recruiter
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetByRecruiterAsync(int userId)
        {
            try
            {
                var recruiter = await _unitOfWork.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
                if (recruiter == null) return new List<ApplicationResponseDto>();

                var applications = await _unitOfWork.Applications.GetByRecruiterAsync(recruiter.RecruiterId);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications for recruiter user ID {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get recent applications
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetRecentApplicationsAsync(int days)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetRecentApplicationsAsync(days);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent applications for {Days} days", days);
                throw;
            }
        }

        /// <summary>
        /// Get applications pending review
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetPendingReviewApplicationsAsync()
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetPendingReviewApplicationsAsync();
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending review applications");
                throw;
            }
        }

        /// <summary>
        /// Get applications shortlisted and pending manager review
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetManagerReviewApplicationsAsync()
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetManagerReviewApplicationsAsync();
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Manager review applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manager review applications");
                throw;
            }
        }

        /// <summary>
        /// Get applications with interviews scheduled
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetApplicationsWithInterviewsAsync()
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetApplicationsWithInterviewsAsync();
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications with interviews");
                throw;
            }
        }

        /// <summary>
        /// Search applications by candidate name or job title
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> SearchApplicationsAsync(string searchTerm)
        {
            try
            {
                var applications = await _unitOfWork.Applications.SearchApplicationsAsync(searchTerm);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching applications with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Bulk update application status
        /// </summary>
        public async Task<int> BulkUpdateStatusAsync(List<int> applicationIds, ApplicationStatus status, string notes = null)
        {
            try
            {
                var count = await _unitOfWork.Applications.BulkUpdateStatusAsync(applicationIds, status, notes);

                _logger.LogInformation("Bulk updated {Count} applications to status {Status}", count, status);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk update application status");
                throw;
            }
        }

        /// <summary>
        /// Get application with full details including interviews and feedback
        /// </summary>
        public async Task<ApplicationWithInterviewDto> GetApplicationWithDetailsAsync(int applicationId)
        {
            try
            {
                var baseResponse = await GetByIdAsync(applicationId);
                var application = await _unitOfWork.Applications.GetApplicationWithDetailsAsync(applicationId);

                var interviews = await _unitOfWork.Interviews.GetByApplicationIdAsync(applicationId);
                var feedbacks = await _unitOfWork.Feedbacks.GetByApplicationIdAsync(applicationId);

                return new ApplicationWithInterviewDto
                {
                    ApplicationId = baseResponse.ApplicationId,
                    JobId = baseResponse.JobId,
                    JobTitle = baseResponse.JobTitle,
                    Company = baseResponse.Company,
                    Status = baseResponse.Status,
                    AppliedAt = baseResponse.AppliedAt,
                    AI_Score = baseResponse.AI_Score,
                    CandidateName = baseResponse.CandidateName,
                    CandidateEmail = baseResponse.CandidateEmail,
                    Message = baseResponse.Message,
                    Success = baseResponse.Success,
                    ScoreBreakdown = baseResponse.ScoreBreakdown,
                    Department = baseResponse.Department,
                    Location = baseResponse.Location,

                    Interview = interviews.FirstOrDefault() != null ? new InterviewDetailsDto
                    {
                        InterviewId = interviews.FirstOrDefault()!.InterviewId,
                        ScheduledAt = interviews.FirstOrDefault()!.ScheduledAt,
                        Duration = interviews.FirstOrDefault()!.Duration,
                        Type = interviews.FirstOrDefault()!.Type,
                        Status = interviews.FirstOrDefault()!.Status.ToString(),
                        MeetingLink = interviews.FirstOrDefault()!.MeetingLink
                    } : null,

                    Feedback = feedbacks.FirstOrDefault() != null ? new FeedbackDetailsDto
                    {
                        FeedbackId = feedbacks.FirstOrDefault()!.FeedbackId,
                        TechnicalScore = (double)feedbacks.FirstOrDefault()!.TechnicalScore,
                        BehavioralScore = (double)feedbacks.FirstOrDefault()!.BehavioralScore,
                        CommunicationScore = (double)feedbacks.FirstOrDefault()!.CommunicationScore,
                        AverageScore = (double)(feedbacks.FirstOrDefault()!.TechnicalScore + feedbacks.FirstOrDefault()!.BehavioralScore + feedbacks.FirstOrDefault()!.CommunicationScore) / 3,
                        Comments = feedbacks.FirstOrDefault()!.Comments,
                        Decision = feedbacks.FirstOrDefault()!.Decision,
                        CreatedAt = feedbacks.FirstOrDefault()!.CreatedAt
                    } : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application with details for {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Get application count by status for a job
        /// </summary>
        public async Task<Dictionary<ApplicationStatus, int>> GetApplicationCountByStatusAsync(int jobId)
        {
            try
            {
                return await _unitOfWork.Applications.GetApplicationCountByStatusAsync(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application count by status for job {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Get applications by AI score range
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetByAIScoreRangeAsync(double minScore, double maxScore)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetByAIScoreRangeAsync(minScore, maxScore);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications by AI score range {MinScore} to {MaxScore}", minScore, maxScore);
                throw;
            }
        }

        /// <summary>
        /// Get application with status history
        /// </summary>
        public async Task<ApplicationResponseDto> GetApplicationWithStatusHistoryAsync(int applicationId)
        {
            // Placeholder - would need a status history table
            return await GetByIdAsync(applicationId);
        }

        /// <summary>
        /// Check if candidate has applied to a job
        /// </summary>
        public async Task<bool> HasCandidateAppliedAsync(int candidateId, int jobId)
        {
            try
            {
                return await _unitOfWork.Applications.HasCandidateAppliedAsync(candidateId, jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate {CandidateId} applied to job {JobId}", candidateId, jobId);
                throw;
            }
        }

        /// <summary>
        /// Get total application count for a job
        /// </summary>
        public async Task<int> GetApplicationCountForJobAsync(int jobId)
        {
            try
            {
                return await _unitOfWork.Applications.GetApplicationCountForJobAsync(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application count for job {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Get active applications for a candidate
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetActiveApplicationsForCandidateAsync(int candidateId)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetActiveApplicationsForCandidateAsync(candidateId);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateName = $"{a.Candidate.User.FirstName} {a.Candidate.User.LastName}",
                    CandidateEmail = a.Candidate.User.Email,
                    Success = true,
                    Message = "Applications retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active applications for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        /// <summary>
        /// Get application statistics by department
        /// </summary>
        public async Task<Dictionary<string, RecruitmentAPI.DTOs.ApplicationStatistics>> GetStatisticsByDepartmentAsync()
        {
            try
            {
                var stats = new Dictionary<string, RecruitmentAPI.DTOs.ApplicationStatistics>();
                var departments = await _unitOfWork.Jobs.GetDepartmentJobCountsAsync();

                foreach (var dept in departments.Keys)
                {
                    var applications = await _unitOfWork.Applications.GetByDepartmentAsync(dept);
                    var appList = applications.ToList();

                    stats[dept] = new RecruitmentAPI.DTOs.ApplicationStatistics
                    {
                        TotalApplications = appList.Count,
                        PendingReview = appList.Count(a => a.Status == ApplicationStatus.Submitted),
                        Shortlisted = appList.Count(a => a.Status == ApplicationStatus.Shortlisted),
                        InterviewScheduled = appList.Count(a => a.Status == ApplicationStatus.InterviewScheduled),
                        Hired = appList.Count(a => a.Status == ApplicationStatus.Hired),
                        Rejected = appList.Count(a => a.Status == ApplicationStatus.Rejected)
                    };

                    if (appList.Any(a => a.AI_Score.HasValue))
                    {
                        var scores = appList.Where(a => a.AI_Score.HasValue).Select(a => (decimal)a.AI_Score.Value);
                        stats[dept].AverageAIScore = scores.Average();
                    }
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application statistics by department");
                throw;
            }
        }

        /// <summary>
        /// Get application timeline for a candidate
        /// </summary>
        public async Task<IEnumerable<ApplicationTimelineDto>> GetApplicationTimelineAsync(int candidateId)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetCandidateApplicationsWithJobDetailsAsync(candidateId);

                return applications.Select(a => new ApplicationTimelineDto
                {
                    ApplicationId = a.ApplicationId,
                    JobTitle = a.Job.Title,
                    AppliedAt = a.AppliedAt,
                    ReviewedAt = a.ReviewedAt,
                    ShortlistedAt = a.ShortlistedAt,
                    InterviewedAt = a.InterviewedAt,
                    HiredAt = a.HiredAt,
                    RejectedAt = a.RejectedAt,
                    CurrentStatus = a.Status.ToString(),
                    StatusChanges = new List<StatusChangeDto>() // Would need status history table
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application timeline for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        /// <summary>
        /// Recalculate AI scores for all applications of a job
        /// </summary>
        public async Task<int> RecalculateAIScoresForJobAsync(int jobId)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetByJobAsync(jobId);
                var count = 0;

                foreach (var application in applications)
                {
                var aiScore = await _aiService.MatchCandidateToJobAsync(application.CandidateId, "", jobId, "");
                    application.AI_Score = aiScore.Score;
                    application.UpdatedAt = DateTime.UtcNow;
                    count++;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Recalculated AI scores for {Count} applications for job {JobId}", count, jobId);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating AI scores for job {JobId}", jobId);
                throw;
            }
        }

        #region Private Methods

        /// <summary>
        /// Validate status transition
        /// </summary>
        private bool IsValidStatusTransition(ApplicationStatus current, ApplicationStatus next)
        {
            var validTransitions = new Dictionary<ApplicationStatus, List<ApplicationStatus>>
            {
                { ApplicationStatus.Submitted, new List<ApplicationStatus> { ApplicationStatus.UnderReview, ApplicationStatus.Shortlisted, ApplicationStatus.Withdrawn } },
                { ApplicationStatus.UnderReview, new List<ApplicationStatus> { ApplicationStatus.Shortlisted, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn } },
                { ApplicationStatus.Shortlisted, new List<ApplicationStatus> { ApplicationStatus.ManagerApproved, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn } },
                { ApplicationStatus.ManagerApproved, new List<ApplicationStatus> { ApplicationStatus.InterviewScheduled, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn } },
                { ApplicationStatus.InterviewScheduled, new List<ApplicationStatus> { ApplicationStatus.Interviewed, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn } },
                { ApplicationStatus.Interviewed, new List<ApplicationStatus> { ApplicationStatus.Hired, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn } },
                { ApplicationStatus.Hired, new List<ApplicationStatus>() },
                { ApplicationStatus.Rejected, new List<ApplicationStatus>() },
                { ApplicationStatus.Withdrawn, new List<ApplicationStatus>() },
                { ApplicationStatus.OnHold, new List<ApplicationStatus> { ApplicationStatus.UnderReview, ApplicationStatus.Rejected } }
            };

            return validTransitions.ContainsKey(current) && validTransitions[current].Contains(next);
        }

        /// <summary>
        /// Send notification based on status
        /// </summary>
        private async Task SendStatusNotificationAsync(Application application)
        {
            try
            {
                var candidateEmail = application.Candidate.User.Email;
                var jobTitle = application.Job.Title;

                switch (application.Status)
                {
                    case ApplicationStatus.UnderReview:
                        await _notificationService.SendApplicationUnderReviewAsync(candidateEmail, jobTitle);
                        break;
                    case ApplicationStatus.Shortlisted:
                        await _notificationService.SendApplicationShortlistedAsync(candidateEmail, jobTitle);
                        break;
                    case ApplicationStatus.Hired:
                        await _notificationService.SendApplicationHiredAsync(candidateEmail, jobTitle);
                        break;
                    case ApplicationStatus.Rejected:
                        await _notificationService.SendApplicationRejectedAsync(candidateEmail, jobTitle, application.RejectionReason);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for application {ApplicationId}", application.ApplicationId);
            }
        }

        #endregion
    }
