namespace RecruitmentAPI.DTOs.Interview;

/// <summary>
/// DTO for feedback response
/// </summary>
public class FeedbackResponseDto
{
    /// <summary>
    /// Unique identifier for the feedback
    /// </summary>
    public int FeedbackId { get; set; }

    /// <summary>
    /// Interview ID associated with the feedback
    /// </summary>
    public int InterviewId { get; set; }

    /// <summary>
    /// Full name of the candidate
    /// </summary>
    public string CandidateName { get; set; } = string.Empty;

    /// <summary>
    /// Job title for the application
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Name of the hiring manager who provided feedback
    /// </summary>
    public string ManagerName { get; set; } = string.Empty;

    /// <summary>
    /// Technical skills score
    /// </summary>
    public decimal TechnicalScore { get; set; }

    /// <summary>
    /// Behavioral assessment score
    /// </summary>
    public decimal BehavioralScore { get; set; }

    /// <summary>
    /// Communication skills score
    /// </summary>
    public decimal CommunicationScore { get; set; }

    /// <summary>
    /// Average of all scores
    /// </summary>
    public decimal AverageScore { get; set; }

    /// <summary>
    /// Detailed comments
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Final decision (Selected, Rejected, Pending)
    /// </summary>
    public string Decision { get; set; } = string.Empty;

    /// <summary>
    /// When the feedback was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
