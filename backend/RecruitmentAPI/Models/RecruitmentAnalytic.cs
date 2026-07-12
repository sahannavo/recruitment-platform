namespace RecruitmentAPI.Models;

/// <summary>
/// Stores recruitment metrics aggregated by department and date.
/// </summary>
public class RecruitmentAnalytic
{
    public int AnalyticsId { get; set; }
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

