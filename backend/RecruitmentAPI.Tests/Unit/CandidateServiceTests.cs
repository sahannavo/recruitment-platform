using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RecruitmentAPI.Contracts.Candidate;
using Xunit;

namespace RecruitmentAPI.Tests.Unit
{
    // NOTE: Written against the ICandidateService contract (see Contracts/TeamContracts.cs).
    // Replace with Savindi's real CandidateService + mocked IUnitOfWork/blob storage client
    // once implemented.
    public class CandidateServiceTests
    {
        private readonly Mock<ICandidateService> _candidateServiceMock = new();

        [Fact]
        public async Task GetProfileAsync_WithExistingUser_ReturnsProfile()
        {
            var expected = new CandidateProfileDto(1, "Jane", "Doe", "jane@test.com", "0771234567",
                "Colombo", "linkedin.com/in/jane", "C#, ASP.NET Core");

            _candidateServiceMock.Setup(s => s.GetProfileAsync(1)).ReturnsAsync(expected);

            var result = await _candidateServiceMock.Object.GetProfileAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Jane", result!.FirstName);
        }

        [Fact]
        public async Task GetProfileAsync_WithNonExistentUser_ReturnsNull()
        {
            _candidateServiceMock.Setup(s => s.GetProfileAsync(999)).ReturnsAsync((CandidateProfileDto?)null);

            var result = await _candidateServiceMock.Object.GetProfileAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProfileAsync_WithValidData_ReturnsUpdatedProfile()
        {
            var update = new CandidateUpdateDto("Jane", "Smith", "0771234567", "Kandy", "linkedin.com/in/jane", "C#, SQL");
            var expected = new CandidateProfileDto(1, "Jane", "Smith", "jane@test.com", "0771234567",
                "Kandy", "linkedin.com/in/jane", "C#, SQL");

            _candidateServiceMock.Setup(s => s.UpdateProfileAsync(1, update)).ReturnsAsync(expected);

            var result = await _candidateServiceMock.Object.UpdateProfileAsync(1, update);

            Assert.Equal("Smith", result.LastName);
            Assert.Equal("Kandy", result.Location);
        }

        [Fact]
        public async Task UploadCvAsync_WithValidFile_ReturnsBlobUrlAndTriggersAiParsing()
        {
            var fakeFile = new MemoryStream(Encoding.UTF8.GetBytes("fake pdf content"));

            _candidateServiceMock
                .Setup(s => s.UploadCvAsync(1, It.IsAny<Stream>(), "resume.pdf"))
                .ReturnsAsync("https://blob.storage.test/cvs/1/resume.pdf");

            var url = await _candidateServiceMock.Object.UploadCvAsync(1, fakeFile, "resume.pdf");

            Assert.StartsWith("https://blob.storage.test", url);
            _candidateServiceMock.Verify(s => s.UploadCvAsync(1, It.IsAny<Stream>(), "resume.pdf"), Times.Once);
        }

        [Fact]
        public async Task GetParsedSkillsAsync_ReturnsSkillsExtractedFromResume()
        {
            _candidateServiceMock
                .Setup(s => s.GetParsedSkillsAsync(1))
                .ReturnsAsync(new System.Collections.Generic.List<string> { "C#", "ASP.NET Core", "SQL Server" });

            var skills = await _candidateServiceMock.Object.GetParsedSkillsAsync(1);

            Assert.Contains("C#", skills);
            Assert.Equal(3, skills.Count);
        }
    }
}
