using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.DTOs.Notification;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IEmailSender emailSender,
            ISmsSender smsSender,
            IUnitOfWork unitOfWork,
            ILogger<NotificationService> logger)
        {
            _emailSender = emailSender;
            _smsSender = smsSender;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<NotificationResultDto> SendEmailAsync(int userId, string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                return Failure("Recipient email is required");

            var (success, messageId, error) = await _emailSender.SendAsync(toEmail, subject, htmlBody);

            await SaveNotificationAsync(userId, subject, htmlBody, success ? "Sent" : "Failed", "Email");

            if (!success)
                _logger.LogWarning("Email to user {UserId} failed: {Error}", userId, error);

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
                return Failure("Recipient phone number is required");

            var (success, messageId, error) = await _smsSender.SendAsync(toPhoneNumber, message);

            await SaveNotificationAsync(userId, "SMS Notification", message, success ? "Sent" : "Failed", "SMS");

            if (!success)
                _logger.LogWarning("SMS to user {UserId} failed: {Error}", userId, error);

            return new NotificationResultDto
            {
                Success = success,
                ProviderMessageId = messageId,
                ErrorMessage = error,
                SentAt = DateTime.UtcNow
            };
        }

        public async Task<NotificationResultDto> SendInterviewReminderAsync(int candidateId, string toEmail, string candidateName, 
            string jobTitle, DateTime scheduledAt, string meetingLink, string notes)
        {
            var subject = NotificationTemplates.InterviewReminderSubject(jobTitle);
            var htmlBody = NotificationTemplates.InterviewReminderHtml(candidateName, jobTitle, scheduledAt, meetingLink, notes);
            return await SendEmailAsync(candidateId, toEmail, subject, htmlBody);
        }

        public async Task<NotificationResultDto> SendStatusUpdateAsync(int userId, string toEmail, string candidateName,
            string jobTitle, string newStatus)
        {
            var subject = NotificationTemplates.StatusUpdateSubject(jobTitle);
            var html = NotificationTemplates.StatusUpdateHtml(candidateName, jobTitle, newStatus);
            return await SendEmailAsync(userId, toEmail, subject, html);
        }

        // ============================================
        // APPLICATION STATUS NOTIFICATIONS
        // ============================================

        public async Task SendApplicationSubmittedAsync(string toEmail, string jobTitle)
        {
            var subject = $"Application Submitted: {jobTitle}";
            var html = $@"
                <p>Thank you for applying to <strong>{jobTitle}</strong>!</p>
                <p>Your application has been received and is being reviewed by our team.</p>
                <p>We will update you on the status of your application soon.</p>
                <p>&mdash; Recruitment Team</p>";
            await SendEmailAsync(0, toEmail, subject, html);
        }

        public async Task SendApplicationWithdrawnAsync(string toEmail, string jobTitle)
        {
            var subject = $"Application Withdrawn: {jobTitle}";
            var html = $@"
                <p>You have successfully withdrawn your application for <strong>{jobTitle}</strong>.</p>
                <p>If you change your mind, you can reapply at any time.</p>
                <p>&mdash; Recruitment Team</p>";
            await SendEmailAsync(0, toEmail, subject, html);
        }

        public async Task SendApplicationUnderReviewAsync(string toEmail, string jobTitle)
        {
            var subject = $"Application Under Review: {jobTitle}";
            var html = $@"
                <p>Your application for <strong>{jobTitle}</strong> is now under review.</p>
                <p>Our hiring team is carefully evaluating your qualifications.</p>
                <p>We will contact you if we need additional information.</p>
                <p>&mdash; Recruitment Team</p>";
            await SendEmailAsync(0, toEmail, subject, html);
        }

        public async Task SendApplicationShortlistedAsync(string toEmail, string jobTitle)
        {
            var subject = $"Congratulations! You've been Shortlisted: {jobTitle}";
            var html = $@"
                <p>Congratulations!</p>
                <p>You have been shortlisted for the <strong>{jobTitle}</strong> position.</p>
                <p>Our recruitment team will contact you shortly to schedule an interview.</p>
                <p>&mdash; Recruitment Team</p>";
            await SendEmailAsync(0, toEmail, subject, html);
        }

        public async Task SendApplicationHiredAsync(string toEmail, string jobTitle)
        {
            var subject = $"Job Offer: {jobTitle}";
            var html = $@"
                <p>Congratulations!</p>
                <p>We are delighted to offer you the <strong>{jobTitle}</strong> position.</p>
                <p>Please check your dashboard for the official offer letter and next steps.</p>
                <p>We look forward to welcoming you to the team!</p>
                <p>&mdash; Recruitment Team</p>";
            await SendEmailAsync(0, toEmail, subject, html);
        }

        public async Task SendApplicationRejectedAsync(string toEmail, string jobTitle, string? reason)
        {
            var subject = $"Application Update: {jobTitle}";
            var reasonText = string.IsNullOrEmpty(reason)
                ? "After careful consideration, we have decided to move forward with other candidates."
                : reason;
            var html = $@"
                <p>Thank you for your interest in the <strong>{jobTitle}</strong> position.</p>
                <p>{reasonText}</p>
                <p>We appreciate the time and effort you put into your application.</p>
                <p>We encourage you to apply for future opportunities that match your skills.</p>
                <p>&mdash; Recruitment Team</p>";
            await SendEmailAsync(0, toEmail, subject, html);
        }

        // ============================================
        // PRIVATE HELPERS
        // ============================================

        private async Task SaveNotificationAsync(int userId, string subject, string content, string status, string type)
        {
            try
            {
                if (userId == 0) return;

                var notification = new RecruitmentAPI.Models.Notification
                {
                    UserId = userId,
                    Type = type,
                    Subject = subject.Length > 200 ? subject[..200] : subject,
                    Content = content.Length > 4000 ? content[..4000] : content,
                    SentAt = DateTime.UtcNow,
                    DeliveryStatus = status
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save notification record for user {UserId}", userId);
            }
        }

        private static NotificationResultDto Failure(string error) => new()
        {
            Success = false,
            ErrorMessage = error,
            SentAt = DateTime.UtcNow
        };
    }
}