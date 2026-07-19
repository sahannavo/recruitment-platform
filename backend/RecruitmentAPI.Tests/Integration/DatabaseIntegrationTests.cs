using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using Xunit;

namespace RecruitmentAPI.Tests.Integration
{
    /// <summary>
    /// Database integration tests using the real ApplicationDbContext with in-memory provider.
    /// </summary>
    public class DatabaseIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;

        public DatabaseIntegrationTests()
        {
            var dbName = $"RecruitmentTestDb_{Guid.NewGuid()}";
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            _context = new ApplicationDbContext(_options);
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task AddUser_ThenRetrieveByEmail_ReturnsSameUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashed_password",
                Role = "Candidate",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var retrieved = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Test", retrieved!.FirstName);
            Assert.Equal("User", retrieved.LastName);
            Assert.Equal("Candidate", retrieved.Role);
        }

        [Fact]
        public async Task AddCandidate_WithUser_ReturnsCandidateWithUser()
        {
            // Arrange
            var user = new User
            {
                Email = "candidate@example.com",
                FirstName = "Candidate",
                LastName = "User",
                PasswordHash = "hashed_password",
                Role = "Candidate",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var candidate = new Candidate
            {
                User = user,
                IsAvailable = true,
                IsOpenToOpportunities = true,
                Location = "New York"
            };

            // Act
            await _context.Users.AddAsync(user);
            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();

            var retrieved = await _context.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Email == "candidate@example.com");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Candidate", retrieved!.User.FirstName);
            Assert.True(retrieved.IsAvailable);
            Assert.Equal("New York", retrieved.Location);
        }

        [Fact]
        public async Task AddJobPosting_WithRecruiter_ReturnsJobWithRecruiter()
        {
            // Arrange
            var user = new User
            {
                Email = "recruiter@example.com",
                FirstName = "Recruiter",
                LastName = "User",
                PasswordHash = "hashed_password",
                Role = "Recruiter",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var recruiter = new Recruiter
            {
                User = user,
                Department = "Engineering",
                JobTitle = "Senior Recruiter"
            };

            var job = new JobPosting
            {
                Title = "Senior Developer",
                Description = "We are looking for a senior developer...",
                Requirements = "5+ years experience...",
                Department = "Engineering",
                Location = "Remote",
                Recruiter = recruiter,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _context.Users.AddAsync(user);
            await _context.Recruiters.AddAsync(recruiter);
            await _context.JobPostings.AddAsync(job);
            await _context.SaveChangesAsync();

            var retrieved = await _context.JobPostings
                .Include(j => j.Recruiter)
                .ThenInclude(r => r!.User)
                .FirstOrDefaultAsync(j => j.Title == "Senior Developer");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Senior Developer", retrieved!.Title);
            Assert.NotNull(retrieved.Recruiter);
            Assert.Equal("Recruiter", retrieved.Recruiter.User.FirstName);
            Assert.Equal("Engineering", retrieved.Department);
        }

        [Fact]
        public async Task AddApplication_WithCandidateAndJob_ReturnsApplication()
        {
            // Arrange
            var candidateUser = new User
            {
                Email = "app_candidate@example.com",
                FirstName = "App",
                LastName = "Candidate",
                PasswordHash = "hashed",
                Role = "Candidate",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var candidate = new Candidate { User = candidateUser };

            var job = new JobPosting
            {
                Title = "Software Engineer",
                Description = "Description",
                Requirements = "Requirements",
                Department = "Engineering",
                Location = "Remote",
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            var application = new Application
            {
                Candidate = candidate,
                Job = job,
                Status = ApplicationStatus.Submitted,
                AppliedAt = DateTime.UtcNow,
                AI_Score = 85.5
            };

            // Act
            await _context.Users.AddAsync(candidateUser);
            await _context.Candidates.AddAsync(candidate);
            await _context.JobPostings.AddAsync(job);
            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();

            var retrieved = await _context.Applications
                .Include(a => a.Candidate)
                .ThenInclude(c => c!.User)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Job.Title == "Software Engineer");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(ApplicationStatus.Submitted, retrieved!.Status);
            Assert.Equal(85.5, retrieved.AI_Score);
            Assert.Equal("App", retrieved.Candidate!.User.FirstName);
        }

        [Fact]
        public async Task DeleteUser_CascadeDeletesCandidate()
        {
            // Arrange
            var user = new User
            {
                Email = "delete_candidate@example.com",
                FirstName = "Delete",
                LastName = "Me",
                PasswordHash = "hashed",
                Role = "Candidate",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var candidate = new Candidate { User = user };

            await _context.Users.AddAsync(user);
            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();

            // Act
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var deletedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "delete_candidate@example.com");
            var deletedCandidate = await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == user.UserId);

            // Assert
            Assert.Null(deletedUser);
            Assert.Null(deletedCandidate);
        }

        [Fact]
        public async Task UserEmail_IsUnique_Constraint()
        {
            // Arrange
            var user1 = new User
            {
                Email = "duplicate@example.com",
                FirstName = "First",
                LastName = "User",
                PasswordHash = "hashed",
                Role = "Candidate",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Email = "duplicate@example.com", // Same email
                FirstName = "Second",
                LastName = "User",
                PasswordHash = "hashed",
                Role = "Candidate",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user1);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(async () =>
            {
                await _context.Users.AddAsync(user2);
                await _context.SaveChangesAsync();
            });
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}