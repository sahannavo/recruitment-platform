using System.Threading.Tasks;

namespace RecruitmentAPI.Services.Notification
{
    /// <summary>
    /// Thin wrapper around the SendGrid client so NotificationService is unit-testable
    /// without hitting the real SendGrid API (SendGrid's own client is not easily mockable).
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email to the specified recipient.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The email subject line.</param>
        /// <param name="htmlBody">The HTML body content.</param>
        /// <returns>A tuple indicating success, provider message ID, and any error message.</returns>
        Task<(bool Success, string? MessageId, string? Error)> SendAsync(string toEmail, string subject, string htmlBody);
    }
}
