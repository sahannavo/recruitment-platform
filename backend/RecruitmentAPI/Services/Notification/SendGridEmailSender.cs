using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RecruitmentAPI.Services.Notification
{
    /// <summary>
    /// Sends transactional email via the SendGrid API.
    /// Wraps the SendGrid client behind <see cref="IEmailSender"/> so that
    /// <see cref="NotificationService"/> is unit-testable without hitting the real API.
    /// </summary>
    public class SendGridEmailSender : IEmailSender
    {
        private readonly NotificationServiceOptions _options;
        private readonly ILogger<SendGridEmailSender> _logger;

        /// <summary>
        /// Initialises a new instance of <see cref="SendGridEmailSender"/>.
        /// </summary>
        /// <param name="options">Notification configuration including SendGrid API key.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public SendGridEmailSender(IOptions<NotificationServiceOptions> options, ILogger<SendGridEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<(bool Success, string? MessageId, string? Error)> SendAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var client = new SendGridClient(_options.SendGridApiKey);
                var from = new EmailAddress(_options.FromEmail, _options.FromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlBody);

                var response = await client.SendEmailAsync(msg);
                var success = (int)response.StatusCode is >= 200 and < 300;
                var messageId = response.Headers?.TryGetValues("X-Message-Id", out var values) == true
                    ? values.FirstOrDefault()
                    : null;

                if (!success)
                {
                    var body = await response.Body.ReadAsStringAsync();
                    _logger.LogWarning("SendGrid returned {StatusCode}: {Body}", response.StatusCode, body);
                    return (false, null, $"SendGrid returned {response.StatusCode}");
                }

                return (true, messageId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SendGrid to {ToEmail}", toEmail);
                return (false, null, ex.Message);
            }
        }
    }
}
