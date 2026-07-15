using System;

namespace RecruitmentAPI.DTOs
{
    public class JobStatisticsDto
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int ClosedJobs { get; set; }
        public int DraftJobs { get; set; }
        public int TotalApplications { get; set; }
        public int AverageApplicationsPerJob { get; set; }
        public int JobsThisMonth { get; set; }
        public int JobsLastMonth { get; set; }
        public decimal MonthOverMonthChange { get; set; }
    }

    public class JobFilterDto
    {
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public DateTime? PostedAfter { get; set; }
        public DateTime? PostedBefore { get; set; }
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}