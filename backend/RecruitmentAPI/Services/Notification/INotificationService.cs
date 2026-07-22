using System;
using System.Threading.Tasks;
using RecruitmentAPI.DTOs.Notification;

// Changed to match the standard interface namespace pattern so your services can find it
namespace RecruitmentAPI.Services.Interfaces
{
    /// <summary>
    /// Sends transactional notifications (email/SMS) related to recruitment events.
    /// </summary>
    public interface INotificationService
    {
        Task<NotificationResultDto> SendEmailAsync(int userId, string toEmail, string subject, string htmlBody);

        Task<NotificationResultDto> SendSmsAsync(int userId, string toPhoneNumber, string message);

        /// <summary>
        /// Send an interview reminder with meeting details
        /// </summary>
        Task<NotificationResultDto> SendInterviewReminderAsync(int candidateId, string toEmail, string candidateName, 
            string jobTitle, DateTime scheduledAt, string meetingLink, string notes);

        /// <summary>Notifies a candidate that their application status has changed.</summary>
        Task<NotificationResultDto> SendStatusUpdateAsync(int userId, string toEmail, string candidateName,
            string jobTitle, string newStatus);

        // ── Added Helper Signatures to support ApplicationService pipelines ──
        
        Task SendApplicationSubmittedAsync(string toEmail, string jobTitle) => 
            Task.CompletedTask;

        Task SendApplicationWithdrawnAsync(string toEmail, string jobTitle) => 
            Task.CompletedTask;

        Task SendApplicationUnderReviewAsync(string toEmail, string jobTitle) => 
            Task.CompletedTask;

        Task SendApplicationShortlistedAsync(string toEmail, string jobTitle) => 
            Task.CompletedTask;

        Task SendApplicationHiredAsync(string toEmail, string jobTitle) => 
            Task.CompletedTask;

        Task SendApplicationRejectedAsync(string toEmail, string jobTitle, string? reason) => 
            Task.CompletedTask;
    }
}