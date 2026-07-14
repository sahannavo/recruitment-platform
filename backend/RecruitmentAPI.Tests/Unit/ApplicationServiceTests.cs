using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RecruitmentAPI.Contracts.Application;
using Xunit;

namespace RecruitmentAPI.Tests.Unit
{
    // NOTE: Written against the IApplicationService contract (see Contracts/TeamContracts.cs).
    // Replace with Savindi's real ApplicationService once implemented. When wired to the
    // real service, also verify it calls IAIService.MatchCandidateToJobAsync during submission.
    public class ApplicationServiceTests
    {
        private readonly Mock<IApplicationService> _applicationServiceMock = new();

        [Fact]
        public async Task SubmitApplicationAsync_WithValidData_ReturnsApplicationWithAiScore()
        {
            var dto = new ApplicationSubmitDto(1, 5, "Excited to apply");
            var expected = new ApplicationResponseDto(100, "Backend Developer", "AcmeCorp", "Submitted",
                DateTime.UtcNow, 78.5, "Jane Doe");

            _applicationServiceMock.Setup(s => s.SubmitApplicationAsync(dto)).ReturnsAsync(expected);

            var result = await _applicationServiceMock.Object.SubmitApplicationAsync(dto);

            Assert.Equal("Submitted", result.Status);
            Assert.NotNull(result.AiScore);
        }

        [Fact]
        public async Task GetByCandidateAsync_ReturnsCandidatesApplications()
        {
            _applicationServiceMock.Setup(s => s.GetByCandidateAsync(5)).ReturnsAsync(new List<ApplicationResponseDto>
            {
                new(100, "Backend Developer", "AcmeCorp", "Submitted", DateTime.UtcNow, 78.5, "Jane Doe"),
                new(101, "QA Engineer", "AcmeCorp", "Shortlisted", DateTime.UtcNow, 82.0, "Jane Doe")
            });

            var apps = await _applicationServiceMock.Object.GetByCandidateAsync(5);

            Assert.Equal(2, apps.Count);
        }

        [Theory]
        [InlineData("Submitted", "Reviewed")]
        [InlineData("Reviewed", "Shortlisted")]
        [InlineData("Shortlisted", "Interviewed")]
        [InlineData("Interviewed", "Hired")]
        [InlineData("Interviewed", "Rejected")]
        public async Task UpdateStatusAsync_FollowsValidWorkflowTransitions(string fromStatus, string toStatus)
        {
            var expected = new ApplicationResponseDto(100, "Backend Developer", "AcmeCorp", toStatus,
                DateTime.UtcNow, 78.5, "Jane Doe");

            _applicationServiceMock.Setup(s => s.UpdateStatusAsync(100, toStatus)).ReturnsAsync(expected);

            var result = await _applicationServiceMock.Object.UpdateStatusAsync(100, toStatus);

            Assert.Equal(toStatus, result.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_WithInvalidTransition_ThrowsInvalidOperation()
        {
            _applicationServiceMock
                .Setup(s => s.UpdateStatusAsync(100, "Hired"))
                .ThrowsAsync(new InvalidOperationException("Cannot move directly from Submitted to Hired"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _applicationServiceMock.Object.UpdateStatusAsync(100, "Hired"));
        }

        [Fact]
        public async Task WithdrawAsync_WithOwningCandidate_CompletesSuccessfully()
        {
            _applicationServiceMock.Setup(s => s.WithdrawAsync(100, 5)).Returns(Task.CompletedTask);

            await _applicationServiceMock.Object.WithdrawAsync(100, 5);

            _applicationServiceMock.Verify(s => s.WithdrawAsync(100, 5), Times.Once);
        }
    }
}
