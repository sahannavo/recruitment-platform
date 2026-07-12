namespace RecruitmentAPI.Models;

/// <summary>
/// Admin role entity linked to the base User table.
/// </summary>
public class Admin
{
    public int AdminId { get; set; }
    public int UserId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}

