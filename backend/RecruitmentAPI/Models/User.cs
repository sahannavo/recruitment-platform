namespace RecruitmentAPI.Models;

/// <summary>
/// Base user entity shared across all role-specific tables.
/// </summary>
public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Admin? Admin { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

