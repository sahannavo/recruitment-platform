using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Data;

/// <summary>
/// Entity Framework Core database context for the recruitment platform.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // ─────────────────────────────────────────────────────────────────────────
    // DbSets
    // ─────────────────────────────────────────────────────────────────────────

    public DbSet<User> Users => Set<User>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<RecruitmentAnalytic> RecruitmentAnalytics => Set<RecruitmentAnalytic>();
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>Immutable audit trail of all admin actions.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ─────────────────────────────────────────────────────────────────────────
    // Fluent API configuration
    // ─────────────────────────────────────────────────────────────────────────

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
        });

        // ── Admin ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(a => a.AdminId);
            entity.Property(a => a.Department).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Permissions).HasMaxLength(500);

            // One-to-one: one User has at most one Admin profile
            entity.HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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

            entity.HasIndex(a => a.PerformedByUserId);
            entity.HasIndex(a => new { a.EntityType, a.EntityId });

            // Restrict so audit logs survive even if the admin account is later removed
            entity.HasOne(a => a.PerformedBy)
                .WithMany()
                .HasForeignKey(a => a.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

