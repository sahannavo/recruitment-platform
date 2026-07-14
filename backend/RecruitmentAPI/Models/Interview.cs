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

    [Required]
    [Range(15, 480)]
    public int Duration { get; set; } // Duration in minutes

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // Online, Physical, Technical, HR

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

    [StringLength(500)]
    public string? MeetingLink { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("ApplicationId")]
    public virtual Application? Application { get; set; }

    public virtual ICollection<InterviewFeedback> Feedbacks { get; set; } = new List<InterviewFeedback>();
}
