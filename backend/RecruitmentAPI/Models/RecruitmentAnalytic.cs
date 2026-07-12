using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentPlatform.API.Models
{
    /// <summary>
    /// Represents a single recruitment analytics metric record, used to power
    /// admin dashboards and reporting (e.g. time-to-hire, applications per department).
    /// </summary>
    [Table("RecruitmentAnalytics")]
    public class RecruitmentAnalytic
    {
        /// <summary>Primary key.</summary>
        [Key]
        public int AnalyticsId { get; set; }

        /// <summary>Department the metric belongs to (e.g. "Engineering", "Sales").</summary>
        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        /// <summary>Date the metric applies to (day-granularity aggregation).</summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>Name of the metric being recorded (e.g. "ApplicationsReceived", "TimeToHireDays").</summary>
        [Required]
        [MaxLength(150)]
        public string MetricName { get; set; } = string.Empty;

        /// <summary>Numeric value of the metric.</summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        /// <summary>Timestamp the record was created (UTC).</summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

