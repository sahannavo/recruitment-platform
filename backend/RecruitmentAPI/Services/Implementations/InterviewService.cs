using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.DTOs.Interview;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations;

/// <summary>
/// Implementation of Interview service
/// </summary>
public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InterviewService> _logger;
    private readonly INotificationService _notificationService;

    public InterviewService(
        IInterviewRepository interviewRepository,
        ApplicationDbContext context,
        ILogger<InterviewService> logger,
        INotificationService notificationService)
    {
        _interviewRepository = interviewRepository;
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<InterviewResponseDto> ScheduleAsync(ScheduleInterviewDto dto, int scheduledBy)
    {
        try
        {
            // Validate application exists
            var application = await _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.User)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationId == dto.ApplicationId);

            if (application == null)
            {
                throw new InvalidOperationException($"Application with ID {dto.ApplicationId} not found.");
            }

            // Check if interview time is in the future
            if (dto.ScheduledAt <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Interview must be scheduled for a future date and time.");
            }

            // Generate meeting link for online interviews
            string? meetingLink = dto.MeetingLink;
            if (string.IsNullOrEmpty(meetingLink) && dto.Type.Equals("Online", StringComparison.OrdinalIgnoreCase))
            {
                meetingLink = GenerateMeetingLink();
            }

            // Create interview entity
            var interview = new Interview
            {
                ApplicationId = dto.ApplicationId,
                ScheduledAt = dto.ScheduledAt,
                Duration = dto.Duration,
                Type = dto.Type,
                Status = "Scheduled",
                MeetingLink = meetingLink,
                Notes = dto.Notes,
                InterviewerId = dto.InterviewerId, // Use the selected Interviewer ID
                CreatedAt = DateTime.UtcNow
            };

            await _interviewRepository.AddAsync(interview);
            
            // Update application status
            application.Status = ApplicationStatus.InterviewScheduled;
            application.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Interview {InterviewId} scheduled for application {ApplicationId}", 
                interview.InterviewId, dto.ApplicationId);

            // Send notification to candidate
            if (application.Candidate?.User != null && !string.IsNullOrEmpty(application.Candidate.User.Email))
            {
                var candidateName = $"{application.Candidate.User.FirstName} {application.Candidate.User.LastName}".Trim();
                if (string.IsNullOrEmpty(candidateName)) candidateName = "Candidate";

                var jobTitle = application.Job?.Title ?? "Open Position";
                
                await _notificationService.SendInterviewReminderAsync(
                    application.Candidate.UserId,
                    application.Candidate.User.Email, 
                    candidateName, 
                    jobTitle, 
                    dto.ScheduledAt, 
                    meetingLink ?? string.Empty,
                    dto.Notes ?? string.Empty
                );
            }

            return MapToResponseDto(interview, application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling interview for application {ApplicationId}", dto.ApplicationId);
            throw;
        }
    }

    public async Task<IEnumerable<InterviewResponseDto>> GetByUserAsync(int userId, string role)
    {
        try
        {
            IEnumerable<Interview> interviews = role.ToLower() switch
            {
                "candidate" => await _interviewRepository.GetByCandidateAsync(userId),
                "recruiter" => await _interviewRepository.GetByRecruiterAsync(userId),
                "hiringmanager" => await _interviewRepository.GetByHiringManagerAsync(userId),
                _ => await _interviewRepository.GetAllAsync()
            };

            return interviews.Select(i => MapToResponseDto(i));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving interviews for user {UserId} with role {Role}", userId, role);
            throw;
        }
    }

    public async Task<InterviewResponseDto?> GetByIdAsync(int interviewId)
    {
        try
        {
            var interview = await _interviewRepository.GetByIdAsync(interviewId);
            return interview != null ? MapToResponseDto(interview) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving interview {InterviewId}", interviewId);
            throw;
        }
    }

    public async Task<InterviewResponseDto> UpdateStatusAsync(int interviewId, string status, int userId)
    {
        try
        {
            var interview = await _interviewRepository.GetByIdAsync(interviewId);
            if (interview == null)
            {
                throw new InvalidOperationException($"Interview with ID {interviewId} not found.");
            }

            // Validate status
            var validStatuses = new[] { "Scheduled", "Completed", "Cancelled", "Rescheduled" };
            if (!validStatuses.Contains(status))
            {
                throw new InvalidOperationException($"Invalid status: {status}");
            }

            interview.Status = status;
            interview.UpdatedAt = DateTime.UtcNow;

            await _interviewRepository.UpdateAsync(interview);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Interview {InterviewId} status updated to {Status} by user {UserId}", 
                interviewId, status, userId);

            return MapToResponseDto(interview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for interview {InterviewId}", interviewId);
            throw;
        }
    }

    public async Task<bool> CancelAsync(int interviewId, int userId)
    {
        try
        {
            var interview = await _interviewRepository.GetByIdAsync(interviewId);
            if (interview == null)
            {
                return false;
            }

            interview.Status = "Cancelled";
            interview.UpdatedAt = DateTime.UtcNow;

            await _interviewRepository.UpdateAsync(interview);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Interview {InterviewId} cancelled by user {UserId}", interviewId, userId);

            // Send cancellation notification
            // Uncomment when NotificationService is available
            // await SendCancellationNotificationAsync(interview);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling interview {InterviewId}", interviewId);
            throw;
        }
    }

    public async Task<IEnumerable<DateTime>> GetAvailabilityAsync(DateTime date, int duration)
    {
        // Mock implementation - returns available time slots
        // In production, integrate with Google Calendar or Outlook API
        var availableSlots = new List<DateTime>();
        var startHour = 9; // 9 AM
        var endHour = 17; // 5 PM

        for (int hour = startHour; hour < endHour; hour++)
        {
            var slot = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0, DateTimeKind.Utc);
            
            // Check if slot is available (no overlapping interviews)
            var hasConflict = await CheckScheduleConflictAsync(slot, duration);
            
            if (!hasConflict)
            {
                availableSlots.Add(slot);
            }
        }

        return await Task.FromResult(availableSlots);
    }

    public async Task<IEnumerable<InterviewResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var interviews = await _interviewRepository.GetByDateRangeAsync(startDate, endDate);
            return interviews.Select(i => MapToResponseDto(i));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving interviews between {StartDate} and {EndDate}", startDate, endDate);
            throw;
        }
    }

    #region Helper Methods

    private string GenerateMeetingLink()
    {
        // Generate a unique meeting link (placeholder)
        // In production, integrate with Google Meet, Zoom, or Microsoft Teams API
        var meetingId = Guid.NewGuid().ToString("N").Substring(0, 10);
        return $"https://meet.example.com/{meetingId}";
    }

    private async Task<bool> CheckScheduleConflictAsync(DateTime scheduledAt, int duration)
    {
        var endTime = scheduledAt.AddMinutes(duration);
        var interviews = await _interviewRepository.GetByDateRangeAsync(
            scheduledAt.AddHours(-2), 
            endTime.AddHours(2));

        return interviews.Any(i => 
            i.Status == "Scheduled" &&
            i.ScheduledAt < endTime &&
            i.ScheduledAt.AddMinutes(i.Duration) > scheduledAt);
    }

    private InterviewResponseDto MapToResponseDto(Interview interview, Application? application = null)
    {
        var app = application ?? interview.Application;
        
        return new InterviewResponseDto
        {
            InterviewId = interview.InterviewId,
            ApplicationId = interview.ApplicationId,
            CandidateName = app?.Candidate != null 
                ? $"{app.Candidate.User.FirstName} {app.Candidate.User.LastName}" 
                : "N/A",
            CandidateEmail = app?.Candidate?.User?.Email ?? "N/A",
            JobTitle = app?.Job?.Title ?? "N/A",
            ScheduledAt = interview.ScheduledAt,
            Duration = interview.Duration,
            Type = interview.Type,
            Status = interview.Status,
            MeetingLink = interview.MeetingLink,
            Notes = interview.Notes,
            CreatedAt = interview.CreatedAt,
            FeedbackCount = interview.Feedbacks?.Count ?? 0
        };
    }

    // Uncomment when NotificationService is available
    /*
    private async Task SendInterviewNotificationAsync(Interview interview, Application application)
    {
        try
        {
            var notification = new NotificationDto
            {
                UserId = application.CandidateId,
                Type = "InterviewScheduled",
                Subject = "Interview Scheduled",
                Content = $"Your interview for {application.Job?.Title} has been scheduled for {interview.ScheduledAt:f}."
            };

            await _notificationService.SendEmailAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send interview notification for interview {InterviewId}", interview.InterviewId);
        }
    }

    private async Task SendCancellationNotificationAsync(Interview interview)
    {
        try
        {
            var notification = new NotificationDto
            {
                UserId = interview.Application!.CandidateId,
                Type = "InterviewCancelled",
                Subject = "Interview Cancelled",
                Content = $"Your interview scheduled for {interview.ScheduledAt:f} has been cancelled."
            };

            await _notificationService.SendEmailAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send cancellation notification for interview {InterviewId}", interview.InterviewId);
        }
    }
    */

    #endregion
}
