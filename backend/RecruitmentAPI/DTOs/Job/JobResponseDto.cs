using System;
using System.Collections.Generic;

namespace RecruitmentAPI.DTOs
{
    /// <summary>
    /// DTO for job posting response
    /// </summary>
    public class JobResponseDto
    {
        /// <summary>
        /// Unique identifier for the job posting
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Job title/position name
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Detailed job description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Job requirements and qualifications
        /// </summary>
        public string Requirements { get; set; }

        /// <summary>
        /// Department where the position is available
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Work location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Salary range for the position
        /// </summary>
        public string SalaryRange { get; set; }

        /// <summary>
        /// Current status of the job posting
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// User ID of the recruiter who posted the job
        /// </summary>
        public int PostedBy { get; set; }

        /// <summary>
        /// Name of the recruiter who posted the job
        /// </summary>
        public string PostedByName { get; set; }

        /// <summary>
        /// Date and time when the job was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Number of applicants for this job
        /// </summary>
        public int ApplicantsCount { get; set; }

        /// <summary>
        /// Number of active applications (not rejected/withdrawn)
        /// </summary>
        public int ActiveApplicantsCount { get; set; }

        /// <summary>
        /// Number of shortlisted candidates
        /// </summary>
        public int ShortlistedCount { get; set; }

        /// <summary>
        /// Number of hired candidates
        /// </summary>
        public int HiredCount { get; set; }

        /// <summary>
        /// Optional expiry date for the job posting
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Number of positions available
        /// </summary>
        public int PositionsAvailable { get; set; }

        /// <summary>
        /// Experience level required
        /// </summary>
        public string ExperienceLevel { get; set; }

        /// <summary>
        /// Employment type
        /// </summary>
        public string EmploymentType { get; set; }

        /// <summary>
        /// Whether the job is remote
        /// </summary>
        public bool IsRemote { get; set; }

        /// <summary>
        /// Skills required for the job (comma-separated or array)
        /// </summary>
        public List<string> RequiredSkills { get; set; }

        /// <summary>
        /// Date when the job was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Whether the job is still accepting applications
        /// </summary>
        public bool IsAcceptingApplications { get; set; }

        /// <summary>
        /// Days since the job was posted
        /// </summary>
        public int DaysSincePosted => (DateTime.UtcNow - CreatedAt).Days;

        /// <summary>
        /// Average AI score of all applications
        /// </summary>
        public double? AverageAIScore { get; set; }

        /// <summary>
        /// Top candidate AI score for this job
        /// </summary>
        public double? TopAIScore { get; set; }
    }

    /// <summary>
    /// DTO for creating a new job posting
    /// </summary>
    public class JobPostDto
    {
        /// <summary>
        /// Job title/position name
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Detailed job description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Job requirements and qualifications
        /// </summary>
        public string Requirements { get; set; }

        /// <summary>
        /// Department where the position is available
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Work location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Salary range for the position
        /// </summary>
        public string SalaryRange { get; set; }

        /// <summary>
        /// Current status of the job posting
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Optional expiry date for the job posting
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Number of positions available (default: 1)
        /// </summary>
        public int PositionsAvailable { get; set; } = 1;

        /// <summary>
        /// Experience level required
        /// </summary>
        public string ExperienceLevel { get; set; }

        /// <summary>
        /// Employment type
        /// </summary>
        public string EmploymentType { get; set; }

        /// <summary>
        /// Whether the job is remote
        /// </summary>
        public bool IsRemote { get; set; }

        /// <summary>
        /// Skills required for the job (comma-separated list)
        /// </summary>
        public string RequiredSkills { get; set; }
    }

    /// <summary>
    /// DTO for job listing (summary view)
    /// </summary>
    public class JobSummaryDto
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string SalaryRange { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ApplicantsCount { get; set; }
        public bool IsRemote { get; set; }
        public string EmploymentType { get; set; }
        public string ExperienceLevel { get; set; }
    }

    /// <summary>
    /// DTO for paginated job list response
    /// </summary>
    public class JobListResponseDto
    {
        public List<JobSummaryDto> Jobs { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}