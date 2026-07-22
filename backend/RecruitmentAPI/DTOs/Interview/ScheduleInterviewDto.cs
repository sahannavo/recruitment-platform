using System.ComponentModel.DataAnnotations;

namespace RecruitmentAPI.DTOs.Interview;

/// <summary>
/// DTO for scheduling a new interview
/// </summary>
public class ScheduleInterviewDto
{
    /// <summary>
    /// The application ID for which the interview is being scheduled
    /// </summary>
    [Required(ErrorMessage = "Application ID is required")]
    public int ApplicationId { get; set; }

    /// <summary>
    /// Scheduled date and time for the interview
    /// </summary>
    [Required(ErrorMessage = "Scheduled date and time is required")]
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// Duration of the interview in minutes
    /// </summary>
    [Required(ErrorMessage = "Duration is required")]
    [Range(15, 480, ErrorMessage = "Duration must be between 15 minutes and 8 hours")]
    public int Duration { get; set; }

    /// <summary>
    /// Type of interview (Online, Physical, Technical, HR)
    /// </summary>
    [Required(ErrorMessage = "Interview type is required")]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Time zone for the interview
    /// </summary>
    [StringLength(100)]
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// Meeting link or connection instructions for online interviews
    /// </summary>
    [StringLength(500)]
    public string? MeetingLink { get; set; }

    /// <summary>
    /// Additional notes for the interview
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Optional Interviewer ID to assign this interview to a specific Hiring Manager
    /// </summary>
    public int? InterviewerId { get; set; }
}
