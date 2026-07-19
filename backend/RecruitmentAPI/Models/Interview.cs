using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Models;

/// <summary>
/// Represents a scheduled interview for a job application
/// </summary>
public class Interview
{
    [Key]
    public int InterviewId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    //  Alias for ScheduledAt (some code uses ScheduledDate)
    [NotMapped]
    public DateTime ScheduledDate
    {
        get => ScheduledAt;
        set => ScheduledAt = value;
    }

    [Required]
    [Range(15, 480)]
    public int Duration { get; set; } = 60; // Duration in minutes

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = "Online"; // Online, Physical, Technical, HR

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

    //  Location for physical interviews
    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? MeetingLink { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    // Interviewer ID
    public int? InterviewerId { get; set; }

    // Navigation property for Interviewer
    [ForeignKey("InterviewerId")]
    public virtual User? Interviewer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("ApplicationId")]
    public virtual Application? Application { get; set; }

    public virtual ICollection<InterviewFeedback> Feedbacks { get; set; } = new List<InterviewFeedback>();
}