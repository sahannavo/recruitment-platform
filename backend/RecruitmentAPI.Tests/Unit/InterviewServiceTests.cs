using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RecruitmentAPI.Contracts.Interview;
using Xunit;

namespace RecruitmentAPI.Tests.Unit
{
    // NOTE: Written against the IInterviewService contract (see Contracts/TeamContracts.cs).
    // Replace with Sobani's real InterviewService once implemented. When wired to the real
    // service, also verify it calls INotificationService.SendInterviewReminderAsync on schedule.
    public class InterviewServiceTests
    {
        private readonly Mock<IInterviewService> _interviewServiceMock = new();

        [Fact]
        public async Task ScheduleAsync_WithAvailableSlot_ReturnsScheduledInterview()
        {
            var dto = new ScheduleInterviewDto(100, DateTime.UtcNow.AddDays(2), 60, "Technical", "Asia/Colombo");
            var expected = new InterviewResponseDto(1, "Jane Doe", "Backend Developer", dto.ScheduledAt,
                60, "Technical", "Scheduled", "https://meet.test/abc");

            _interviewServiceMock.Setup(s => s.ScheduleAsync(dto)).ReturnsAsync(expected);

            var result = await _interviewServiceMock.Object.ScheduleAsync(dto);

            Assert.Equal("Scheduled", result.Status);
            Assert.NotNull(result.MeetingLink);
        }

        [Fact]
        public async Task ScheduleAsync_WithConflictingSlot_ThrowsInvalidOperation()
        {
            var dto = new ScheduleInterviewDto(100, DateTime.UtcNow.AddDays(2), 60, "Technical", "Asia/Colombo");

            _interviewServiceMock
                .Setup(s => s.ScheduleAsync(dto))
                .ThrowsAsync(new InvalidOperationException("Interviewer is unavailable at the requested time"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _interviewServiceMock.Object.ScheduleAsync(dto));
        }

        [Fact]
        public async Task GetByUserAsync_ReturnsUpcomingInterviews()
        {
            _interviewServiceMock.Setup(s => s.GetByUserAsync(5)).ReturnsAsync(new List<InterviewResponseDto>
            {
                new(1, "Jane Doe", "Backend Developer", DateTime.UtcNow.AddDays(1), 60, "Technical", "Scheduled", "https://meet.test/abc")
            });

            var interviews = await _interviewServiceMock.Object.GetByUserAsync(5);

            Assert.Single(interviews);
        }

        [Fact]
        public async Task CancelAsync_WithValidId_CompletesSuccessfully()
        {
            _interviewServiceMock.Setup(s => s.CancelAsync(1)).Returns(Task.CompletedTask);

            await _interviewServiceMock.Object.CancelAsync(1);

            _interviewServiceMock.Verify(s => s.CancelAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetAvailabilityAsync_WhenSlotFree_ReturnsTrue()
        {
            var proposedTime = DateTime.UtcNow.AddDays(3);

            _interviewServiceMock.Setup(s => s.GetAvailabilityAsync(5, proposedTime, 60)).ReturnsAsync(true);

            var isAvailable = await _interviewServiceMock.Object.GetAvailabilityAsync(5, proposedTime, 60);

            Assert.True(isAvailable);
        }
    }
}
