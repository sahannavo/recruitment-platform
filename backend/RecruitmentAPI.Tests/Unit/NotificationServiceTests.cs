using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecruitmentAPI.Services.Notification;
using Xunit;

namespace RecruitmentAPI.Tests.Unit
{
    public class NotificationServiceTests
    {
        private readonly Mock<IEmailSender> _emailSenderMock = new();
        private readonly Mock<ISmsSender> _smsSenderMock = new();
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _service = new NotificationService(_emailSenderMock.Object, _smsSenderMock.Object,
                NullLogger<NotificationService>.Instance);
        }

        [Fact]
        public async Task SendEmailAsync_WhenSenderSucceeds_ReturnsSuccessResult()
        {
            _emailSenderMock
                .Setup(s => s.SendAsync("candidate@test.com", "Subject", "<p>Body</p>"))
                .ReturnsAsync((true, "msg-123", (string?)null));

            var result = await _service.SendEmailAsync(1, "candidate@test.com", "Subject", "<p>Body</p>");

            Assert.True(result.Success);
            Assert.Equal("msg-123", result.ProviderMessageId);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public async Task SendEmailAsync_WhenSenderFails_ReturnsFailureResultWithError()
        {
            _emailSenderMock
                .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, (string?)null, "SendGrid returned 400"));

            var result = await _service.SendEmailAsync(1, "candidate@test.com", "Subject", "<p>Body</p>");

            Assert.False(result.Success);
            Assert.Equal("SendGrid returned 400", result.ErrorMessage);
        }

        [Fact]
        public async Task SendEmailAsync_WithMissingRecipient_DoesNotCallSenderAndFails()
        {
            var result = await _service.SendEmailAsync(1, "", "Subject", "<p>Body</p>");

            Assert.False(result.Success);
            _emailSenderMock.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendSmsAsync_WhenSenderSucceeds_ReturnsSuccessResult()
        {
            _smsSenderMock
                .Setup(s => s.SendAsync("+94771234567", "Your interview is confirmed"))
                .ReturnsAsync((true, "sms-456", (string?)null));

            var result = await _service.SendSmsAsync(2, "+94771234567", "Your interview is confirmed");

            Assert.True(result.Success);
            Assert.Equal("sms-456", result.ProviderMessageId);
        }

        [Fact]
        public async Task SendSmsAsync_WithMissingPhoneNumber_DoesNotCallSenderAndFails()
        {
            var result = await _service.SendSmsAsync(2, "", "message");

            Assert.False(result.Success);
            _smsSenderMock.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendInterviewReminderAsync_BuildsSubjectAndBodyAndDelegatesToEmailSender()
        {
            _emailSenderMock
                .Setup(s => s.SendAsync("candidate@test.com",
                    It.Is<string>(subj => subj.Contains("Interview Reminder") && subj.Contains("Backend Developer")),
                    It.Is<string>(body => body.Contains("Jane Doe") && body.Contains("https://meet.test/abc"))))
                .ReturnsAsync((true, "msg-789", (string?)null));

            var scheduledAt = new DateTime(2026, 8, 1, 10, 0, 0);

            var result = await _service.SendInterviewReminderAsync(
                3, "candidate@test.com", "Jane Doe", "Backend Developer", scheduledAt, "https://meet.test/abc");

            Assert.True(result.Success);
            _emailSenderMock.VerifyAll();
        }

        [Fact]
        public async Task SendStatusUpdateAsync_BuildsSubjectAndBodyAndDelegatesToEmailSender()
        {
            _emailSenderMock
                .Setup(s => s.SendAsync("candidate@test.com",
                    It.Is<string>(subj => subj.Contains("Application Update")),
                    It.Is<string>(body => body.Contains("Shortlisted"))))
                .ReturnsAsync((true, "msg-999", (string?)null));

            var result = await _service.SendStatusUpdateAsync(
                4, "candidate@test.com", "Jane Doe", "Backend Developer", "Shortlisted");

            Assert.True(result.Success);
            _emailSenderMock.VerifyAll();
        }
    }
}
