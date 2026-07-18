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
        /// <summary>
        /// Unique identifier for the job posting
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JobId { get; set; }

        /// <summary>
        /// Job title/position name
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed job description
        /// </summary>
        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Job requirements and qualifications
        /// </summary>
        [Required]
        [MaxLength(4000)]
        public string Requirements { get; set; } = string.Empty;

        /// <summary>
        /// Department where the position is available
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Work location (Remote, On-site, Hybrid, or specific city)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Salary range for the position (e.g., "$80,000 - $100,000")
        /// </summary>
        [MaxLength(50)]
        public string? SalaryRange { get; set; }

        /// <summary>
        /// Current status of the job posting
        /// </summary>
        [Required]
        public JobStatus Status { get; set; } = JobStatus.Draft;

        /// <summary>
        /// ✅ FIXED: User ID of the recruiter who posted the job
        /// Renamed for consistency with other models
        /// </summary>
        [Required]
        public int RecruiterId { get; set; }

        /// <summary>
        /// ✅ ADDED: Optional Hiring Manager ID for approval workflow
        /// </summary>
        public int? HiringManagerId { get; set; }

        /// <summary>
        /// Date and time when the job was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the job was last updated
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ✅ FIXED: Renamed for consistency (ExpiryDate → ExpiresAt)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Number of positions available (default: 1)
        /// </summary>
        [Required]
        public int PositionsAvailable { get; set; } = 1;

        /// <summary>
        /// Experience level required (Entry, Mid, Senior, Lead)
        /// </summary>
        [MaxLength(50)]
        public string? ExperienceLevel { get; set; }

        /// <summary>
        /// Employment type (Full-time, Part-time, Contract, Internship)
        /// </summary>
        [MaxLength(50)]
        public string? EmploymentType { get; set; }

        /// <summary>
        /// Whether the job is remote
        /// </summary>
        public bool IsRemote { get; set; }

        /// <summary>
        /// Skills required for the job (stored as JSON array)
        /// </summary>
        [MaxLength(500)]
        public string? RequiredSkills { get; set; }

        /// <summary>
        /// ✅ ADDED: Minimum years of experience required
        /// </summary>
        public int? MinExperienceYears { get; set; }

        /// <summary>
        /// ✅ ADDED: Maximum years of experience required
        /// </summary>
        public int? MaxExperienceYears { get; set; }

        /// <summary>
        /// ✅ ADDED: Education level required
        /// </summary>
        [MaxLength(100)]
        public string? EducationLevel { get; set; }

        /// <summary>
        /// ✅ ADDED: Company benefits
        /// </summary>
        [MaxLength(1000)]
        public string? Benefits { get; set; }

        /// <summary>
        /// ✅ ADDED: Application deadline
        /// </summary>
        public DateTime? ApplicationDeadline { get; set; }

        /// <summary>
        /// ✅ ADDED: View count for analytics
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// ✅ ADDED: Application count (denormalized for performance)
        /// </summary>
        public int ApplicationCount { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // Navigation Properties
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// ✅ FIXED: Recruiter who posted the job
        /// </summary>
        [ForeignKey(nameof(RecruiterId))]
        public virtual User? Recruiter { get; set; }

        /// <summary>
        /// ✅ ADDED: Hiring Manager for this job
        /// </summary>
        [ForeignKey(nameof(HiringManagerId))]
        public virtual HiringManager? HiringManager { get; set; }

        /// <summary>
        /// ✅ FIXED: Initialize collection to avoid null reference
        /// </summary>
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Enums
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Enum for job posting status
    /// </summary>
    public enum JobStatus
    {
        [Display("Draft")]
        Draft = 0,

        [Display("Published")]
        Published = 1,

        [Display("Open")]
        Open = 2,

        [Display("Closed")]
        Closed = 3,

        [Display("On Hold")]
        OnHold = 4,

        [Display("Filled")]
        Filled = 5,

        [Display("Archived")]
        Archived = 6
    }

    /// <summary>
    /// Enum for employment type
    /// </summary>
    public enum EmploymentType
    {
        [Display("Full-Time")]
        FullTime = 0,

        [Display("Part-Time")]
        PartTime = 1,

        [Display("Contract")]
        Contract = 2,

        [Display("Internship")]
        Internship = 3,

        [Display("Freelance")]
        Freelance = 4,

        [Display("Temporary")]
        Temporary = 5
    }

    /// <summary>
    /// Enum for experience level
    /// </summary>
    public enum ExperienceLevel
    {
        [Display("Entry Level")]
        Entry = 0,

        [Display("Mid Level")]
        Mid = 1,

        [Display("Senior Level")]
        Senior = 2,

        [Display("Lead")]
        Lead = 3,

        [Display("Manager")]
        Manager = 4,

        [Display("Director")]
        Director = 5,

        [Display("Executive")]
        Executive = 6
    }

    /// <summary>
    /// Display attribute for enum values (used in Swagger/UI)
    /// </summary>
    public class DisplayAttribute : Attribute
    {
        public string Name { get; set; }

        public DisplayAttribute(string name)
        {
            Name = name;
        }
    }
}