namespace RecruitmentAPI.Models
{
    public class HiringManager : User
    {
        public string? Department { get; set; }
        public string? ReportingTo { get; set; }
    }
}