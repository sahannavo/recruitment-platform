using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using RecruitmentAPI.Services.AI;
using Xunit;

namespace RecruitmentAPI.Tests.Unit
{
    public class AIServiceTests
    {
        private static AIService CreateService(HttpMessageHandler? handler = null, AIServiceOptions? options = null)
        {
            var httpClient = handler != null ? new HttpClient(handler) : new HttpClient();
            httpClient.BaseAddress = new System.Uri("https://fake-ai-provider.test/");

            options ??= new AIServiceOptions { Provider = "None" }; // forces keyword fallback by default

            return new AIService(httpClient, Options.Create(options), NullLogger<AIService>.Instance);
        }

        [Fact]
        public async Task ParseResumeAsync_WithNoProviderConfigured_UsesKeywordFallback()
        {
            var service = CreateService();

            var result = await service.ParseResumeAsync("Experienced C# developer with 5 years experience in ASP.NET Core and SQL Server.");

            Assert.True(result.ParsedSuccessfully);
            Assert.Equal("KeywordFallback", result.ParseEngine);
            Assert.Contains("C#", result.Skills);
            Assert.Contains("ASP.NET Core", result.Skills);
            Assert.Equal(5, result.YearsOfExperience);
        }

        [Fact]
        public async Task ParseResumeAsync_WithEmptyText_ReturnsUnsuccessfulResult()
        {
            var service = CreateService();

            var result = await service.ParseResumeAsync("");

            Assert.False(result.ParsedSuccessfully);
        }

        [Fact]
        public async Task ExtractSkillsAsync_FindsKnownSkillsCaseInsensitively()
        {
            var service = CreateService();

            var skills = await service.ExtractSkillsAsync("Worked with REACT, docker and Kubernetes daily.");

            Assert.Contains("React", skills);
            Assert.Contains("Docker", skills);
            Assert.Contains("Kubernetes", skills);
        }

        [Fact]
        public async Task MatchCandidateToJobAsync_CalculatesScoreFromOverlappingSkills()
        {
            var service = CreateService();

            var match = await service.MatchCandidateToJobAsync(
                candidateId: 1,
                candidateSkillsText: "C#, ASP.NET Core, SQL Server, 6 years experience",
                jobId: 10,
                jobRequirementsText: "Looking for C#, ASP.NET Core, Docker skills");

            Assert.Equal(1, match.CandidateId);
            Assert.Equal(10, match.JobId);
            Assert.Contains("C#", match.MatchedSkills);
            Assert.Contains("Docker", match.MissingSkills);
            Assert.True(match.Score > 0);
            Assert.True(match.SkillsMatch <= 100);
        }

        [Fact]
        public async Task MatchCandidateToJobAsync_NoOverlap_ReturnsZeroSkillsMatch()
        {
            var service = CreateService();

            var match = await service.MatchCandidateToJobAsync(
                candidateId: 2,
                candidateSkillsText: "Excellent communication and leadership",
                jobId: 20,
                jobRequirementsText: "Requires Kubernetes and Azure experience");

            Assert.Equal(0, match.SkillsMatch);
        }

        [Fact]
        public async Task RankCandidatesAsync_OrdersDescendingByScoreAndAssignsRank()
        {
            var service = CreateService();

            var candidates = new Dictionary<int, string>
            {
                { 1, "C#, ASP.NET Core, SQL Server, Docker, 8 years experience" }, // strong match
                { 2, "communication only, 1 year experience" },                    // weak match
                { 3, "C#, SQL Server, 4 years experience" }                        // medium match
            };

            var ranked = await service.RankCandidatesAsync(100, "C#, ASP.NET Core, SQL Server, Docker", candidates);

            Assert.Equal(3, ranked.Count);
            Assert.Equal(1, ranked[0].Rank);
            Assert.Equal(1, ranked[0].CandidateId); // strongest match should rank first
            Assert.True(ranked[0].Score >= ranked[1].Score);
            Assert.True(ranked[1].Score >= ranked[2].Score);
        }

        [Theory]
        [InlineData(8, 8, 8, "Advance")]
        [InlineData(6, 6, 6, "Hold")]
        [InlineData(3, 3, 3, "Reject")]
        public async Task GenerateFeedbackAsync_ReturnsExpectedDecisionForScoreBand(
            int technical, int behavioral, int communication, string expectedDecision)
        {
            var service = CreateService();

            var feedback = await service.GenerateFeedbackAsync("Solid interview overall.", technical, behavioral, communication);

            Assert.Equal(expectedDecision, feedback.RecommendedDecision);
        }

        [Fact]
        public async Task ParseResumeAsync_WhenProviderConfiguredButHttpCallFails_FallsBackToKeywordParsing()
        {
            // Simulate a configured AI provider whose HTTP call throws/fails,
            // proving the service degrades gracefully instead of throwing.
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("simulated network failure"));

            var options = new AIServiceOptions
            {
                Provider = "OpenAI",
                ApiKey = "fake-key",
                Endpoint = "https://fake-ai-provider.test/v1/chat/completions",
                TimeoutSeconds = 5
            };

            var service = CreateService(handlerMock.Object, options);

            var result = await service.ParseResumeAsync("Java developer, Spring Boot, 3 years experience");

            Assert.True(result.ParsedSuccessfully);
            Assert.Equal("KeywordFallback", result.ParseEngine);
            Assert.Contains("Java", result.Skills);
        }
    }
}
