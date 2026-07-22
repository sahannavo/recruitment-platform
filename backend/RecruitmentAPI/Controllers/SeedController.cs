using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeedController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunSeed()
        {
            // 1. Create a Recruiter User and Profile
            var rUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "r@demo.com");
            if (rUser == null)
            {
                rUser = new User { Email = "r@demo.com", FirstName = "Demo", LastName = "Recruiter", PasswordHash = "xyz", Role = "Recruiter" };
                _context.Users.Add(rUser);
                await _context.SaveChangesAsync();

                _context.Recruiters.Add(new Recruiter { UserId = rUser.UserId, Department = "Engineering" });
                await _context.SaveChangesAsync();
            }

            var recruiterId = (await _context.Recruiters.FirstAsync(r => r.UserId == rUser.UserId)).RecruiterId;

            // 2. Create Jobs
            var job1 = new JobPosting
            {
                Title = "Senior Frontend Developer",
                Department = "Engineering",
                Description = "Build beautiful UIs.",
                Requirements = "React, JS, CSS",
                Location = "Remote",
                SalaryRange = "$120k - $150k",
                Status = JobStatus.Open,
                EmploymentType = "Full-time",
                RecruiterId = recruiterId
            };
            var job2 = new JobPosting
            {
                Title = "Product Manager",
                Department = "Product",
                Description = "Lead the product.",
                Requirements = "Agile, Jira",
                Location = "New York, NY",
                SalaryRange = "$130k - $160k",
                Status = JobStatus.Open,
                EmploymentType = "Full-time",
                RecruiterId = recruiterId
            };
            _context.JobPostings.AddRange(job1, job2);
            await _context.SaveChangesAsync();

            // 3. Create Candidate
            var cUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "c@demo.com");
            if (cUser == null)
            {
                cUser = new User { Email = "c@demo.com", FirstName = "Demo", LastName = "Candidate", PasswordHash = "xyz", Role = "Candidate" };
                _context.Users.Add(cUser);
                await _context.SaveChangesAsync();

                _context.Candidates.Add(new Candidate { UserId = cUser.UserId, SkillsSummary = "React, Node.js" });
                await _context.SaveChangesAsync();
            }
            var candidateId = (await _context.Candidates.FirstAsync(c => c.UserId == cUser.UserId)).CandidateId;

            // 4. Create Applications
            _context.Applications.Add(new Application
            {
                JobId = job1.JobId,
                CandidateId = candidateId,
                Status = ApplicationStatus.Submitted,
                Source = "Direct"
            });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Seeded successfully" });
        }
    }
}
