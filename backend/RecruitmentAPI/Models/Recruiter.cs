namespace RecruitmentAPI.Models
{
    public class Recruiter : User
    {
        public int RecruiterId { get; set; }
        public int UserId { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public virtual ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    }
}