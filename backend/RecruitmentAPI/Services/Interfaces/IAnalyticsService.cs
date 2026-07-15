using RecruitmentAPI.DTOs;

namespace RecruitmentAPI.Services.Interfaces;

/// <summary>
/// Service contract for recruitment analytics operations,
/// KPI reporting, and system health monitoring.
/// </summary>
public interface IAnalyticsService
{
    // ── Raw record CRUD ───────────────────────────────────────────────────────

    /// <summary>Returns a single analytics record by its ID.</summary>
    Task<AnalyticsResponseDto> GetByIdAsync(int analyticsId);

    /// <summary>Returns analytics records with optional filters.</summary>
    Task<IEnumerable<AnalyticsResponseDto>> GetAllAsync(AnalyticsFilterDto? filter = null);

    /// <summary>Creates a new analytics data point.</summary>
    Task<AnalyticsResponseDto> CreateAsync(CreateAnalyticsRequestDto request);

    /// <summary>Updates mutable fields on an existing analytics record.</summary>
    Task<AnalyticsResponseDto> UpdateAsync(int analyticsId, UpdateAnalyticsRequestDto request);

    /// <summary>Deletes an analytics record.</summary>
    Task DeleteAsync(int analyticsId);

    // ── KPI reporting ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the main recruitment KPIs (latest value + period-over-period change)
    /// for the supplied date window.
    /// </summary>
    Task<IEnumerable<AnalyticsKpiDto>> GetRecruitmentKPIAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Returns the full recruitment analytics report covering time-to-hire,
    /// applicants per job, fill rates by department, and source effectiveness.
    /// </summary>
    Task<RecruitmentAnalyticsDto> GetRecruitmentAnalyticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Returns average, min, and max time-to-hire broken down by department.
    /// Supports date range filtering.
    /// </summary>
    Task<IEnumerable<TimeToHireDto>> GetTimeToHireAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Returns filled vs total positions per department as a percentage.
    /// Supports date range filtering.
    /// </summary>
    Task<IEnumerable<FillRateDto>> GetFillRatesAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);

    // ── Aggregations ──────────────────────────────────────────────────────────

    /// <summary>Returns aggregated metric summaries grouped by department.</summary>
    Task<IEnumerable<DepartmentMetricSummaryDto>> GetDepartmentSummariesAsync(
        AnalyticsFilterDto? filter = null);

    /// <summary>Returns the analytics dashboard with top metrics and latest values.</summary>
    Task<AnalyticsDashboardDto> GetDashboardAsync();

    // ── System health ─────────────────────────────────────────────────────────

    /// <summary>
    /// Probes the database, AI service, and blob storage and returns
    /// a <see cref="SystemHealthDto"/> snapshot with per-component status strings.
    /// </summary>
    Task<SystemHealthDto> GetSystemHealthAsync();
}

