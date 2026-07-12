using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RecruitmentAPI.Contracts.Feedback;
using Xunit;

namespace RecruitmentAPI.Tests.Unit
{
    // NOTE: Written against the IFeedbackService contract (see Contracts/TeamContracts.cs).
    // Once Sobani's real FeedbackService implementation lands, replace the mock target
    // below with `new FeedbackService(...)` and wire up its real dependencies
    // (IUnitOfWork, IAIService) so this test exercises real business logic.
    public class FeedbackServiceTests
    {
        private readonly Mock<IFeedbackService> _feedbackServiceMock = new();

        [Fact]
        public async Task SubmitFeedbackAsync_WithValidScores_ReturnsFeedbackWithDecision()
        {
            var dto = new FeedbackSubmitDto(1, 8, 7, 9, "Strong candidate overall.", "Advance");

            var expected = new FeedbackResponseDto(
                FeedbackId: 10,
                InterviewId: 1,
                CandidateName: "Jane Doe",
                TechnicalScore: 8,
                BehavioralScore: 7,
                CommunicationScore: 9,
                AverageScore: 8.0,
                Decision: "Advance",
                Comments: "Strong candidate overall.",
                CreatedAt: DateTime.UtcNow);

            _feedbackServiceMock.Setup(s => s.SubmitFeedbackAsync(dto)).ReturnsAsync(expected);

            var result = await _feedbackServiceMock.Object.SubmitFeedbackAsync(dto);

            Assert.Equal("Advance", result.Decision);
            Assert.Equal(8.0, result.AverageScore);
            Assert.Equal(1, result.InterviewId);
        }

        [Fact]
        public async Task SubmitFeedbackAsync_WithLowScores_ReturnsRejectDecision()
        {
            var dto = new FeedbackSubmitDto(2, 3, 4, 2, "Needs improvement.", "Reject");

            var expected = new FeedbackResponseDto(
                FeedbackId: 11,
                InterviewId: 2,
                CandidateName: "John Smith",
                TechnicalScore: 3,
                BehavioralScore: 4,
                CommunicationScore: 2,
                AverageScore: 3.0,
                Decision: "Reject",
                Comments: "Needs improvement.",
                CreatedAt: DateTime.UtcNow);

            _feedbackServiceMock.Setup(s => s.SubmitFeedbackAsync(dto)).ReturnsAsync(expected);

            var result = await _feedbackServiceMock.Object.SubmitFeedbackAsync(dto);

            Assert.Equal("Reject", result.Decision);
            Assert.True(result.AverageScore < 5);
        }

        [Fact]
        public async Task GetByInterviewAsync_ReturnsAllFeedbackForInterview()
        {
            var feedbacks = new List<FeedbackResponseDto>
            {
                new(10, 1, "Jane Doe", 8, 7, 9, 8.0, "Advance", "Great candidate.", DateTime.UtcNow),
                new(11, 1, "Jane Doe", 7, 8, 7, 7.3, "Advance", "Good fit for team.", DateTime.UtcNow)
            };

            _feedbackServiceMock.Setup(s => s.GetByInterviewAsync(1)).ReturnsAsync(feedbacks);

            var result = await _feedbackServiceMock.Object.GetByInterviewAsync(1);

            Assert.Equal(2, result.Count);
            Assert.All(result, f => Assert.Equal(1, f.InterviewId));
        }

        [Fact]
        public async Task GetByManagerAsync_ReturnsManagerSpecificFeedback()
        {
            var feedbacks = new List<FeedbackResponseDto>
            {
                new(10, 1, "Jane Doe", 8, 7, 9, 8.0, "Advance", "Great candidate.", DateTime.UtcNow)
            };

            _feedbackServiceMock.Setup(s => s.GetByManagerAsync(5)).ReturnsAsync(feedbacks);

            var result = await _feedbackServiceMock.Object.GetByManagerAsync(5);

            Assert.Single(result);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_WithValidData_ReturnsUpdatedFeedback()
        {
            var dto = new FeedbackSubmitDto(1, 9, 8, 9, "Revised — even stronger than initially assessed.", "Advance");

            var expected = new FeedbackResponseDto(
                FeedbackId: 10,
                InterviewId: 1,
                CandidateName: "Jane Doe",
                TechnicalScore: 9,
                BehavioralScore: 8,
                CommunicationScore: 9,
                AverageScore: 8.67,
                Decision: "Advance",
                Comments: "Revised — even stronger than initially assessed.",
                CreatedAt: DateTime.UtcNow);

            _feedbackServiceMock.Setup(s => s.UpdateFeedbackAsync(10, dto)).ReturnsAsync(expected);

            var result = await _feedbackServiceMock.Object.UpdateFeedbackAsync(10, dto);

            Assert.Equal(9, result.TechnicalScore);
            Assert.True(result.AverageScore > 8.0);
        }

        [Fact]
        public async Task SubmitFeedbackAsync_ByNonHiringManager_ThrowsUnauthorized()
        {
            var dto = new FeedbackSubmitDto(1, 8, 7, 9, "Attempting without permission.", "Advance");

            _feedbackServiceMock.Setup(s => s.SubmitFeedbackAsync(dto))
                .ThrowsAsync(new UnauthorizedAccessException("Only HiringManagers can submit feedback"));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _feedbackServiceMock.Object.SubmitFeedbackAsync(dto));
        }

        [Theory]
        [InlineData(8, 8, 8, 8.0)]
        [InlineData(10, 10, 10, 10.0)]
        [InlineData(3, 4, 5, 4.0)]
        [InlineData(0, 0, 0, 0.0)]
        public async Task SubmitFeedbackAsync_CalculatesCorrectAverageScore(
            int technical, int behavioral, int communication, double expectedAverage)
        {
            var dto = new FeedbackSubmitDto(1, technical, behavioral, communication, "Score test.", "Hold");

            var expected = new FeedbackResponseDto(
                FeedbackId: 20,
                InterviewId: 1,
                CandidateName: "Test Candidate",
                TechnicalScore: technical,
                BehavioralScore: behavioral,
                CommunicationScore: communication,
                AverageScore: expectedAverage,
                Decision: "Hold",
                Comments: "Score test.",
                CreatedAt: DateTime.UtcNow);

            _feedbackServiceMock.Setup(s => s.SubmitFeedbackAsync(dto)).ReturnsAsync(expected);

            var result = await _feedbackServiceMock.Object.SubmitFeedbackAsync(dto);

            Assert.Equal(expectedAverage, result.AverageScore);
        }
    }
}
