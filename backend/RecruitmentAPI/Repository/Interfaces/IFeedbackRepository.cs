using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces;

/// <summary>
/// Repository interface for InterviewFeedback operations
/// </summary>
public interface IFeedbackRepository
{
    /// <summary>
    /// Get feedback by ID
    /// </summary>
    Task<InterviewFeedback?> GetByIdAsync(int feedbackId);

    /// <summary>
    /// Get all feedback for a specific interview
    /// </summary>
    Task<IEnumerable<InterviewFeedback>> GetByInterviewAsync(int interviewId);

    /// <summary>
    /// Get all feedback submitted by a specific manager
    /// </summary>
    Task<IEnumerable<InterviewFeedback>> GetByManagerAsync(int managerId);

    /// <summary>
    /// Get feedback by decision type (Selected, Rejected, Pending)
    /// </summary>
    Task<IEnumerable<InterviewFeedback>> GetByDecisionAsync(string decision);

    /// <summary>
    /// Get all feedback
    /// </summary>
    Task<IEnumerable<InterviewFeedback>> GetAllAsync();

    /// <summary>
    /// Add new feedback
    /// </summary>
    Task<InterviewFeedback> AddAsync(InterviewFeedback feedback);

    /// <summary>
    /// Update existing feedback
    /// </summary>
    Task UpdateAsync(InterviewFeedback feedback);

    /// <summary>
    /// Delete feedback
    /// </summary>
    Task DeleteAsync(int feedbackId);

    /// <summary>
    /// Check if feedback exists for an interview by a specific manager
    /// </summary>
    Task<bool> ExistsByInterviewAndManagerAsync(int interviewId, int managerId);

    /// <summary>
    /// Get feedback by application ID
    /// </summary>
    Task<IEnumerable<InterviewFeedback>> GetByApplicationIdAsync(int applicationId);
}
