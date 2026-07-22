using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Models;

public class PlatformSettings
{
    [Key]
    public int Id { get; set; } = 1;

    [MaxLength(200)]
    public string CompanyName { get; set; } = "Acme Corporation";
    
    [MaxLength(100)]
    public string Industry { get; set; } = "Technology";
    
    [MaxLength(255)]
    public string WebsiteUrl { get; set; } = "https://acme.inc";

    [MaxLength(255)]
    public string? OpenAIKey { get; set; }

    [MaxLength(255)]
    public string? AWSKey { get; set; }

    [MaxLength(255)]
    public string? SendGridApiKey { get; set; }

    public string? EmailTemplate { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal Creativity { get; set; } = 0.70m;

    [Column(TypeName = "decimal(3,2)")]
    public decimal Precision { get; set; } = 0.90m;

    [Column(TypeName = "decimal(3,2)")]
    public decimal Penalty { get; set; } = 0.00m;

    public bool SystemAlerts { get; set; } = true;
    public bool WeeklyReport { get; set; } = true;
    public bool ApiWarnings { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
