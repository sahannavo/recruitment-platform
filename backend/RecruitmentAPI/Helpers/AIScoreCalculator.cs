using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentAPI.DTOs.AI;
using RecruitmentAPI.Services.AI;

namespace RecruitmentAPI.Helpers
{
    /// <summary>
    /// Convenience helper for AI-powered candidate/job scoring and ranking.
    /// Delegates to <see cref="IAIService"/> so that the underlying implementation
    /// (LLM provider or keyword-based fallback) is transparent to callers.
    /// Other team members' services (Application, Job, Interview) can inject
    /// this helper instead of depending on IAIService directly when they only
    /// need scoring functionality.
    /// </summary>
    public class AIScoreCalculator
    {
        private readonly IAIService _aiService;

        /// <summary>
        /// Initialises a new instance of <see cref="AIScoreCalculator"/>.
        /// </summary>
        /// <param name="aiService">The AI service used for skill extraction and scoring.</param>
        public AIScoreCalculator(IAIService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>
        /// Calculates a match score between a single candidate and a job posting.
        /// </summary>
        /// <param name="candidateId">The candidate's user ID.</param>
        /// <param name="candidateSkillsText">Free-text description of the candidate's skills/resume.</param>
        /// <param name="jobId">The job posting ID.</param>
        /// <param name="jobRequirementsText">Free-text description of the job requirements.</param>
        /// <returns>A <see cref="MatchScoreDto"/> containing the overall score, skill match percentage,
        /// experience match percentage, and lists of matched/missing skills.</returns>
        public Task<MatchScoreDto> CalculateMatchScore(int candidateId, string candidateSkillsText,
            int jobId, string jobRequirementsText)
        {
            return _aiService.MatchCandidateToJobAsync(candidateId, candidateSkillsText, jobId, jobRequirementsText);
        }

        /// <summary>
        /// Ranks a set of candidates against a single job posting, returning them in
        /// descending order of match score.
        /// </summary>
        /// <param name="jobId">The job posting ID.</param>
        /// <param name="jobRequirementsText">Free-text description of the job requirements.</param>
        /// <param name="candidateSkillsById">A dictionary mapping candidate IDs to their free-text skills/resume.</param>
        /// <returns>A list of <see cref="CandidateRankingDto"/> ordered by score descending, with rank assigned.</returns>
        public Task<List<CandidateRankingDto>> RankCandidates(int jobId, string jobRequirementsText,
            IDictionary<int, string> candidateSkillsById)
        {
            return _aiService.RankCandidatesAsync(jobId, jobRequirementsText, candidateSkillsById);
        }
    }
}
