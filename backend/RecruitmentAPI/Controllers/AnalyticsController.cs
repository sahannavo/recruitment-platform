using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Controllers;

/// <summary>
/// Recruitment analytics endpoints: KPI reporting, time-to-hire,
/// fill rates, source effectiveness, system health, and raw record management.
/// </summary>
[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Admin,SuperAdmin")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger           = logger;
    }

    // =========================================================================
    // Recruitment overview  –  GET /api/analytics/recruitment
    // =========================================================================

    /// <summary>
    /// Returns the full recruitment analytics report: time-to-hire, applicants per job,
    /// fill rate by department, and sourcing channel effectiveness.
    /// Supports optional date range filtering.
    /// </summary>
    /// <param name="startDate">Report window start (inclusive, UTC).</param>
    /// <param name="endDate">Report window end (inclusive, UTC).</param>
    /// <response code="200">Report generated successfully.</response>
    /// <response code="401">Caller is not authenticated.</response>
    /// <response code="403">Caller lacks admin privileges.</response>
    [HttpGet("recruitment")]
    [ProducesResponseType(typeof(RecruitmentAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RecruitmentAnalyticsDto>> GetRecruitmentAnalytics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var report = await _analyticsService.GetRecruitmentAnalyticsAsync(startDate, endDate);
        return Ok(report);
    }

    // =========================================================================
    // KPIs  –  GET /api/analytics/kpi
    // =========================================================================

    /// <summary>
    /// Returns all recruitment KPIs with current value and period-over-period change.
    /// When a date range is supplied the previous equal-length window is used
    /// to calculate the change percentage.
    /// </summary>
    /// <param name="startDate">Current period start (UTC).</param>
    /// <param name="endDate">Current period end (UTC).</param>
    /// <response code="200">KPI list returned.</response>
    [HttpGet("kpi")]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsKpiDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AnalyticsKpiDto>>> GetRecruitmentKPI(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var kpis = await _analyticsService.GetRecruitmentKPIAsync(startDate, endDate);
        return Ok(kpis);
    }

    // =========================================================================
    // Time-to-hire  –  GET /api/analytics/time-to-hire
    // =========================================================================

    /// <summary>
    /// Returns average, minimum, and maximum time-to-hire (in calendar days)
    /// broken down by department. Supports date range filtering.
    /// </summary>
    /// <param name="startDate">Filter start date (UTC).</param>
    /// <param name="endDate">Filter end date (UTC).</param>
    /// <response code="200">Time-to-hire breakdown returned.</response>
    [HttpGet("time-to-hire")]
    [ProducesResponseType(typeof(IEnumerable<TimeToHireDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TimeToHireDto>>> GetTimeToHire(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var data = await _analyticsService.GetTimeToHireAsync(startDate, endDate);
        return Ok(data);
    }

    // =========================================================================
    // Fill rates  –  GET /api/analytics/fill-rates
    // =========================================================================

    /// <summary>
    /// Returns the job fill rate per department (filled vs total positions as a %).
    /// Supports date range filtering.
    /// </summary>
    /// <param name="startDate">Filter start date (UTC).</param>
    /// <param name="endDate">Filter end date (UTC).</param>
    /// <response code="200">Fill-rate data returned.</response>
    [HttpGet("fill-rates")]
    [ProducesResponseType(typeof(IEnumerable<FillRateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FillRateDto>>> GetFillRates(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var data = await _analyticsService.GetFillRatesAsync(startDate, endDate);
        return Ok(data);
    }

    // =========================================================================
    // System health  –  GET /api/analytics/system-health
    // =========================================================================

    /// <summary>
    /// Probes the database, AI scoring service, and blob storage and returns
    /// a health snapshot with per-component status and process uptime.
    /// </summary>
    /// <response code="200">Health snapshot returned (check ApiStatus field for overall result).</response>
    [HttpGet("system-health")]
    [ProducesResponseType(typeof(SystemHealthDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealth()
    {
        var health = await _analyticsService.GetSystemHealthAsync();

        // Return 200 even when degraded/unhealthy so the caller can read the details.
        // Log a warning so infra monitoring picks up non-healthy states.
        if (health.ApiStatus != "Healthy")
        {
            _logger.LogWarning(
                "System health check returned {Status} at {CheckedAt}",
                health.ApiStatus, health.CheckedAt);
        }

        return Ok(health);
    }

    // =========================================================================
    // Department summaries  –  GET /api/analytics/summaries
    // =========================================================================

    /// <summary>
    /// Returns aggregated metric totals and averages grouped by department.
    /// Supports filtering by department, metric name, and date range.
    /// </summary>
    /// <param name="filter">Optional filters.</param>
    /// <response code="200">Summaries returned.</response>
    [HttpGet("summaries")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentMetricSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentMetricSummaryDto>>> GetDepartmentSummaries(
        [FromQuery] AnalyticsFilterDto filter)
    {
        var summaries = await _analyticsService.GetDepartmentSummariesAsync(filter);
        return Ok(summaries);
    }

    // =========================================================================
    // Dashboard  –  GET /api/analytics/dashboard
    // =========================================================================

    /// <summary>
    /// Returns the analytics dashboard: record count, department/metric lists,
    /// top 10 metrics by total value, and latest value per metric.
    /// </summary>
    /// <response code="200">Dashboard data returned.</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AnalyticsDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetDashboard()
    {
        var dashboard = await _analyticsService.GetDashboardAsync();
        return Ok(dashboard);
    }

    // =========================================================================
    // Raw record CRUD
    // =========================================================================

    /// <summary>
    /// Returns all analytics records, optionally filtered by department,
    /// metric name, and date range.
    /// </summary>
    /// <param name="filter">Optional query filters.</param>
    /// <response code="200">Records returned.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AnalyticsResponseDto>>> GetAll(
        [FromQuery] AnalyticsFilterDto filter)
    {
        var records = await _analyticsService.GetAllAsync(filter);
        return Ok(records);
    }

    /// <summary>Returns a single analytics record by its ID.</summary>
    /// <param name="id">Analytics record ID.</param>
    /// <response code="200">Record found.</response>
    /// <response code="404">Record not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AnalyticsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalyticsResponseDto>> GetById(int id)
    {
        var record = await _analyticsService.GetByIdAsync(id);
        return Ok(record);
    }

    /// <summary>Creates a new analytics data point.</summary>
    /// <param name="request">Record details (department, date, metricName, value).</param>
    /// <response code="201">Record created.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AnalyticsResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnalyticsResponseDto>> Create(
        [FromBody] CreateAnalyticsRequestDto request)
    {
        var record = await _analyticsService.CreateAsync(request);

        _logger.LogInformation(
            "Analytics record created via API: AnalyticsId {AnalyticsId}", record.AnalyticsId);

        return CreatedAtAction(nameof(GetById), new { id = record.AnalyticsId }, record);
    }

    /// <summary>Updates an existing analytics record.</summary>
    /// <param name="id">Analytics record ID.</param>
    /// <param name="request">Fields to update (all optional).</param>
    /// <response code="200">Record updated.</response>
    /// <response code="404">Record not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AnalyticsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalyticsResponseDto>> Update(
        int id,
        [FromBody] UpdateAnalyticsRequestDto request)
    {
        var record = await _analyticsService.UpdateAsync(id, request);
        return Ok(record);
    }

    /// <summary>Deletes an analytics record.</summary>
    /// <param name="id">Analytics record ID.</param>
    /// <response code="204">Record deleted.</response>
    /// <response code="404">Record not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _analyticsService.DeleteAsync(id);
        return NoContent();
    }
}

