using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentAPI.DTOs.AI;

namespace RecruitmentAPI.Services.AI
{
    /// <summary>
    /// Provides AI-assisted resume parsing, skill extraction, and candidate/job matching.
    /// Implementations should degrade gracefully to a keyword-based fallback if the
    /// external AI provider is unavailable.
    /// </summary>
    public interface IAIService
    {
        /// <summary>Parses raw resume text (or a file's extracted text) into structured data.</summary>
        Task<ResumeParseResult> ParseResumeAsync(string resumeText);

        /// <summary>Extracts a normalized list of skills from free text.</summary>
        Task<List<string>> ExtractSkillsAsync(string text);

        /// <summary>Calculates a match score between a candidate's profile/resume and a job description.</summary>
        Task<MatchScoreDto> MatchCandidateToJobAsync(int candidateId, string candidateSkillsText,
            int jobId, string jobRequirementsText);

        /// <summary>Ranks a set of candidates against a single job posting, descending by score.</summary>
        Task<List<CandidateRankingDto>> RankCandidatesAsync(int jobId, string jobRequirementsText,
            IDictionary<int, string> candidateSkillsById);

        /// <summary>Generates a narrative feedback summary from raw interview notes/scores.</summary>
        Task<AIFeedbackDto> GenerateFeedbackAsync(string interviewNotes, int technicalScore,
            int behavioralScore, int communicationScore);
    }
}
