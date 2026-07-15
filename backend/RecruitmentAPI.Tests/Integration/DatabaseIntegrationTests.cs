using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace RecruitmentAPI.Tests.Integration
{
    // NOTE: Replace `ApplicationDbContext` with the real DbContext from Sahan's module
    // (RecruitmentAPI.Data.ApplicationDbContext) once it is merged. Uses EF Core's
    // InMemory provider so these tests run without a real SQL Server instance;
    // swap to a LocalDB/Testcontainers-backed SQL Server for closer-to-production
    // coverage of things like unique constraints and cascade deletes.
    public class DatabaseIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<TestDbContext> _options;

        public DatabaseIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: $"RecruitmentTestDb_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task AddUser_ThenRetrieveByEmail_ReturnsSameUser()
        {
            await using var context = new TestDbContext(_options);
            context.Users.Add(new TestUser { Id = 1, Email = "test@test.com", FirstName = "Test", LastName = "User" });
            await context.SaveChangesAsync();

            await using var readContext = new TestDbContext(_options);
            var user = await readContext.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");

            Assert.NotNull(user);
            Assert.Equal("Test", user!.FirstName);
        }

        [Fact]
        public async Task DeleteUser_RemovesFromDatabase()
        {
            await using (var context = new TestDbContext(_options))
            {
                context.Users.Add(new TestUser { Id = 2, Email = "delete-me@test.com", FirstName = "Del", LastName = "Ete" });
                await context.SaveChangesAsync();
            }

            await using (var context = new TestDbContext(_options))
            {
                var user = await context.Users.FindAsync(2);
                Assert.NotNull(user);
                context.Users.Remove(user!);
                await context.SaveChangesAsync();
            }

            await using (var context = new TestDbContext(_options))
            {
                var user = await context.Users.FindAsync(2);
                Assert.Null(user);
            }
        }

        public void Dispose() { /* InMemory provider requires no teardown */ }
    }

    // Minimal stand-ins for the real Models/Data layer so this file compiles in isolation.
    // Delete these once RecruitmentAPI.Models.User and RecruitmentAPI.Data.ApplicationDbContext
    // are available, and reference the real types instead.
    public class TestUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestUser> Users => Set<TestUser>();
    }
}
