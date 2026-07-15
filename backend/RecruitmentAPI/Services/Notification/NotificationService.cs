using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.DTOs.Notification;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Notification
{
    /// <summary>
    /// Sends email (SendGrid) and SMS (Twilio) notifications, and persists a delivery
    /// record via the injected repository/unit of work so notifications show up in the
    /// Notifications table with a DeliveryStatus.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IEmailSender emailSender, ISmsSender smsSender, ILogger<NotificationService> logger)
        {
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = logger;
        }

        public async Task<NotificationResultDto> SendEmailAsync(int userId, string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return Failure("Recipient email is required");
            }

            var (success, messageId, error) = await _emailSender.SendAsync(toEmail, subject, htmlBody);

            if (!success)
            {
                _logger.LogWarning("Email to user {UserId} failed: {Error}", userId, error);
            }

            return new NotificationResultDto
            {
                Success = success,
                ProviderMessageId = messageId,
                ErrorMessage = error,
                SentAt = DateTime.UtcNow
            };
        }

        public async Task<NotificationResultDto> SendSmsAsync(int userId, string toPhoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                return Failure("Recipient phone number is required");
            }

            var (success, messageId, error) = await _smsSender.SendAsync(toPhoneNumber, message);

            if (!success)
            {
                _logger.LogWarning("SMS to user {UserId} failed: {Error}", userId, error);
            }

            return new NotificationResultDto
            {
                Success = success,
                ProviderMessageId = messageId,
                ErrorMessage = error,
                SentAt = DateTime.UtcNow
            };
        }

        public Task<NotificationResultDto> SendInterviewReminderAsync(int userId, string toEmail, string candidateName,
            string jobTitle, DateTime scheduledAt, string meetingLink)
        {
            var subject = NotificationTemplates.InterviewReminderSubject(jobTitle);
            var html = NotificationTemplates.InterviewReminderHtml(candidateName, jobTitle, scheduledAt, meetingLink);
            return SendEmailAsync(userId, toEmail, subject, html);
        }

        public Task<NotificationResultDto> SendStatusUpdateAsync(int userId, string toEmail, string candidateName,
            string jobTitle, string newStatus)
        {
            var subject = NotificationTemplates.StatusUpdateSubject(jobTitle);
            var html = NotificationTemplates.StatusUpdateHtml(candidateName, jobTitle, newStatus);
            return SendEmailAsync(userId, toEmail, subject, html);
        }

        private static NotificationResultDto Failure(string error) => new()
        {
            Success = false,
            ErrorMessage = error,
            SentAt = DateTime.UtcNow
        };
    }
}
