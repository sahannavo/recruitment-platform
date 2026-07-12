namespace RecruitmentAPI.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// Raw Analytics Record DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Response DTO for a single recruitment analytics record.
/// </summary>
public class AnalyticsResponseDto
{
    public int AnalyticsId { get; set; }
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a new analytics record.
/// </summary>
public class CreateAnalyticsRequestDto
{
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

/// <summary>
/// Request DTO for updating an existing analytics record.
/// </summary>
public class UpdateAnalyticsRequestDto
{
    public string? Department { get; set; }
    public DateTime? Date { get; set; }
    public string? MetricName { get; set; }
    public decimal? Value { get; set; }
}

/// <summary>
/// Query parameters for filtering analytics records.
/// </summary>
public class AnalyticsFilterDto
{
    public string? Department { get; set; }
    public string? MetricName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// KPI DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// KPI summary DTO showing current value and change vs the previous period.
/// </summary>
public class AnalyticsKpiDto
{
    /// <summary>Human-readable name of the KPI (e.g., "Time to Hire", "Applicants Per Job").</summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>Current period value.</summary>
    public decimal Value { get; set; }

    /// <summary>Absolute change from the previous period (positive = improved).</summary>
    public decimal Change { get; set; }

    /// <summary>Percentage change from the previous period.</summary>
    public decimal ChangePercentage { get; set; }

    /// <summary>Trend direction: "up", "down", or "stable".</summary>
    public string Trend => ChangePercentage > 0.5m ? "up"
                         : ChangePercentage < -0.5m ? "down"
                         : "stable";
}

// ─────────────────────────────────────────────────────────────────────────────
// High-Level Analytics Report DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Comprehensive recruitment analytics report covering the main hiring funnel KPIs.
/// </summary>
public class RecruitmentAnalyticsDto
{
    /// <summary>Average calendar days from job posting to offer acceptance.</summary>
    public decimal TimeToHire { get; set; }

    /// <summary>Average number of applicants per open job posting.</summary>
    public decimal ApplicantsPerJob { get; set; }

    /// <summary>Job fill rate broken down by department (department → fill-rate %).</summary>
    public Dictionary<string, decimal> FillRateByDepartment { get; set; } = new();

    /// <summary>Effectiveness of each sourcing channel as a percentage of hires.</summary>
    public Dictionary<string, decimal> SourceEffectiveness { get; set; } = new();

    /// <summary>Date range this report covers.</summary>
    public DateTime ReportStartDate { get; set; }

    /// <summary>Date range this report covers.</summary>
    public DateTime ReportEndDate { get; set; }
}

/// <summary>
/// Time-to-hire breakdown by department, useful for identifying bottlenecks.
/// </summary>
public class TimeToHireDto
{
    public string Department { get; set; } = string.Empty;

    /// <summary>Average days to hire in this department.</summary>
    public decimal AverageDays { get; set; }

    /// <summary>Minimum days observed in this period.</summary>
    public decimal MinDays { get; set; }

    /// <summary>Maximum days observed in this period.</summary>
    public decimal MaxDays { get; set; }

    /// <summary>Total number of hires included in this calculation.</summary>
    public int HireCount { get; set; }
}

/// <summary>
/// Department fill-rate entry: how many open positions were filled vs total.
/// </summary>
public class FillRateDto
{
    public string Department { get; set; } = string.Empty;
    public int TotalPositions { get; set; }
    public int FilledPositions { get; set; }
    public decimal FillRatePercentage { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// System Health DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// System health snapshot used by the health-check endpoint.
/// </summary>
public class SystemHealthDto
{
    /// <summary>Overall API availability status (Healthy / Degraded / Unhealthy).</summary>
    public string ApiStatus { get; set; } = "Healthy";

    /// <summary>Database connectivity status.</summary>
    public string DatabaseStatus { get; set; } = string.Empty;

    /// <summary>AI scoring service availability status.</summary>
    public string AIStatus { get; set; } = string.Empty;

    /// <summary>Azure Blob Storage / file storage availability status.</summary>
    public string BlobStatus { get; set; } = string.Empty;

    /// <summary>Process uptime since last restart.</summary>
    public string Uptime { get; set; } = string.Empty;

    /// <summary>UTC timestamp when this snapshot was taken.</summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Additional per-component messages (component → message).</summary>
    public Dictionary<string, string> Details { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────
// Aggregation / Summary DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Aggregated metric summary grouped by department.
/// </summary>
public class DepartmentMetricSummaryDto
{
    public string Department { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public decimal AverageValue { get; set; }
    public int RecordCount { get; set; }
}

/// <summary>
/// Response DTO for analytics dashboard overview.
/// </summary>
public class AnalyticsDashboardDto
{
    public int TotalRecords { get; set; }
    public List<string> Departments { get; set; } = new();
    public List<string> MetricNames { get; set; } = new();
    public List<DepartmentMetricSummaryDto> TopMetrics { get; set; } = new();
    public Dictionary<string, decimal> LatestMetricValues { get; set; } = new();
}

