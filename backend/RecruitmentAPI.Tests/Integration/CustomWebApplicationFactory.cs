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
                var descriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Also remove any ApplicationDbContext registration
                var dbContextDescriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(ApplicationDbContext));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

                // Build service provider and log
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory>>();
                logger.LogInformation("Using in-memory database for integration tests");

                // Ensure database is created
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}