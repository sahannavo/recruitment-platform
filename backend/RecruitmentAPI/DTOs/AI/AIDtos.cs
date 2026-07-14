using System.Collections.Generic;

namespace RecruitmentAPI.DTOs.AI
{
    /// <summary>Result of parsing a candidate's resume/CV.</summary>
    public class ResumeParseResult
    {
        public string ExtractedText { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<ExperienceEntry> Experience { get; set; } = new();
        public List<EducationEntry> Education { get; set; } = new();
        public int YearsOfExperience { get; set; }
        public bool ParsedSuccessfully { get; set; }
        public string? ParseEngine { get; set; } // "OpenAI", "AzureFormRecognizer", "KeywordFallback"
    }

    public class ExperienceEntry
    {
        public string? Company { get; set; }
        public string? Title { get; set; }
        public string? Duration { get; set; }
    }

    public class EducationEntry
    {
        public string? Institution { get; set; }
        public string? Degree { get; set; }
        public string? Year { get; set; }
    }

    /// <summary>Match score between a candidate and a job posting.</summary>
    public class MatchScoreDto
    {
        public int CandidateId { get; set; }
        public int JobId { get; set; }
        public double Score { get; set; } // 0-100
        public double SkillsMatch { get; set; } // 0-100
        public double ExperienceMatch { get; set; } // 0-100
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
    }

    /// <summary>A ranked candidate result for a given job.</summary>
    public class CandidateRankingDto
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public double Score { get; set; }
        public int Rank { get; set; }
    }

    /// <summary>AI-generated narrative feedback for an interview or application.</summary>
    public class AIFeedbackDto
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new();
        public List<string> Concerns { get; set; } = new();
        public string RecommendedDecision { get; set; } = string.Empty; // Advance, Hold, Reject
    }
}
