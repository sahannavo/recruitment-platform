using RecruitmentAPI.Models;

namespace RecruitmentAPI.Helpers
{
    public interface IAIScoreCalculator
    {
        int CalculateMatchScore(string candidateSkills, string jobRequirements);
        IEnumerable<Candidate> RankCandidates(IEnumerable<Candidate> candidates, string jobRequirements);
    }

    /// <summary>
    /// Placeholder helper for AI-based candidate matching and scoring.
    /// This will be expanded by the AI service integration later.
    /// </summary>
    public class AIScoreCalculator : IAIScoreCalculator
    {
        /// <summary>
        /// Calculates a dummy match score between 0 and 100.
        /// </summary>
        public int CalculateMatchScore(string candidateSkills, string jobRequirements)
        {
            // TODO: Implement actual AI logic (e.g., calling OpenAI or analyzing embeddings)
            // For now, return a random realistic match score
            var random = new Random();
            return random.Next(40, 99);
        }

        /// <summary>
        /// Ranks a list of candidates based on dummy match scores.
        /// </summary>
        public IEnumerable<Candidate> RankCandidates(IEnumerable<Candidate> candidates, string jobRequirements)
        {
            // TODO: Implement actual AI batch processing/ranking
            // For now, assign random scores and sort descending
            var random = new Random();

            return candidates
                .Select(c => new { Candidate = c, Score = random.Next(40, 99) })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Candidate)
                .ToList();
        }
    }
}