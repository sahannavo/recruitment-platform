using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Exceptions;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations;

/// <summary>
/// EF Core implementation of <see cref="IAdminRepository"/>.
/// Provides user-management and audit-log queries that go beyond generic CRUD.
/// </summary>
public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDbContext _context;

    public AdminRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllUsersAsync() =>
        await _context.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role) =>
        await _context.Users
            .Where(u => u.Role == role)
            .OrderBy(u => u.LastName)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetActiveUsersAsync() =>
        await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc />
    public async Task DisableUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} was not found.");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task EnableUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} was not found.");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateUserRoleAsync(int userId, string newRole)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} was not found.");

        user.Role = newRole;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByEmailAsync(string email) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        int? performedByUserId = null,
        string? entityType = null)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (performedByUserId.HasValue)
            query = query.Where(a => a.PerformedByUserId == performedByUserId.Value);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        return await query
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAuditLogAsync(AuditLog entry)
    {
        await _context.AuditLogs.AddAsync(entry);
        await _context.SaveChangesAsync();
    }
}
