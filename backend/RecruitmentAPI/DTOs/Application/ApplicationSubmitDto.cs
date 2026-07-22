using System;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentAPI.DTOs
{
    /// <summary>
    /// DTO for submitting a new job application
    /// </summary>
    public class ApplicationSubmitDto
    {
        /// <summary>
        /// ID of the job being applied to
        /// </summary>
        [Required(ErrorMessage = "Job ID is required")]
        public int JobId { get; set; }

        /// <summary>
        /// ID of the candidate submitting the application
        /// </summary>
        [Required(ErrorMessage = "Candidate ID is required")]
        public int CandidateId { get; set; }

        /// <summary>
        /// Additional notes or cover letter from the candidate
        /// </summary>
        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        /// <summary>
        /// Source of the application (e.g., LinkedIn, Company Website, Referral)
        /// </summary>
        [MaxLength(100)]
        public string? Source { get; set; }

        /// <summary>
        /// Expected salary for this position
        /// </summary>
        [MaxLength(50)]
        public string? ExpectedSalary { get; set; }

        /// <summary>
        /// Availability date for the candidate
        /// </summary>
        public DateTime? AvailableFrom { get; set; }

        /// <summary>
        /// Document ID if attaching a specific CV
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// Whether the candidate agrees to the terms and conditions
        /// </summary>
        public bool AgreeToTerms { get; set; }

        /// <summary>
        /// Additional questions answered by the candidate
        /// </summary>
        public Dictionary<string, string>? CustomAnswers { get; set; }
    }

    /// <summary>
    /// DTO for updating application status
    /// </summary>
    public class ApplicationStatusUpdateDto
    {
        [Required]
        public string Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public string? RejectionReason { get; set; }

        public int? ReviewedBy { get; set; }
    }

    /// <summary>
    /// DTO for application listing
    /// </summary>
    public class ApplicationListDto
    {
        public int ApplicationId { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public double? AI_Score { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
    }

    /// <summary>
    /// DTO for application summary (for dashboard)
    /// </summary>
    public class ApplicationSummaryDto
    {
        public int TotalApplications { get; set; }
        public int Submitted { get; set; }
        public int UnderReview { get; set; }
        public int Shortlisted { get; set; }
        public int Interviewed { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
        public int Withdrawn { get; set; }
        public double AverageAIScore { get; set; }
        public DateTime LastAppliedDate { get; set; }
    }
}