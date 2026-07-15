using RecruitmentAPI.DTOs.Interview;

namespace RecruitmentAPI.Services.Interfaces;

/// <summary>
/// Service interface for Interview operations
/// </summary>
public interface IInterviewService
{
    /// <summary>
    /// Schedule a new interview
    /// </summary>
    Task<InterviewResponseDto> ScheduleAsync(ScheduleInterviewDto dto, int scheduledBy);

    /// <summary>
    /// Get interviews by user role (candidate, recruiter, hiring manager)
    /// </summary>
    Task<IEnumerable<InterviewResponseDto>> GetByUserAsync(int userId, string role);

    /// <summary>
    /// Get interview by ID
    /// </summary>
    Task<InterviewResponseDto?> GetByIdAsync(int interviewId);

    /// <summary>
    /// Update interview status
    /// </summary>
    Task<InterviewResponseDto> UpdateStatusAsync(int interviewId, string status, int userId);

    /// <summary>
    /// Cancel an interview
    /// </summary>
    Task<bool> CancelAsync(int interviewId, int userId);

    /// <summary>
    /// Get available time slots (mock implementation)
    /// </summary>
    Task<IEnumerable<DateTime>> GetAvailabilityAsync(DateTime date, int duration);

    /// <summary>
    /// Get interviews by date range
    /// </summary>
    Task<IEnumerable<InterviewResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
