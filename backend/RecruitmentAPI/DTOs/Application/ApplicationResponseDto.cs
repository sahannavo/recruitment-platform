using System;
using System.Collections.Generic;

namespace RecruitmentAPI.DTOs
{
    /// <summary>
    /// DTO for application response after submission
    /// </summary>
    public class ApplicationResponseDto
    {
        /// <summary>
        /// Unique identifier for the application
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        /// Title of the job applied for
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// Company name (or department)
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Current status of the application
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Date when the application was submitted
        /// </summary>
        public DateTime AppliedAt { get; set; }

        /// <summary>
        /// AI-generated match score for the candidate
        /// </summary>
        public double? AI_Score { get; set; }

        /// <summary>
        /// Name of the candidate
        /// </summary>
        public string CandidateName { get; set; }

        /// <summary>
        /// Email of the candidate
        /// </summary>
        public string CandidateEmail { get; set; }

        /// <summary>
        /// Additional information about the application
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Whether the application was successfully submitted
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// AI score breakdown (optional)
        /// </summary>
        public MatchScoreDto ScoreBreakdown { get; set; }

        /// <summary>
        /// Job ID (for reference)
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Candidate ID (for reference)
        /// </summary>
        public int CandidateId { get; set; }

        /// <summary>
        /// Date when the application was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Notes or cover letter from the candidate
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Source of the application
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Department of the job
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Location of the job
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Expected salary from the candidate
        /// </summary>
        public string ExpectedSalary { get; set; }

        /// <summary>
        /// Available from date
        /// </summary>
        public DateTime? AvailableFrom { get; set; }

        /// <summary>
        /// Status history (audit trail)
        /// </summary>
        public List<ApplicationStatusHistoryDto> StatusHistory { get; set; }
    }

    /// <summary>
    /// DTO for AI match score breakdown
    /// </summary>
    public class MatchScoreDto
    {
        /// <summary>
        /// Overall match score (0-100)
        /// </summary>
        public double OverallScore { get; set; }

        /// <summary>
        /// Skills match percentage (0-100)
        /// </summary>
        public double SkillsMatch { get; set; }

        /// <summary>
        /// Experience match percentage (0-100)
        /// </summary>
        public double ExperienceMatch { get; set; }

        /// <summary>
        /// Education match percentage (0-100)
        /// </summary>
        public double EducationMatch { get; set; }

        /// <summary>
        /// Location match percentage (0-100)
        /// </summary>
        public double LocationMatch { get; set; }

        /// <summary>
        /// AI recommendation message
        /// </summary>
        public string Recommendation { get; set; }

        /// <summary>
        /// Matching skills found
        /// </summary>
        public List<string> MatchingSkills { get; set; }

        /// <summary>
        /// Missing skills
        /// </summary>
        public List<string> MissingSkills { get; set; }

        /// <summary>
        /// Overall rating (Excellent, Good, Average, Poor)
        /// </summary>
        public string Rating { get; set; }

        /// <summary>
        /// Score interpretation
        /// </summary>
        public string Interpretation { get; set; }
    }

    /// <summary>
    /// DTO for application status history
    /// </summary>
    public class ApplicationStatusHistoryDto
    {
        public string Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for application with interview details
    /// </summary>
    public class ApplicationWithInterviewDto : ApplicationResponseDto
    {
        /// <summary>
        /// Interview details if scheduled
        /// </summary>
        public InterviewDetailsDto Interview { get; set; }

        /// <summary>
        /// Feedback details if provided
        /// </summary>
        public FeedbackDetailsDto Feedback { get; set; }
    }

    /// <summary>
    /// DTO for interview details
    /// </summary>
    public class InterviewDetailsDto
    {
        public int InterviewId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int Duration { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string MeetingLink { get; set; }
        public string InterviewerName { get; set; }
    }

    /// <summary>
    /// DTO for feedback details
    /// </summary>
    public class FeedbackDetailsDto
    {
        public int FeedbackId { get; set; }
        public double TechnicalScore { get; set; }
        public double BehavioralScore { get; set; }
        public double CommunicationScore { get; set; }
        public double AverageScore { get; set; }
        public string Comments { get; set; }
        public string Decision { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ManagerName { get; set; }
    }

    /// <summary>
    /// DTO for application statistics summary
    /// </summary>
    public class ApplicationStatisticsDto
    {
        public int TotalApplications { get; set; }
        public int Submitted { get; set; }
        public int UnderReview { get; set; }
        public int Shortlisted { get; set; }
        public int InterviewScheduled { get; set; }
        public int Interviewed { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
        public int Withdrawn { get; set; }
        public double AverageAIScore { get; set; }
        public double HighestAIScore { get; set; }
        public double LowestAIScore { get; set; }
        public DateTime LastAppliedDate { get; set; }
        public Dictionary<string, int> ApplicationsByMonth { get; set; }
        public Dictionary<string, int> ApplicationsBySource { get; set; }
    }
    public class ApplicationTimelineDto
    {
        public int ApplicationId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ShortlistedAt { get; set; }
        public DateTime? InterviewedAt { get; set; }
        public DateTime? HiredAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public List<StatusChangeDto> StatusChanges { get; set; } = new();
    }

    public class StatusChangeDto
    {
        public string FromStatus { get; set; } = string.Empty;
        public string ToStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}