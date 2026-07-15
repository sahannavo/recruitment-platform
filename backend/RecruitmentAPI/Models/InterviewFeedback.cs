using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Models;

/// <summary>
/// Represents feedback provided by a hiring manager after an interview
/// </summary>
public class InterviewFeedback
{
    [Key]
    public int FeedbackId { get; set; }

    [Required]
    public int InterviewId { get; set; }

    [Required]
    public int ManagerId { get; set; } // User ID of the hiring manager

    [Required]
    [Range(0, 10)]
    [Column(TypeName = "decimal(3,1)")]
    public decimal TechnicalScore { get; set; }

    [Required]
    [Range(0, 10)]
    [Column(TypeName = "decimal(3,1)")]
    public decimal BehavioralScore { get; set; }

    [Required]
    [Range(0, 10)]
    [Column(TypeName = "decimal(3,1)")]
    public decimal CommunicationScore { get; set; }

    [StringLength(2000)]
    public string? Comments { get; set; }

    [Required]
    [StringLength(50)]
    public string Decision { get; set; } = string.Empty; // Selected, Rejected, Pending

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("InterviewId")]
    public virtual Interview? Interview { get; set; }

    [ForeignKey("ManagerId")]
    public virtual HiringManager? Manager { get; set; }
}
