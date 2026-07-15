using System.ComponentModel.DataAnnotations;

namespace RecruitmentAPI.DTOs.Interview;

/// <summary>
/// DTO for submitting interview feedback
/// </summary>
public class FeedbackSubmitDto
{
    /// <summary>
    /// The interview ID for which feedback is being submitted
    /// </summary>
    [Required(ErrorMessage = "Interview ID is required")]
    public int InterviewId { get; set; }

    /// <summary>
    /// Technical skills score (0-10)
    /// </summary>
    [Required(ErrorMessage = "Technical score is required")]
    [Range(0, 10, ErrorMessage = "Technical score must be between 0 and 10")]
    public decimal TechnicalScore { get; set; }

    /// <summary>
    /// Behavioral assessment score (0-10)
    /// </summary>
    [Required(ErrorMessage = "Behavioral score is required")]
    [Range(0, 10, ErrorMessage = "Behavioral score must be between 0 and 10")]
    public decimal BehavioralScore { get; set; }

    /// <summary>
    /// Communication skills score (0-10)
    /// </summary>
    [Required(ErrorMessage = "Communication score is required")]
    [Range(0, 10, ErrorMessage = "Communication score must be between 0 and 10")]
    public decimal CommunicationScore { get; set; }

    /// <summary>
    /// Detailed comments about the interview
    /// </summary>
    [StringLength(2000, ErrorMessage = "Comments cannot exceed 2000 characters")]
    public string? Comments { get; set; }

    /// <summary>
    /// Final decision (Selected, Rejected, Pending)
    /// </summary>
    [Required(ErrorMessage = "Decision is required")]
    [StringLength(50)]
    public string Decision { get; set; } = string.Empty;
}
