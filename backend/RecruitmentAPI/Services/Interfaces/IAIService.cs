namespace RecruitmentAPI.Services.Interfaces
{
    public interface IAIService
    {
        Task<decimal> CalculateAIScoreAsync(int applicationId);
        Task<string> GenerateFeedbackAsync(int applicationId);
        Task<string> MatchCandidateToJobAsync(int candidateId, int jobId);
    }
}