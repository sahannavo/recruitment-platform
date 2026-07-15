using System;

namespace RecruitmentAPI.DTOs
{
    /// <summary>
    /// DTO for displaying candidate profile information
    /// </summary>
    public class CandidateProfileDto
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Candidate's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Candidate's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Candidate's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Candidate's phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Candidate's location (city, country)
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Candidate's LinkedIn profile URL
        /// </summary>
        public string LinkedIn { get; set; }

        /// <summary>
        /// Summary of candidate's skills
        /// </summary>
        public string SkillsSummary { get; set; }

        /// <summary>
        /// Candidate's full name (computed)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Whether the candidate profile is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Date when the profile was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the profile was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Number of applications submitted by the candidate
        /// </summary>
        public int TotalApplications { get; set; }

        /// <summary>
        /// Number of active applications (not rejected or withdrawn)
        /// </summary>
        public int ActiveApplications { get; set; }

        /// <summary>
        /// List of skills extracted from CV (additional detail)
        /// </summary>
        public List<string> Skills { get; set; }

        /// <summary>
        /// Candidate's profile picture URL (optional)
        /// </summary>
        public string ProfilePictureUrl { get; set; }

        /// <summary>
        /// Candidate's current job title (optional)
        /// </summary>
        public string CurrentJobTitle { get; set; }

        /// <summary>
        /// Candidate's current company (optional)
        /// </summary>
        public string CurrentCompany { get; set; }

        /// <summary>
        /// Years of experience (optional)
        /// </summary>
        public int? YearsOfExperience { get; set; }

        /// <summary>
        /// Highest education level (optional)
        /// </summary>
        public string EducationLevel { get; set; }
    }

    /// <summary>
    /// DTO for candidate registration
    /// </summary>
    public class CandidateRegisterDto
    {
        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// LinkedIn profile URL
        /// </summary>
        public string LinkedIn { get; set; }

        /// <summary>
        /// Skills summary
        /// </summary>
        public string SkillsSummary { get; set; }
    }

    /// <summary>
    /// DTO for candidate profile summary (used in listings)
    /// </summary>
    public class CandidateSummaryDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public string SkillsSummary { get; set; }
        public int TotalApplications { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}