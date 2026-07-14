using System.Threading.Tasks;

namespace RecruitmentAPI.Services.Notification
{
    /// <summary>
    /// Thin wrapper around the Twilio client so NotificationService is unit-testable
    /// without hitting the real Twilio API.
    /// </summary>
    public interface ISmsSender
    {
        /// <summary>
        /// Sends an SMS to the specified phone number.
        /// </summary>
        /// <param name="toPhoneNumber">The recipient's phone number in E.164 format.</param>
        /// <param name="message">The SMS text message content.</param>
        /// <returns>A tuple indicating success, provider message ID (Twilio SID), and any error message.</returns>
        Task<(bool Success, string? MessageId, string? Error)> SendAsync(string toPhoneNumber, string message);
    }
}
