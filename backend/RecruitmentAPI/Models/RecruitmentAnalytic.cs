using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentPlatform.Models
{
    /// <summary>
    /// Stores computed/aggregated recruitment metrics, one row per (Department, Date, MetricName).
    /// Populated by a scheduled job or on-demand calculation from Applications/JobPostings/Interviews.
    /// </summary>
    [Table("RecruitmentAnalytics")]
    public class RecruitmentAnalytic
    {
        [Key]
        public int AnalyticsId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(100)]
        public string MetricName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

