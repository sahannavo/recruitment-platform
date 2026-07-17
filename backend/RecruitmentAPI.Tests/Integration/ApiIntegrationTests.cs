using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RecruitmentAPI.Tests.Integration
{
    // These tests hit an in-memory TestServer and exercise full request pipelines
    // (routing -> middleware -> controller -> service -> repository -> InMemory/Test DB).
    // The CustomWebApplicationFactory configures the API to use EF Core's InMemory database.
    public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

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
        public async Task Get_Jobs_WithoutAuth_ReturnsUnauthorizedOrOk()
        {
            // Adjust once Savindi confirms whether GET /api/jobs is public or requires auth.
            var response = await _client.GetAsync("/api/jobs");

            Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Post_AuthRegister_ThenLogin_ReturnsToken()
        {
            var registerPayload = new
            {
                email = $"integration_{System.Guid.NewGuid():N}@test.com",
                password = "IntegrationTest@123",
                firstName = "Integration",
                lastName = "Tester",
                role = "Candidate"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
            Assert.True(registerResponse.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK);

            var loginPayload = new { email = registerPayload.email, password = registerPayload.password };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var body = await loginResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.True(body.TryGetProperty("token", out var token));
            Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
        }

        [Fact]
        public async Task Full_ApplicationWorkflow_SubmitToStatusUpdate_ReturnsExpectedStatuses()
        {
            // End-to-end happy path once all controllers exist:
            // 1. Register + login as candidate -> get JWT
            // 2. Register + login as recruiter -> get JWT, create job posting
            // 3. Candidate applies to job -> AI score attached
            // 4. Recruiter moves application through workflow -> Reviewed -> Shortlisted
            // 5. Recruiter schedules interview -> notification triggered
            // 6. Hiring manager submits feedback -> application status auto-updates
            //
            // This is left as a scaffold: fill in each HTTP call as the other members'
            // controllers land, using bearer tokens from step 1/2 via
            // _client.DefaultRequestHeaders.Authorization.
            Assert.True(true, "Scaffold - implement once Auth/Job/Application/Interview/Feedback controllers are merged");
        }
    }
}
