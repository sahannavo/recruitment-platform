namespace RecruitmentAPI.Models
{
    public class Candidate : User
    {
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? LinkedIn { get; set; }
        public string? SkillsSummary { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool? IsAvailable { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public string? NoticePeriod { get; set; }
        public bool? IsOpenToOpportunities { get; set; }
        public bool? WillingToRelocate { get; set; }
        public bool? WillingToWorkRemote { get; set; }
        public string? PreferredLocations { get; set; }
    }
}
