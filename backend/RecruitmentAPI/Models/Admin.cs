using RecruitmentAPI.Models;

namespace Backend.RecruitmentAPI.Models
{
    public class Admin : User
    {
        public string Role { get; set; } = "Admin"; // "SuperAdmin" or "Admin"
        public string? PermissionsJson { get; set; }
    }
}