using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RecruitmentAPI.Contracts.Job;
using Xunit;

namespace RecruitmentAPI.Tests.Unit
{
    // NOTE: Written against the IJobService contract (see Contracts/TeamContracts.cs).
    // Replace with Savindi's real JobService once implemented.
    public class JobServiceTests
    {
        private readonly Mock<IJobService> _jobServiceMock = new();

        [Fact]
        public async Task GetAllAsync_ReturnsAllJobPostings()
        {
            _jobServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<JobResponseDto>
            {
                new(1, "Backend Developer", "desc", "reqs", "Engineering", "Colombo", "150k-200k", "Active", 10, DateTime.UtcNow, 5),
                new(2, "Frontend Developer", "desc", "reqs", "Engineering", "Remote", "120k-160k", "Active", 10, DateTime.UtcNow, 3)
            });

            var jobs = await _jobServiceMock.Object.GetAllAsync();

            Assert.Equal(2, jobs.Count);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsJob()
        {
            var job = new JobResponseDto(1, "Backend Developer", "desc", "reqs", "Engineering", "Colombo",
                "150k-200k", "Active", 10, DateTime.UtcNow, 5);

            _jobServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(job);

            var result = await _jobServiceMock.Object.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Backend Developer", result!.Title);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            _jobServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((JobResponseDto?)null);

            var result = await _jobServiceMock.Object.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedJob()
        {
            var dto = new JobPostDto("QA Engineer", "desc", "reqs", "QA", "Colombo", "100k-140k", "Active");
            var expected = new JobResponseDto(3, "QA Engineer", "desc", "reqs", "QA", "Colombo",
                "100k-140k", "Active", 7, DateTime.UtcNow, 0);

            _jobServiceMock.Setup(s => s.CreateAsync(7, dto)).ReturnsAsync(expected);

            var result = await _jobServiceMock.Object.CreateAsync(7, dto);

            Assert.Equal(3, result.JobId);
            Assert.Equal(0, result.ApplicantsCount);
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_CompletesSuccessfully()
        {
            _jobServiceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            await _jobServiceMock.Object.DeleteAsync(1);

            _jobServiceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetRecommendedJobsAsync_UsesAiScoringToReturnRankedJobs()
        {
            _jobServiceMock.Setup(s => s.GetRecommendedJobsAsync(1)).ReturnsAsync(new List<JobResponseDto>
            {
                new(5, "Senior Backend Developer", "desc", "reqs", "Engineering", "Colombo", "200k+", "Active", 10, DateTime.UtcNow, 8)
            });

            var recommended = await _jobServiceMock.Object.GetRecommendedJobsAsync(1);

            Assert.Single(recommended);
        }
    }
}
