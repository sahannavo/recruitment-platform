using System.ComponentModel.DataAnnotations;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.DTOs.Job
{
    public class JobPostingDto
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? SalaryRange { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ExperienceLevel { get; set; }
        public string? EmploymentType { get; set; }
        public bool IsRemote { get; set; }
        public string? RequiredSkills { get; set; }
        public int PositionsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ApplicationCount { get; set; }
        public int RecruiterId { get; set; }
        public string RecruiterName { get; set; } = string.Empty;
    }

    public class CreateJobPostingDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Requirements { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SalaryRange { get; set; }

        public JobStatus Status { get; set; } = JobStatus.Draft;

        public string? ExperienceLevel { get; set; }
        public string? EmploymentType { get; set; }
        public bool IsRemote { get; set; }
        public string? RequiredSkills { get; set; }
        public int PositionsAvailable { get; set; } = 1;
        public DateTime? ExpiresAt { get; set; }
        public int? HiringManagerId { get; set; }
        public int? MinExperienceYears { get; set; }
        public int? MaxExperienceYears { get; set; }
        public string? EducationLevel { get; set; }
        public string? Benefits { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
    }

    public class UpdateJobPostingDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? SalaryRange { get; set; }
        public JobStatus? Status { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? EmploymentType { get; set; }
        public bool? IsRemote { get; set; }
        public string? RequiredSkills { get; set; }
        public int? PositionsAvailable { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? HiringManagerId { get; set; }
        public int? MinExperienceYears { get; set; }
        public int? MaxExperienceYears { get; set; }
        public string? EducationLevel { get; set; }
        public string? Benefits { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
    }

    public class JobSearchFilterDto
    {
        public string? Keyword { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public string? ExperienceLevel { get; set; }
        public JobStatus? Status { get; set; }
        public bool? IsRemote { get; set; }
        public int? MinSalary { get; set; }
        public int? MaxSalary { get; set; }
        public DateTime? PostedAfter { get; set; }
        public DateTime? PostedBefore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}