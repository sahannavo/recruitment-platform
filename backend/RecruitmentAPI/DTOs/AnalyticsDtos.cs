using System;
using System.Collections.Generic;

namespace RecruitmentAPI.DTOs
{
    public class RecruitmentAnalyticsDto 
    {
        public decimal TimeToHire { get; set; }
        public decimal ApplicantsPerJob { get; set; }
        public Dictionary<string, decimal> FillRateByDepartment { get; set; } = new();
        public Dictionary<string, decimal> SourceEffectiveness { get; set; } = new();
        public DateTime ReportStartDate { get; set; }
        public DateTime ReportEndDate { get; set; }
    }

    public class AnalyticsKpiDto 
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercentage { get; set; }
    }

    public class TimeToHireDto 
    {
        public string Department { get; set; } = string.Empty;
        public double AverageDays { get; set; }
        public decimal MinDays { get; set; }
        public decimal MaxDays { get; set; }
        public int HireCount { get; set; }
    }

    public class FillRateDto 
    {
        public string Department { get; set; } = string.Empty;
        public int TotalPositions { get; set; }
        public int FilledPositions { get; set; }
        public double FillRatePercentage { get; set; }
    }

    public class SystemHealthDto 
    {
        public string ApiStatus { get; set; } = "Healthy";
        public string DatabaseStatus { get; set; } = "Healthy";
        public string AIStatus { get; set; } = "Healthy";
        public string BlobStatus { get; set; } = "Healthy";
        public string Uptime { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> Details { get; set; } = new();
    }

    public class DepartmentMetricSummaryDto 
    {
        public string Department { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }
        public decimal AverageValue { get; set; }
        public int RecordCount { get; set; }
    }

    public class AnalyticsDashboardDto 
    {
        public int TotalRecords { get; set; }
        public List<string> Departments { get; set; } = new();
        public List<string> MetricNames { get; set; } = new();
        public List<DepartmentMetricSummaryDto> TopMetrics { get; set; } = new();
        public Dictionary<string, decimal> LatestMetricValues { get; set; } = new();
    }

    public class AnalyticsFilterDto 
    {
        public string? Department { get; set; }
        public string? MetricName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AnalyticsResponseDto 
    {
        public int AnalyticsId { get; set; }
        public string Department { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAnalyticsRequestDto 
    {
        public string Department { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class UpdateAnalyticsRequestDto 
    {
        public string? Department { get; set; }
        public DateTime? Date { get; set; }
        public string? MetricName { get; set; }
        public decimal? Value { get; set; }
    }
}