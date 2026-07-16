using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.DTOs.Interview;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations;

/// <summary>
/// Implementation of Feedback service
/// </summary>
public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IInterviewRepository _interviewRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IInterviewRepository interviewRepository,
        ApplicationDbContext context,
        ILogger<FeedbackService> logger)
    {
        _feedbackRepository = feedbackRepository;
        _interviewRepository = interviewRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<FeedbackResponseDto> SubmitFeedbackAsync(FeedbackSubmitDto dto, int managerId)
    {
        try
        {
            // Validate interview exists
            var interview = await _interviewRepository.GetByIdAsync(dto.InterviewId);
            if (interview == null)
            {
                throw new InvalidOperationException($"Interview with ID {dto.InterviewId} not found.");
            }

            // Check if interview is completed
            if (interview.Status != "Completed")
            {
                throw new InvalidOperationException("Feedback can only be submitted for completed interviews.");
            }

            // Check if manager already submitted feedback for this interview
            var existingFeedback = await _feedbackRepository.ExistsByInterviewAndManagerAsync(dto.InterviewId, managerId);
            if (existingFeedback)
            {
                throw new InvalidOperationException("You have already submitted feedback for this interview.");
            }

            // Validate hiring manager exists
            var manager = await _context.HiringManagers.FindAsync(managerId);
            if (manager == null)
            {
                throw new InvalidOperationException("Only hiring managers can submit feedback.");
            }

            // Validate decision
            var validDecisions = new[] { "Selected", "Rejected", "Pending" };
            if (!validDecisions.Contains(dto.Decision))
            {
                throw new InvalidOperationException($"Invalid decision: {dto.Decision}. Must be Selected, Rejected, or Pending.");
            }

            // Create feedback entity
            var feedback = new InterviewFeedback
            {
                InterviewId = dto.InterviewId,
                ManagerId = managerId,
                TechnicalScore = dto.TechnicalScore,
                BehavioralScore = dto.BehavioralScore,
                CommunicationScore = dto.CommunicationScore,
                Comments = dto.Comments,
                Decision = dto.Decision,
                CreatedAt = DateTime.UtcNow
            };

            await _feedbackRepository.AddAsync(feedback);
            await _context.SaveChangesAsync();

            // Update application status to "UnderReview"
            if (interview.Application != null)
            {
                interview.Application.Status = ApplicationStatus.UnderReview;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Feedback {FeedbackId} submitted for interview {InterviewId} by manager {ManagerId}",
                feedback.FeedbackId, dto.InterviewId, managerId);

            return MapToResponseDto(feedback, interview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for interview {InterviewId}", dto.InterviewId);
            throw;
        }
    }

    public async Task<IEnumerable<FeedbackResponseDto>> GetByInterviewAsync(int interviewId)
    {
        try
        {
            var feedbacks = await _feedbackRepository.GetByInterviewAsync(interviewId);
            return feedbacks.Select(f => MapToResponseDto(f));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback for interview {InterviewId}", interviewId);
            throw;
        }
    }

    public async Task<IEnumerable<FeedbackResponseDto>> GetByManagerAsync(int managerId)
    {
        try
        {
            var feedbacks = await _feedbackRepository.GetByManagerAsync(managerId);
            return feedbacks.Select(f => MapToResponseDto(f));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback for manager {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<FeedbackResponseDto> UpdateFeedbackAsync(int feedbackId, FeedbackSubmitDto dto, int managerId)
    {
        try
        {
            var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                throw new InvalidOperationException($"Feedback with ID {feedbackId} not found.");
            }

            // Verify the manager owns this feedback
            if (feedback.ManagerId != managerId)
            {
                throw new UnauthorizedAccessException("You can only update your own feedback.");
            }

            // Validate decision
            var validDecisions = new[] { "Selected", "Rejected", "Pending" };
            if (!validDecisions.Contains(dto.Decision))
            {
                throw new InvalidOperationException($"Invalid decision: {dto.Decision}");
            }

            // Update feedback
            feedback.TechnicalScore = dto.TechnicalScore;
            feedback.BehavioralScore = dto.BehavioralScore;
            feedback.CommunicationScore = dto.CommunicationScore;
            feedback.Comments = dto.Comments;
            feedback.Decision = dto.Decision;
            feedback.UpdatedAt = DateTime.UtcNow;

            await _feedbackRepository.UpdateAsync(feedback);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Feedback {FeedbackId} updated by manager {ManagerId}", feedbackId, managerId);

            return MapToResponseDto(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feedback {FeedbackId}", feedbackId);
            throw;
        }
    }

    public async Task<FeedbackResponseDto?> GetByIdAsync(int feedbackId)
    {
        try
        {
            var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
            return feedback != null ? MapToResponseDto(feedback) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback {FeedbackId}", feedbackId);
            throw;
        }
    }

    public async Task<IEnumerable<FeedbackResponseDto>> GetByDecisionAsync(string decision)
    {
        try
        {
            // Validate decision
            var validDecisions = new[] { "Selected", "Rejected", "Pending" };
            if (!validDecisions.Contains(decision))
            {
                throw new InvalidOperationException($"Invalid decision: {decision}");
            }

            var feedbacks = await _feedbackRepository.GetByDecisionAsync(decision);
            return feedbacks.Select(f => MapToResponseDto(f));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback by decision {Decision}", decision);
            throw;
        }
    }

    #region Helper Methods

    private decimal CalculateAverageScore(InterviewFeedback feedback)
    {
        return (feedback.TechnicalScore + feedback.BehavioralScore + feedback.CommunicationScore) / 3m;
    }

    private FeedbackResponseDto MapToResponseDto(InterviewFeedback feedback, Interview? interview = null)
    {
        var interviewData = interview ?? feedback.Interview;
        var application = interviewData?.Application;
        var candidate = application?.Candidate;
        var jobPosting = application?.Job;
        var manager = feedback.Manager;

        return new FeedbackResponseDto
        {
            FeedbackId = feedback.FeedbackId,
            InterviewId = feedback.InterviewId,
            CandidateName = candidate != null 
                ? $"{candidate.FirstName} {candidate.LastName}" 
                : "N/A",
            JobTitle = jobPosting?.Title ?? "N/A",
            ManagerName = manager != null 
                ? $"{manager.FirstName} {manager.LastName}" 
                : "N/A",
            TechnicalScore = feedback.TechnicalScore,
            BehavioralScore = feedback.BehavioralScore,
            CommunicationScore = feedback.CommunicationScore,
            AverageScore = CalculateAverageScore(feedback),
            Comments = feedback.Comments,
            Decision = feedback.Decision,
            CreatedAt = feedback.CreatedAt
        };
    }

    #endregion
}
