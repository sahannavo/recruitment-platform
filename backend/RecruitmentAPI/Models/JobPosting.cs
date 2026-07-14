using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string Title { get; set; }

        /// <summary>
        /// Detailed job description
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Job requirements and qualifications
        /// </summary>
        [Required]
        public string Requirements { get; set; }

        /// <summary>
        /// Department where the position is available
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Department { get; set; }

        /// <summary>
        /// Work location (Remote, On-site, Hybrid, or specific city)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Location { get; set; }

        /// <summary>
        /// Salary range for the position (e.g., "$80,000 - $100,000")
        /// </summary>
        [MaxLength(50)]
        public string SalaryRange { get; set; }

        /// <summary>
        /// Current status of the job posting
        /// </summary>
        [Required]
        public JobStatus Status { get; set; }

        /// <summary>
        /// User ID of the recruiter who posted the job
        /// </summary>
        [Required]
        public int PostedBy { get; set; }

        /// <summary>
        /// Date and time when the job was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the job was last updated
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Optional: Expiry date for the job posting
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Number of positions available (default: 1)
        /// </summary>
        [Required]
        public int PositionsAvailable { get; set; } = 1;

        /// <summary>
        /// Experience level required (Entry, Mid, Senior, Lead)
        /// </summary>
        [MaxLength(50)]
        public string ExperienceLevel { get; set; }

        /// <summary>
        /// Employment type (Full-time, Part-time, Contract, Internship)
        /// </summary>
        [MaxLength(50)]
        public string EmploymentType { get; set; }

        /// <summary>
        /// Whether the job is remote
        /// </summary>
        public bool IsRemote { get; set; }

        /// <summary>
        /// Skills required for the job (stored as JSON array)
        /// </summary>
        [MaxLength(500)]
        public string RequiredSkills { get; set; }

        // Navigation Properties
        [ForeignKey("PostedBy")]
        public virtual User Recruiter { get; set; }

        public virtual ICollection<Application> Applications { get; set; }
    }

    /// <summary>
    /// Enum for job posting status
    /// </summary>
    public enum JobStatus
    {
        [Display(Name = "Draft")]
        Draft = 0,

        [Display(Name = "Published")]
        Published = 1,

        [Display(Name = "Open")]
        Open = 2,

        [Display(Name = "Closed")]
        Closed = 3,

        [Display(Name = "On Hold")]
        OnHold = 4,

        [Display(Name = "Filled")]
        Filled = 5,

        [Display(Name = "Archived")]
        Archived = 6
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