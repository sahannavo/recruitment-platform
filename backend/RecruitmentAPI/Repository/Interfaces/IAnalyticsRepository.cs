using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces;

/// <summary>
/// Specialised repository for recruitment analytics queries
/// that support KPI calculations and reporting.
/// </summary>
public interface IAnalyticsRepository
{
    /// <summary>
    /// Returns the latest value for each distinct metric name,
    /// scoped to the optional date window.
    /// </summary>
    Task<IEnumerable<RecruitmentAnalytic>> GetKPIAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Returns all <c>TimeToHire</c> records grouped per department,
    /// scoped to the optional date window.
    /// </summary>
    Task<IEnumerable<RecruitmentAnalytic>> GetTimeToHireAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Returns all <c>FillRate</c> records grouped per department,
    /// scoped to the optional date window.
    /// </summary>
    Task<IEnumerable<RecruitmentAnalytic>> GetFillRatesAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Returns all records whose <c>MetricName</c> contains "source" (case-insensitive),
    /// used to calculate channel / source effectiveness.
    /// </summary>
    Task<IEnumerable<RecruitmentAnalytic>> GetSourceDataAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Returns all analytics records that fall within an optional
    /// department / metric / date range filter.
    /// </summary>
    Task<IEnumerable<RecruitmentAnalytic>> GetFilteredAsync(
        string? department = null,
        string? metricName = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
}

