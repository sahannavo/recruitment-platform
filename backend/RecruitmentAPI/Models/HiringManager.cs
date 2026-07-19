namespace RecruitmentAPI.Models
{
    /// <summary>
    /// HiringManager profile — supplementary data for users whose <see cref="User.Role"/> is "HiringManager".
    /// Uses a separate table with a foreign key to the base <see cref="User"/> record.
    /// </summary>
    public class HiringManager
    {
        public int HiringManagerId { get; set; }

        /// <summary>Foreign key to the base <see cref="User"/> record.</summary>
        public int UserId { get; set; }

        public string? Department { get; set; }
        public string? ReportingTo { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    }
}