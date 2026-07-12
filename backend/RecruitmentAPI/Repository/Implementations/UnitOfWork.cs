using Microsoft.EntityFrameworkCore.Storage;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations;

/// <summary>
/// Unit of Work implementation coordinating repository access and database transactions.
/// Lazily instantiates all repositories so only the ones in use are allocated.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // ── Generic repositories ──────────────────────────────────────────────────
    private IGenericRepository<User>? _users;
    private IGenericRepository<Admin>? _admins;
    private IGenericRepository<RecruitmentAnalytic>? _recruitmentAnalytics;
    private IGenericRepository<Notification>? _notifications;

    // ── Specialised repositories ──────────────────────────────────────────────
    private IAdminRepository? _adminRepository;
    private IAnalyticsRepository? _analyticsRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── Generic repository accessors ─────────────────────────────────────────

    public IGenericRepository<User> Users =>
        _users ??= new GenericRepository<User>(_context);

    public IGenericRepository<Admin> Admins =>
        _admins ??= new GenericRepository<Admin>(_context);

    public IGenericRepository<RecruitmentAnalytic> RecruitmentAnalytics =>
        _recruitmentAnalytics ??= new GenericRepository<RecruitmentAnalytic>(_context);

    public IGenericRepository<Notification> Notifications =>
        _notifications ??= new GenericRepository<Notification>(_context);

    // ── Specialised repository accessors ─────────────────────────────────────

    public IAdminRepository AdminRepository =>
        _adminRepository ??= new AdminRepository(_context);

    public IAnalyticsRepository AnalyticsRepository =>
        _analyticsRepository ??= new AnalyticsRepository(_context);

    // ── Persistence ───────────────────────────────────────────────────────────

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    // ── Transaction management ────────────────────────────────────────────────

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to rollback.");

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

