using System;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentAPI.DTOs
{
    /// <summary>
    /// DTO for updating candidate profile information
    /// </summary>
    public class CandidateUpdateDto
    {
        /// <summary>
        /// Candidate's first name
        /// </summary>
        [MaxLength(100)]
        public string? FirstName { get; set; }

        /// <summary>
        /// Candidate's last name
        /// </summary>
        [MaxLength(100)]
        public string? LastName { get; set; }

        /// <summary>
        /// Candidate's phone number
        /// </summary>
        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Candidate's location (city, country)
        /// </summary>
        [MaxLength(200)]
        public string? Location { get; set; }

        /// <summary>
        /// Candidate's LinkedIn profile URL
        /// </summary>
        [Url]
        [MaxLength(255)]
        public string? LinkedIn { get; set; }

        /// <summary>
        /// Summary of candidate's skills
        /// </summary>
        [MaxLength(1000)]
        public string? SkillsSummary { get; set; }

        /// <summary>
        /// Candidate's current job title
        /// </summary>
        [MaxLength(100)]
        public string? CurrentJobTitle { get; set; }

        /// <summary>
        /// Candidate's current company
        /// </summary>
        [MaxLength(200)]
        public string? CurrentCompany { get; set; }

        /// <summary>
        /// Years of experience
        /// </summary>
        [Range(0, 50)]
        public int? YearsOfExperience { get; set; }

        /// <summary>
        /// Highest education level
        /// </summary>
        [MaxLength(100)]
        public string? EducationLevel { get; set; }

        /// <summary>
        /// Candidate's biography or summary
        /// </summary>
        [MaxLength(2000)]
        public string? Biography { get; set; }

        /// <summary>
        /// Candidate's date of birth
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Candidate's nationality
        /// </summary>
        [MaxLength(50)]
        public string? Nationality { get; set; }

        /// <summary>
        /// Candidate's gender
        /// </summary>
        [MaxLength(20)]
        public string? Gender { get; set; }

        /// <summary>
        /// Candidate's preferred work location
        /// </summary>
        [MaxLength(200)]
        public string? PreferredLocation { get; set; }

        /// <summary>
        /// Whether the candidate is willing to relocate
        /// </summary>
        public bool? WillingToRelocate { get; set; }

        /// <summary>
        /// Whether the candidate is willing to work remotely
        /// </summary>
        public bool? WillingToWorkRemote { get; set; }

        /// <summary>
        /// Expected salary range
        /// </summary>
        [MaxLength(50)]
        public string? ExpectedSalary { get; set; }

        /// <summary>
        /// Available start date
        /// </summary>
        public DateTime? AvailableFrom { get; set; }
    }

    /// <summary>
    /// Validation response for candidate update
    /// </summary>
    public class CandidateUpdateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CandidateProfileDto UpdatedProfile { get; set; }
        public List<string> ValidationErrors { get; set; }
    }
}