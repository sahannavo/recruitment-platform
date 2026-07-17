using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.Data;

namespace RecruitmentAPI.Tests.Integration
{
    /// <summary>
    /// Custom WebApplicationFactory that configures the API to use an in-memory database for integration tests.
    /// This allows integration tests to run without requiring a real SQL Server database.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing ApplicationDbContext registration
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                // Resolve logger for debugging
                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<CustomWebApplicationFactory>>();
                logger.LogInformation("Using in-memory database for integration tests");
            });
        }
    }
}
