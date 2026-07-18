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

    /// <summary>Role discriminator: Candidate | Recruiter | HiringManager | Admin | SuperAdmin</summary>
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Admin? Admin { get; set; }
    public Candidate? Candidate { get; set; }
    public Recruiter? Recruiter { get; set; }
    public HiringManager? HiringManager { get; set; }
    public virtual ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}

