using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// -----------------------------------------------------------------------------------
// NOTE FOR THE TEAM:
// These are placeholder interfaces + DTO shapes mirroring the master prompt's spec for
// Sahan (Auth), Savindi (Candidate/Job/Application), and Sobani (Interview) modules.
// Kaveesha's unit tests below (Tests/Unit/*) are written against these contracts so the
// test suite compiles today. Once each member merges their real interfaces/DTOs, DELETE
// this file and point the `using` statements in the test files at the real namespaces
// (RecruitmentAPI.Services.Auth, RecruitmentAPI.Services.Candidate, etc.). The method
// signatures here intentionally match the master prompt so no test logic should need to change.
// -----------------------------------------------------------------------------------

namespace RecruitmentAPI.Contracts.Auth
{
    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Role);
    public record AuthResponse(string Token, DateTime ExpiresAt, int UserId, string Email, string Role);

    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> GetCurrentUserAsync(int userId);
    }
}

namespace RecruitmentAPI.Contracts.Candidate
{
    public record CandidateProfileDto(int UserId, string FirstName, string LastName, string Email,
        string? Phone, string? Location, string? LinkedIn, string? SkillsSummary);
    public record CandidateUpdateDto(string FirstName, string LastName, string? Phone, string? Location,
        string? LinkedIn, string? SkillsSummary);

    public interface ICandidateService
    {
        Task<CandidateProfileDto?> GetProfileAsync(int userId);
        Task<CandidateProfileDto> UpdateProfileAsync(int userId, CandidateUpdateDto dto);
        Task<string> UploadCvAsync(int userId, Stream fileStream, string fileName);
        Task<List<string>> GetParsedSkillsAsync(int userId);
    }
}

namespace RecruitmentAPI.Contracts.Job
{
    public record JobPostDto(string Title, string Description, string Requirements, string Department,
        string Location, string SalaryRange, string Status);
    public record JobResponseDto(int JobId, string Title, string Description, string Requirements,
        string Department, string Location, string SalaryRange, string Status, int PostedBy,
        DateTime CreatedAt, int ApplicantsCount);

    public interface IJobService
    {
        Task<List<JobResponseDto>> GetAllAsync();
        Task<JobResponseDto?> GetByIdAsync(int jobId);
        Task<JobResponseDto> CreateAsync(int postedByUserId, JobPostDto dto);
        Task<JobResponseDto> UpdateAsync(int jobId, JobPostDto dto);
        Task DeleteAsync(int jobId);
        Task<List<JobResponseDto>> GetRecommendedJobsAsync(int candidateId);
    }
}

namespace RecruitmentAPI.Contracts.Application
{
    public record ApplicationSubmitDto(int JobId, int CandidateId, string? Notes);
    public record ApplicationResponseDto(int ApplicationId, string JobTitle, string Company, string Status,
        DateTime AppliedAt, double? AiScore, string CandidateName);

    public interface IApplicationService
    {
        Task<ApplicationResponseDto> SubmitApplicationAsync(ApplicationSubmitDto dto);
        Task<List<ApplicationResponseDto>> GetByCandidateAsync(int candidateId);
        Task<List<ApplicationResponseDto>> GetByJobAsync(int jobId);
        Task<ApplicationResponseDto> UpdateStatusAsync(int applicationId, string newStatus);
        Task WithdrawAsync(int applicationId, int candidateId);
    }
}

namespace RecruitmentAPI.Contracts.Interview
{
    public record ScheduleInterviewDto(int ApplicationId, DateTime ScheduledAt, int DurationMinutes,
        string Type, string TimeZone);
    public record InterviewResponseDto(int InterviewId, string CandidateName, string JobTitle,
        DateTime ScheduledAt, int DurationMinutes, string Type, string Status, string? MeetingLink);

    public interface IInterviewService
    {
        Task<InterviewResponseDto> ScheduleAsync(ScheduleInterviewDto dto);
        Task<List<InterviewResponseDto>> GetByUserAsync(int userId);
        Task<InterviewResponseDto> UpdateStatusAsync(int interviewId, string newStatus);
        Task CancelAsync(int interviewId);
        Task<bool> GetAvailabilityAsync(int userId, DateTime proposedTime, int durationMinutes);
    }
}

namespace RecruitmentAPI.Contracts.Feedback
{
    public record FeedbackSubmitDto(int InterviewId, int TechnicalScore, int BehavioralScore,
        int CommunicationScore, string Comments, string Decision);
    public record FeedbackResponseDto(int FeedbackId, int InterviewId, string CandidateName,
        int TechnicalScore, int BehavioralScore, int CommunicationScore, double AverageScore,
        string Decision, string Comments, DateTime CreatedAt);

    public interface IFeedbackService
    {
        Task<FeedbackResponseDto> SubmitFeedbackAsync(FeedbackSubmitDto dto);
        Task<List<FeedbackResponseDto>> GetByInterviewAsync(int interviewId);
        Task<List<FeedbackResponseDto>> GetByManagerAsync(int managerId);
        Task<FeedbackResponseDto> UpdateFeedbackAsync(int feedbackId, FeedbackSubmitDto dto);
    }
}
