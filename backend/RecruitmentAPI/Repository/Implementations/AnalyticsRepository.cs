using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations;

/// <summary>
/// EF Core implementation of <see cref="IAnalyticsRepository"/>.
/// Provides analytics queries that drive KPI calculations and recruitment reporting.
/// </summary>
public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecruitmentAnalytic>> GetKPIAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        // Returns the single most-recent record per metric, optionally scoped to a date window.
        var query = BuildDateRangeQuery(startDate, endDate);

        // Pull everything and group in memory — the dataset is small per typical KPI usage.
        var records = await query.AsNoTracking().ToListAsync();

        return records
            .GroupBy(r => r.MetricName)
            .Select(g => g.OrderByDescending(r => r.Date).First());
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecruitmentAnalytic>> GetTimeToHireAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = BuildDateRangeQuery(startDate, endDate)
            .Where(r => r.MetricName == "TimeToHire");

        return await query
            .OrderBy(r => r.Department)
            .ThenByDescending(r => r.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecruitmentAnalytic>> GetFillRatesAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = BuildDateRangeQuery(startDate, endDate)
            .Where(r => r.MetricName == "FillRate");

        return await query
            .OrderBy(r => r.Department)
            .ThenByDescending(r => r.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecruitmentAnalytic>> GetSourceDataAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        // Metrics whose name starts with "Source" represent sourcing channel data
        // e.g. "SourceLinkedIn", "SourceJobBoard", "SourceReferral".
        var query = BuildDateRangeQuery(startDate, endDate)
            .Where(r => EF.Functions.Like(r.MetricName, "Source%"));

        return await query
            .OrderBy(r => r.MetricName)
            .ThenByDescending(r => r.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecruitmentAnalytic>> GetFilteredAsync(
        string? department = null,
        string? metricName = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.RecruitmentAnalytics.AsQueryable();

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(r => r.Department == department);

        if (!string.IsNullOrWhiteSpace(metricName))
            query = query.Where(r => r.MetricName == metricName);

        if (startDate.HasValue)
            query = query.Where(r => r.Date >= startDate.Value.Date);

        if (endDate.HasValue)
            query = query.Where(r => r.Date <= endDate.Value.Date);

        return await query
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.Department)
            .AsNoTracking()
            .ToListAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private IQueryable<RecruitmentAnalytic> BuildDateRangeQuery(
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _context.RecruitmentAnalytics.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(r => r.Date >= startDate.Value.Date);

        if (endDate.HasValue)
            query = query.Where(r => r.Date <= endDate.Value.Date);

        return query;
    }
}

