using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Data;

/// <summary>
/// Entity Framework Core database context for the recruitment platform.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Recruiter> Recruiters => Set<Recruiter>();
    public DbSet<HiringManager> HiringManagers => Set<HiringManager>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<InterviewFeedback> InterviewFeedbacks => Set<InterviewFeedback>();
    public DbSet<RecruitmentAnalytic> RecruitmentAnalytics => Set<RecruitmentAnalytic>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(u => u.UpdatedAt).IsRequired(false);
        });

        // ── Admin ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(a => a.AdminId);
            entity.Property(a => a.Department).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Permissions).HasMaxLength(500);

            entity.HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Candidate ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(c => c.CandidateId);
            entity.Property(c => c.Phone).HasMaxLength(20);
            entity.Property(c => c.Location).HasMaxLength(200);
            entity.Property(c => c.LinkedIn).HasMaxLength(200);
            entity.Property(c => c.SkillsSummary).HasMaxLength(2000);
            entity.Property(c => c.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(c => c.NoticePeriod).HasMaxLength(50);
            entity.Property(c => c.PreferredLocations).HasMaxLength(500);
            entity.Property(c => c.IsAvailable).HasDefaultValue(true);
            entity.Property(c => c.IsOpenToOpportunities).HasDefaultValue(true);
            entity.Property(c => c.AvailableFrom);
            entity.Property(c => c.WillingToRelocate);
            entity.Property(c => c.WillingToWorkRemote);
            entity.Property(c => c.UpdatedAt).IsRequired(false);

            entity.HasOne(c => c.User)
                .WithOne(u => u.Candidate)
                .HasForeignKey<Candidate>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationships
            entity.HasMany(c => c.Applications)
                .WithOne(a => a.Candidate)
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(c => c.Documents)
                .WithOne(d => d.Candidate)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Recruiter ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Recruiter>(entity =>
        {
            entity.HasKey(r => r.RecruiterId);
            entity.Property(r => r.Department).HasMaxLength(100);
            entity.Property(r => r.JobTitle).HasMaxLength(100);

            entity.HasOne(r => r.User)
                .WithOne(u => u.Recruiter)
                .HasForeignKey<Recruiter>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.JobPostings)
                .WithOne(j => j.Recruiter)
                .HasForeignKey(j => j.RecruiterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── HiringManager ──────────────────────────────────────────────────────
        modelBuilder.Entity<HiringManager>(entity =>
        {
            entity.HasKey(h => h.HiringManagerId);
            entity.Property(h => h.Department).HasMaxLength(100);
            entity.Property(h => h.ReportingTo).HasMaxLength(100);

            entity.HasOne(h => h.User)
                .WithOne(u => u.HiringManager)
                .HasForeignKey<HiringManager>(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(h => h.JobPostings)
                .WithOne(j => j.HiringManager)
                .HasForeignKey(j => j.HiringManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── JobPosting ─────────────────────────────────────────────────────────
        modelBuilder.Entity<JobPosting>(entity =>
        {
            entity.HasKey(j => j.JobId);
            entity.Property(j => j.Title).HasMaxLength(200).IsRequired();
            entity.Property(j => j.Department).HasMaxLength(100).IsRequired();
            entity.Property(j => j.Description).HasMaxLength(4000);
            entity.Property(j => j.Requirements).HasMaxLength(4000);
            entity.Property(j => j.Location).HasMaxLength(200);
            entity.Property(j => j.SalaryRange).HasMaxLength(100);
            entity.Property(j => j.Status).HasDefaultValue(JobStatus.Open);
            entity.Property(j => j.EmploymentType).HasMaxLength(50);
            entity.Property(j => j.ExperienceLevel).HasMaxLength(50);
            entity.Property(j => j.RequiredSkills).HasMaxLength(500);

            entity.Property(j => j.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(j => j.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(j => j.ExpiresAt).IsRequired(false);

            entity.HasIndex(j => j.Status);
            entity.HasIndex(j => j.Department);
            entity.HasIndex(j => j.CreatedAt);
            entity.HasIndex(j => j.RecruiterId);
            entity.HasIndex(j => j.HiringManagerId);
            entity.HasIndex(j => new { j.Status, j.CreatedAt });
            entity.HasIndex(j => new { j.Department, j.Status });

            entity.HasOne(j => j.Recruiter)
                .WithMany(r => r.JobPostings)
                .HasForeignKey(j => j.RecruiterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.HiringManager)
                .WithMany(h => h.JobPostings)
                .HasForeignKey(j => j.HiringManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(j => j.Applications)
                .WithOne(a => a.Job)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Application ────────────────────────────────────────────────────────
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(ap => ap.ApplicationId);
            entity.Property(ap => ap.Status).IsRequired().HasDefaultValue(ApplicationStatus.Submitted);
            entity.Property(ap => ap.Notes).HasMaxLength(1000);
            entity.Property(ap => ap.AppliedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(ap => ap.Source).HasMaxLength(50);
            entity.Property(ap => ap.ExpectedSalary).HasPrecision(18, 2);
            entity.Property(ap => ap.RejectionReason).HasMaxLength(500);

            entity.HasIndex(ap => ap.Status);
            entity.HasIndex(ap => new { ap.JobId, ap.CandidateId }).IsUnique();

            entity.HasOne(ap => ap.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(ap => ap.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ap => ap.Candidate)
                .WithMany(c => c.Applications)
                .HasForeignKey(ap => ap.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Interview ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Interview>(entity =>
        {
            entity.HasKey(i => i.InterviewId);
            entity.Property(i => i.Type).HasMaxLength(50).IsRequired();
            entity.Property(i => i.Status).HasMaxLength(50).IsRequired().HasDefaultValue("Scheduled");
            entity.Property(i => i.Location).HasMaxLength(200);
            entity.Property(i => i.MeetingLink).HasMaxLength(500);
            entity.Property(i => i.Notes).HasMaxLength(1000);

            entity.HasIndex(i => i.Status);
            entity.HasIndex(i => i.ScheduledAt);

            // ✅ FIXED: One-to-Many (Application has many Interviews)
            entity.HasOne(i => i.Application)
                .WithMany(a => a.Interviews)
                .HasForeignKey(i => i.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Interviewer)
                .WithMany()
                .HasForeignKey(i => i.InterviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── InterviewFeedback ──────────────────────────────────────────────────
        modelBuilder.Entity<InterviewFeedback>(entity =>
        {
            entity.HasKey(f => f.FeedbackId);
            entity.Property(f => f.Comments).HasMaxLength(2000);
            entity.Property(f => f.Decision).HasMaxLength(50).IsRequired();
            entity.Property(f => f.TechnicalScore).HasPrecision(3, 1);
            entity.Property(f => f.BehavioralScore).HasPrecision(3, 1);
            entity.Property(f => f.CommunicationScore).HasPrecision(3, 1);

            entity.HasOne(f => f.Interview)
                .WithMany(i => i.Feedbacks)
                .HasForeignKey(f => f.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Manager)
                .WithMany()
                .HasForeignKey(f => f.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── RecruitmentAnalytic ───────────────────────────────────────────────
        modelBuilder.Entity<RecruitmentAnalytic>(entity =>
        {
            entity.HasKey(a => a.AnalyticsId);
            entity.Property(a => a.Department).HasMaxLength(100).IsRequired();
            entity.Property(a => a.MetricName).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Value).HasPrecision(18, 2);

            entity.HasIndex(a => new { a.Department, a.Date, a.MetricName });
        });

        // ── Notification ──────────────────────────────────────────────────────
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.NotificationId);
            entity.Property(n => n.Type).HasMaxLength(50).IsRequired();
            entity.Property(n => n.Subject).HasMaxLength(200).IsRequired();
            entity.Property(n => n.Content).HasMaxLength(4000).IsRequired();
            entity.Property(n => n.DeliveryStatus).HasMaxLength(50).IsRequired();
            entity.Property(n => n.SentAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLog ──────────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.AuditLogId);
            entity.Property(a => a.Action).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Details).HasMaxLength(2000);
            entity.Property(a => a.IpAddress).HasMaxLength(45);
            entity.Property(a => a.UserAgent).HasMaxLength(500);
            entity.Property(a => a.PerformedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(a => a.PerformedByUserId);
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
            entity.HasIndex(a => a.PerformedAt);

            entity.HasOne(a => a.PerformedBy)
                .WithMany()
                .HasForeignKey(a => a.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Document ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.DocumentId);
            entity.Property(d => d.FileName).HasMaxLength(255).IsRequired();
            entity.Property(d => d.BlobUrl).HasMaxLength(500).IsRequired();
            entity.Property(d => d.FileType).HasMaxLength(100);
            entity.Property(d => d.DocumentType).HasMaxLength(50);
            entity.Property(d => d.FileExtension).HasMaxLength(20);
            entity.Property(d => d.DocumentName).HasMaxLength(255);

            entity.HasOne(d => d.Candidate)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}