using System;
using System.Collections.Generic;

namespace RecruitmentAPI.DTOs
{
    public class JobStatisticsDto
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int ClosedJobs { get; set; }
        public int DraftJobs { get; set; }
        public int FilledJobs { get; set; }
        public int ArchivedJobs { get; set; }
        public int TotalApplications { get; set; }
        public double AverageApplicationsPerJob { get; set; }
        public int JobsThisMonth { get; set; }
        public int JobsLastMonth { get; set; }
        public decimal MonthOverMonthChange { get; set; }
        public Dictionary<string, int> JobsByDepartment { get; set; } = new();
        public Dictionary<string, int> JobsByStatus { get; set; } = new();
        public Dictionary<string, int> JobsByEmploymentType { get; set; } = new();
        public int JobsWithNoApplications { get; set; }
        public DateTime? OldestJobDate { get; set; }
        public DateTime? NewestJobDate { get; set; }
    }

    public class JobFilterDto
    {
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public string? EmploymentType { get; set; }
        public string? ExperienceLevel { get; set; }
        public bool? IsRemote { get; set; }
        public DateTime? PostedAfter { get; set; }
        public DateTime? PostedBefore { get; set; }
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class JobUpdateDto
    {
        public string? JobTitle { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? SalaryRange { get; set; }
        public string? EmploymentType { get; set; }
        public string? ExperienceLevel { get; set; }
        public bool? IsRemote { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? PositionsAvailable { get; set; }
        public string? RequiredSkills { get; set; }
    }
}