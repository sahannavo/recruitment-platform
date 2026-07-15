using System.Threading.Tasks;
using RecruitmentAPI.DTOs.Notification;

namespace RecruitmentAPI.Services.Notification
{
    /// <summary>
    /// Sends transactional notifications (email/SMS) related to recruitment events.
    /// </summary>
    public interface INotificationService
    {
        Task<NotificationResultDto> SendEmailAsync(int userId, string toEmail, string subject, string htmlBody);

        Task<NotificationResultDto> SendSmsAsync(int userId, string toPhoneNumber, string message);

        /// <summary>Sends a reminder for an upcoming interview, using the "InterviewReminder" template.</summary>
        Task<NotificationResultDto> SendInterviewReminderAsync(int userId, string toEmail, string candidateName,
            string jobTitle, System.DateTime scheduledAt, string meetingLink);

        /// <summary>Notifies a candidate that their application status has changed.</summary>
        Task<NotificationResultDto> SendStatusUpdateAsync(int userId, string toEmail, string candidateName,
            string jobTitle, string newStatus);
    }
}
