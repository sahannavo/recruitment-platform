using RecruitmentAPI.DTOs.Interview;

namespace RecruitmentAPI.Services.Interfaces;

/// <summary>
/// Service interface for Feedback operations
/// </summary>
public interface IFeedbackService
{
    /// <summary>
    /// Submit feedback for an interview
    /// </summary>
    Task<FeedbackResponseDto> SubmitFeedbackAsync(FeedbackSubmitDto dto, int managerId);

    /// <summary>
    /// Get all feedback for a specific interview
    /// </summary>
    Task<IEnumerable<FeedbackResponseDto>> GetByInterviewAsync(int interviewId);

    /// <summary>
    /// Get all feedback submitted by a specific manager
    /// </summary>
    Task<IEnumerable<FeedbackResponseDto>> GetByManagerAsync(int managerId);

    /// <summary>
    /// Update existing feedback
    /// </summary>
    Task<FeedbackResponseDto> UpdateFeedbackAsync(int feedbackId, FeedbackSubmitDto dto, int managerId);

    /// <summary>
    /// Get feedback by ID
    /// </summary>
    Task<FeedbackResponseDto?> GetByIdAsync(int feedbackId);

    /// <summary>
    /// Get feedback by decision type
    /// </summary>
    Task<IEnumerable<FeedbackResponseDto>> GetByDecisionAsync(string decision);
}
