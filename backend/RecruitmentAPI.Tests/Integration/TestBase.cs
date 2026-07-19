using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RecruitmentAPI.Tests.Integration
{
    /// <summary>
    /// Base class for integration tests with common helper methods.
    /// </summary>
    public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly HttpClient Client;
        protected readonly JsonSerializerOptions JsonOptions;

        protected TestBase(CustomWebApplicationFactory factory)
        {
            Client = factory.CreateClient();
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Registers a new user and returns the email and password used.
        /// </summary>
        protected async Task<(string email, string password)> RegisterUserAsync(
            string? email = null,
            string? password = null,
            string firstName = "Test",
            string lastName = "User",
            string role = "Candidate")
        {
            email ??= $"test_{Guid.NewGuid():N}@example.com";
            password ??= "Test@123456";

            var payload = new
            {
                email,
                password,
                firstName,
                lastName,
                role
            };

            var response = await Client.PostAsJsonAsync("/api/auth/register", payload);
            response.EnsureSuccessStatusCode();

            return (email, password);
        }

        /// <summary>
        /// Logs in a user and returns the JWT token.
        /// </summary>
        protected async Task<string> LoginAsync(string email, string password)
        {
            var payload = new { email, password };
            var response = await Client.PostAsJsonAsync("/api/auth/login", payload);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            return body.GetProperty("token").GetString()!;
        }

        /// <summary>
        /// Registers a user and returns the JWT token.
        /// </summary>
        protected async Task<(string token, string email, string password)> RegisterAndLoginAsync(
            string? email = null,
            string? password = null,
            string firstName = "Test",
            string lastName = "User",
            string role = "Candidate")
        {
            var (regEmail, regPassword) = await RegisterUserAsync(email, password, firstName, lastName, role);
            var token = await LoginAsync(regEmail, regPassword);
            return (token, regEmail, regPassword);
        }

        /// <summary>
        /// Sets the Authorization header with a Bearer token.
        /// </summary>
        protected void SetAuthToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Removes the Authorization header.
        /// </summary>
        protected void RemoveAuthToken()
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }
}