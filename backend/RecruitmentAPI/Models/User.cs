namespace RecruitmentAPI.Models;

/// <summary>
/// Base user account shared by all roles (Candidate, Recruiter, HiringManager, Admin, SuperAdmin).
/// </summary>
public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    /// <summary>Role discriminator: Candidate | Recruiter | HiringManager | Admin | SuperAdmin</summary>
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary> Computed full name</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation properties
    public Admin? Admin { get; set; }
    public Candidate? Candidate { get; set; }
    public Recruiter? Recruiter { get; set; }
    public HiringManager? HiringManager { get; set; }
    public virtual ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}