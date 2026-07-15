using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Models
{
    [Table("Applications")]
    public class Application
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationId { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public ApplicationStatus Status { get; set; }

        [Required]
        public DateTime AppliedAt { get; set; }

        public double? AI_Score { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Additional fields for better tracking
        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedBy { get; set; }

        public DateTime? ShortlistedAt { get; set; }

        public DateTime? InterviewedAt { get; set; }

        public DateTime? HiredAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string RejectionReason { get; set; }

        [MaxLength(200)]
        public string Source { get; set; } // Where the application came from

        // Navigation Properties
        [ForeignKey("CandidateId")]
        public virtual Candidate Candidate { get; set; }

        [ForeignKey("JobId")]
        public virtual JobPosting Job { get; set; }

        [ForeignKey("ReviewedBy")]
        public virtual User Reviewer { get; set; }

        public virtual ICollection<Interview> Interviews { get; set; }
    }

    public enum ApplicationStatus
    {
        Submitted = 0,
        UnderReview = 1,
        Shortlisted = 2,
        InterviewScheduled = 3,
        Interviewed = 4,
        Hired = 5,
        Rejected = 6,
        Withdrawn = 7,
        OnHold = 8
    }

    public enum ApplicationDecision
    {
        None = 0,
        Shortlist = 1,
        Reject = 2,
        Hire = 3,
        Hold = 4
    }
}