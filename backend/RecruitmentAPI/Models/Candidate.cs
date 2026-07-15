namespace RecruitmentAPI.Models
{
    public class Candidate : User
    {
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? LinkedIn { get; set; }
        public string? SkillsSummary { get; set; }
    }
}
