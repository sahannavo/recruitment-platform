
namespace RecruitmentAPI.Models;

/// <summary>
/// Admin profile — supplementary data for users whose <see cref="User.Role"/> is "Admin"
/// or "SuperAdmin". Stores department and permission metadata.
/// </summary>
public class Admin
{
    public int AdminId { get; set; }

    /// <summary>Foreign key to the base <see cref="User"/> record.</summary>
    public int UserId { get; set; }

    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated or JSON permission list, e.g. "ManageUsers,ViewAnalytics".
    /// </summary>
    public string Permissions { get; set; } = string.Empty;

    // Navigation
    public User User { get; set; } = null!;
}
