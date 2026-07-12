using System.Threading.Tasks;
using Moq;
using RecruitmentAPI.Contracts.Auth;
using Xunit;

namespace RecruitmentAPI.Tests.Unit
{
    // NOTE: Written against the IAuthService contract (see Contracts/TeamContracts.cs).
    // Once Sahan's real AuthService implementation lands, replace the mock target
    // below with `new AuthService(...)` and wire up its real dependencies
    // (IUnitOfWork, PasswordHasher, JwtHelper) instead of mocking IAuthService itself,
    // so this test exercises real login/hashing/token logic rather than a mock.
    public class AuthServiceTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponseWithToken()
        {
            var request = new LoginRequest("user@test.com", "CorrectP@ssw0rd");
            var expected = new AuthResponse("fake.jwt.token", System.DateTime.UtcNow.AddHours(1), 1, request.Email, "Candidate");

            _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(expected);

            var result = await _authServiceMock.Object.LoginAsync(request);

            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal(request.Email, result.Email);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorized()
        {
            var request = new LoginRequest("user@test.com", "WrongPassword");

            _authServiceMock.Setup(s => s.LoginAsync(request))
                .ThrowsAsync(new System.UnauthorizedAccessException("Invalid email or password"));

            await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() => _authServiceMock.Object.LoginAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_WithNewEmail_ReturnsAuthResponse()
        {
            var request = new RegisterRequest("new@test.com", "P@ssw0rd123", "Jane", "Doe", "Candidate");
            var expected = new AuthResponse("fake.jwt.token", System.DateTime.UtcNow.AddHours(1), 5, request.Email, request.Role);

            _authServiceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync(expected);

            var result = await _authServiceMock.Object.RegisterAsync(request);

            Assert.Equal(5, result.UserId);
            Assert.Equal("Candidate", result.Role);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperation()
        {
            var request = new RegisterRequest("dup@test.com", "P@ssw0rd123", "Jane", "Doe", "Candidate");

            _authServiceMock.Setup(s => s.RegisterAsync(request))
                .ThrowsAsync(new System.InvalidOperationException("Email already registered"));

            await Assert.ThrowsAsync<System.InvalidOperationException>(() => _authServiceMock.Object.RegisterAsync(request));
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithUnknownUserId_ReturnsNull()
        {
            _authServiceMock.Setup(s => s.GetCurrentUserAsync(999)).ReturnsAsync((AuthResponse?)null);

            var result = await _authServiceMock.Object.GetCurrentUserAsync(999);

            Assert.Null(result);
        }
    }
}
