using System;
using System.Collections.Generic;

namespace RecruitmentAPI.DTOs
{
    public class ApplicationStatistics
    {
        public int TotalApplications { get; set; }
        public int Submitted { get; set; }
        public int PendingReview { get; set; }
        public int UnderReview { get; set; }
        public int Shortlisted { get; set; }
        public int ManagerApproved { get; set; }
        public int InterviewScheduled { get; set; }
        public int Interviewed { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
        public int Withdrawn { get; set; }
        public int OnHold { get; set; }
        public DateTime LastApplicationDate { get; set; }
        public decimal AverageAIScore { get; set; }
        public decimal HighestAIScore { get; set; }
        public decimal LowestAIScore { get; set; }
        public Dictionary<string, int> ApplicationsByDepartment { get; set; } = new();
        public Dictionary<string, int> ApplicationsBySource { get; set; } = new();
        public Dictionary<string, int> ApplicationsByMonth { get; set; } = new();
    }
}