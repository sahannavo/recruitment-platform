using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RecruitmentAPI.Models
{
    /// <summary>
    /// Represents a job posting in the recruitment system
    /// </summary>
    [Table("JobPostings")]
    public class JobPosting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JobId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Requirements { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SalaryRange { get; set; }

        [Required]
        public JobStatus Status { get; set; } = JobStatus.Draft;

        [Required]
        public int RecruiterId { get; set; }

        public int? HiringManagerId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        [Required]
        public int PositionsAvailable { get; set; } = 1;

        [MaxLength(50)]
        public string? ExperienceLevel { get; set; }

        [MaxLength(50)]
        public string? EmploymentType { get; set; }

        public bool IsRemote { get; set; }

        [MaxLength(500)]
        public string? RequiredSkills { get; set; }

        public int? MinExperienceYears { get; set; }
        public int? MaxExperienceYears { get; set; }

        [MaxLength(100)]
        public string? EducationLevel { get; set; }

        [MaxLength(1000)]
        public string? Benefits { get; set; }

        public DateTime? ApplicationDeadline { get; set; }
        public int ViewCount { get; set; }
        public int ApplicationCount { get; set; }

        // ─── Navigation Properties ──────────────────────────────────────────

        /// <summary>✅ FIXED: Recruiter is now Recruiter type, not User</summary>
        [ForeignKey(nameof(RecruiterId))]
        public virtual Recruiter? Recruiter { get; set; }

        [ForeignKey(nameof(HiringManagerId))]
        public virtual HiringManager? HiringManager { get; set; }

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }

    // ─── Enums ─────────────────────────────────────────────────────────────

    public enum JobStatus
    {
        Draft = 0,
        Published = 1,
        Open = 2,
        Closed = 3,
        OnHold = 4,
        Filled = 5,
        Archived = 6
    }

    public enum EmploymentType
    {
        FullTime = 0,
        PartTime = 1,
        Contract = 2,
        Internship = 3,
        Freelance = 4,
        Temporary = 5
    }

    public enum ExperienceLevel
    {
        Entry = 0,
        Mid = 1,
        Senior = 2,
        Lead = 3,
        Manager = 4,
        Director = 5,
        Executive = 6
    }
}