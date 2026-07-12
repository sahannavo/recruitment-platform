using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces;

/// <summary>
/// Unit of Work contract providing access to all repositories and transaction management.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ── Generic repositories (simple CRUD) ────────────────────────────────────
    IGenericRepository<User> Users { get; }
    IGenericRepository<Admin> Admins { get; }
    IGenericRepository<RecruitmentAnalytic> RecruitmentAnalytics { get; }
    IGenericRepository<Notification> Notifications { get; }

    // ── Specialised repositories ──────────────────────────────────────────────

    /// <summary>
    /// User-management and audit-log queries for admin operations.
    /// </summary>
    IAdminRepository AdminRepository { get; }

    /// <summary>
    /// Analytics queries that power KPI calculations and reports.
    /// </summary>
    IAnalyticsRepository AnalyticsRepository { get; }

    // ── Transaction management ────────────────────────────────────────────────
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

