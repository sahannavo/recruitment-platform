using RecruitmentAPI.Models;

namespace Backend.RecruitmentAPI.Models
{
    public class Recruiter : User
    {
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
    }
}