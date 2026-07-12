using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace RecruitmentAPI.Services.Notification
{
    /// <summary>
    /// Sends SMS messages via the Twilio API.
    /// Wraps the Twilio client behind <see cref="ISmsSender"/> so that
    /// <see cref="NotificationService"/> is unit-testable without hitting the real API.
    /// Uses lazy initialisation for the Twilio client to avoid failures at startup
    /// when credentials are not yet available (e.g. during testing).
    /// </summary>
    public class TwilioSmsSender : ISmsSender
    {
        private readonly NotificationServiceOptions _options;
        private readonly ILogger<TwilioSmsSender> _logger;
        private volatile bool _initialized;
        private readonly object _initLock = new();

        /// <summary>
        /// Initialises a new instance of <see cref="TwilioSmsSender"/>.
        /// </summary>
        /// <param name="options">Notification configuration including Twilio credentials.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public TwilioSmsSender(IOptions<NotificationServiceOptions> options, ILogger<TwilioSmsSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Ensures the Twilio client is initialised exactly once in a thread-safe manner.
        /// Uses double-checked locking with a volatile flag to minimise contention.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_initLock)
            {
                if (_initialized) return;
                TwilioClient.Init(_options.TwilioAccountSid, _options.TwilioAuthToken);
                _initialized = true;
            }
        }

        /// <inheritdoc />
        public async Task<(bool Success, string? MessageId, string? Error)> SendAsync(string toPhoneNumber, string message)
        {
            try
            {
                EnsureInitialized();

                var result = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_options.TwilioFromNumber),
                    to: new PhoneNumber(toPhoneNumber));

                var success = result.ErrorCode == null;
                if (!success)
                {
                    _logger.LogWarning("Twilio SMS failed with error {ErrorCode}: {ErrorMessage}",
                        result.ErrorCode, result.ErrorMessage);
                    return (false, result.Sid, result.ErrorMessage);
                }

                return (true, result.Sid, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS via Twilio to {ToPhoneNumber}", toPhoneNumber);
                return (false, null, ex.Message);
            }
        }
    }
}
