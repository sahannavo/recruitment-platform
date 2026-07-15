using System;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentAPI.Models
{
    public class Recruiter
    {
        [Key]
        public int RecruiterId { get; set; }
        public int UserId { get; set; }
        public string Department { get; set; } = string.Empty;
    }

    public class HiringManager
    {
        [Key]
        public int ManagerId { get; set; }
        public int UserId { get; set; }
        public string Department { get; set; } = string.Empty;
    }
}