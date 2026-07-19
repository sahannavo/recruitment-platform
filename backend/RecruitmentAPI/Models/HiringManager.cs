namespace RecruitmentAPI.Models
{
    public class HiringManager : User
    {
        public int HiringManagerId { get; set; }
        public int UserId { get; set; }
        public string? Department { get; set; }
        public string? ReportingTo { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        //public virtual ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
        public new ICollection<JobPosting> JobPostings { get; set; }
    }
}