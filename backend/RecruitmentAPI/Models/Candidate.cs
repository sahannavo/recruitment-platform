using System.ComponentModel.DataAnnotations;

namespace RecruitmentAPI.Models;

/// <summary>
/// Candidate profile — supplementary data for users whose <see cref="User.Role"/> is "Candidate".
/// Uses a separate table with a foreign key to the base <see cref="User"/> record.
/// </summary>
public class Candidate
{
    public int CandidateId { get; set; }

    /// <summary>Foreign key to the base <see cref="User"/> record.</summary>
    public int UserId { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(200)]
    public string? LinkedIn { get; set; }

    [StringLength(2000)]
    public string? SkillsSummary { get; set; }

    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }

    [StringLength(50)]
    public string? NoticePeriod { get; set; }

    [StringLength(500)]
    public string? PreferredLocations { get; set; }

    public bool IsAvailable { get; set; } = true;
    public bool IsOpenToOpportunities { get; set; } = true;
    public DateTime? AvailableFrom { get; set; }
    public bool WillingToRelocate { get; set; }
    public bool WillingToWorkRemote { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    /// <summary>Documents collection for CVs and other files</summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}