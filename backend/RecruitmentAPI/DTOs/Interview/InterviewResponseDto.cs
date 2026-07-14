namespace RecruitmentAPI.DTOs.Interview;

/// <summary>
/// DTO for interview response
/// </summary>
public class InterviewResponseDto
{
    /// <summary>
    /// Unique identifier for the interview
    /// </summary>
    public int InterviewId { get; set; }

    /// <summary>
    /// Application ID associated with the interview
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Full name of the candidate
    /// </summary>
    public string CandidateName { get; set; } = string.Empty;

    /// <summary>
    /// Email of the candidate
    /// </summary>
    public string CandidateEmail { get; set; } = string.Empty;

    /// <summary>
    /// Job title for the application
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled date and time
    /// </summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Interview type (Online, Physical, Technical, HR)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the interview
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Meeting link for online interviews
    /// </summary>
    public string? MeetingLink { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When the interview was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Number of feedback submissions
    /// </summary>
    public int FeedbackCount { get; set; }
}
