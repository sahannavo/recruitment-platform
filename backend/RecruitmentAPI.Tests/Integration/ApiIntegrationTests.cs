using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RecruitmentAPI.Tests.Integration
{
    /// <summary>
    /// Integration tests that exercise the full API pipeline.
    /// Uses CustomWebApplicationFactory with in-memory database.
    /// </summary>
    public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_SwaggerJson_ReturnsOk()
        {
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_Jobs_ReturnsUnauthorized_WhenNoToken()
        {
            // GET /api/jobs requires authentication
            var response = await _client.GetAsync("/api/jobs");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Post_AuthRegister_ReturnsCreated()
        {
            var registerPayload = new
            {
                email = $"test_{Guid.NewGuid():N}@example.com",
                password = "Test@123456",
                firstName = "Integration",
                lastName = "Tester",
                role = "Candidate"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);

            // Should return 201 Created or 200 OK
            Assert.True(
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.OK,
                $"Expected 201 or 200, got {(int)response.StatusCode}"
            );
        }

        [Fact]
        public async Task Post_AuthRegister_ThenLogin_ReturnsToken()
        {
            var email = $"test_{Guid.NewGuid():N}@example.com";
            var password = "Test@123456";

            // Register
            var registerPayload = new
            {
                email = email,
                password = password,
                firstName = "Integration",
                lastName = "Tester",
                role = "Candidate"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
            Assert.True(
                registerResponse.StatusCode == HttpStatusCode.Created ||
                registerResponse.StatusCode == HttpStatusCode.OK,
                $"Registration failed: {(int)registerResponse.StatusCode}"
            );

            // Login
            var loginPayload = new { email = email, password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
            Assert.True(body.TryGetProperty("token", out var token));
            Assert.False(string.IsNullOrWhiteSpace(token.GetString()));

            // Verify user info
            Assert.True(body.TryGetProperty("email", out var userEmail));
            Assert.Equal(email, userEmail.GetString());
        }

        [Fact]
        public async Task Post_AuthLogin_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var loginPayload = new { email = "nonexistent@example.com", password = "wrongpassword" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_ReturnsUnauthorized_WithoutToken()
        {
            var response = await _client.GetAsync("/api/candidates/profile");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_ReturnsOk_WithValidToken()
        {
            // First register and login to get a token
            var email = $"test_{Guid.NewGuid():N}@example.com";
            var password = "Test@123456";

            var registerPayload = new
            {
                email = email,
                password = password,
                firstName = "Integration",
                lastName = "Tester",
                role = "Candidate"
            };

            await _client.PostAsJsonAsync("/api/auth/register", registerPayload);

            var loginPayload = new { email = email, password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
            var token = loginBody.GetProperty("token").GetString();

            // Set authorization header
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Access protected endpoint
            var response = await _client.GetAsync("/api/candidates/profile");

            // Should be OK or NotFound (if profile not created)
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected 200 or 404, got {(int)response.StatusCode}"
            );
        }
    }
}