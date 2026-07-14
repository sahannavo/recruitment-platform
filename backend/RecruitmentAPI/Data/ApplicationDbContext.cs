using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Data
{
    /// <summary>
    /// The main database context for the Recruitment Platform.
    /// Manages the entity objects during runtime and coordinates database operations.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 👤 Sahan's Base & Role Entities
        public DbSet<User> Users { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Recruiter> Recruiters { get; set; }
        public DbSet<HiringManager> HiringManagers { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // 📝 Savindi's Job & Application Entities
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Document> Documents { get; set; }

        // 🤝 Sobani's Interview & Feedback Entities
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<InterviewFeedback> InterviewFeedbacks { get; set; }

        // 📊 Sandawaruni's Admin & Analytics Entities
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RecruitmentAnalytic> RecruitmentAnalytics { get; set; }

        /// <summary>
        /// Configures the database schema, relationships, and constraints.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. User Inheritance Setup (Table-Per-Hierarchy)
            // This maps all user roles to a single 'Users' table and adds a discriminator column.
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Candidate>("Candidate")
                .HasValue<Recruiter>("Recruiter")
                .HasValue<HiringManager>("HiringManager")
                .HasValue<Admin>("Admin");

            // 2. Global Constraints
            // Ensure email addresses are unique across the entire platform
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 3. Entity Specific Configurations (Precision, Limits, etc.)

            // JobPosting: Set precision for SalaryRange if you choose to store it as decimals later, 
            // but for now, we assume it's a string based on your DTOs.
            modelBuilder.Entity<JobPosting>()
                .Property(j => j.Title)
                .HasMaxLength(200)
                .IsRequired();

            // Application: Configure the AI Score to ensure it stores correctly
            modelBuilder.Entity<Application>()
                .Property(a => a.AI_Score)
                .HasColumnType("int");

            // Document: Ensure blob URLs can hold long paths
            modelBuilder.Entity<Document>()
                .Property(d => d.BlobUrl)
                .HasMaxLength(1000);

            // 4. Cascade Delete Behaviors
            // Prevent accidental deletion of a Job if applications exist
            modelBuilder.Entity<Application>()
                .HasOne<JobPosting>()
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Application>()
                .HasOne<Candidate>()
                .WithMany()
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a candidate removes their apps
        }
    }
}