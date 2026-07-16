using System;

namespace RecruitmentAPI.DTOs.Application
{
    /// <summary>
    /// DTO for a single job application summary (used in job listings)
    /// </summary>
    public class JobApplicationSummaryDto
    {
        public int ApplicationId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public double? AI_Score { get; set; }
    }
}
