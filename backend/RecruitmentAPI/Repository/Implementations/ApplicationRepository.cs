using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repositories.Interfaces;

namespace RecruitmentAPI.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Application entity operations
    /// </summary>
    public class ApplicationRepository : Repository<Application>, IApplicationRepository
    {
        public ApplicationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get all applications by candidate ID
        /// </summary>
        public async Task<IEnumerable<Application>> GetByCandidateAsync(int candidateId)
        {
            return await _context.Applications
                .Where(a => a.CandidateId == candidateId)
                .Include(a => a.Job)
                .Include(a => a.Candidate)
                .Include(a => a.Interviews)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get all applications for a specific job
        /// </summary>
        public async Task<IEnumerable<Application>> GetByJobAsync(int jobId)
        {
            return await _context.Applications
                .Where(a => a.JobId == jobId)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Include(a => a.Interviews)
                .OrderByDescending(a => a.AI_Score)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications by status
        /// </summary>
        public async Task<IEnumerable<Application>> GetByStatusAsync(ApplicationStatus status)
        {
            return await _context.Applications
                .Where(a => a.Status == status)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Include(a => a.Interviews)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications with AI score greater than threshold
        /// </summary>
        public async Task<IEnumerable<Application>> GetWithAI_ScoreAsync(double threshold)
        {
            return await _context.Applications
                .Where(a => a.AI_Score.HasValue && a.AI_Score.Value >= threshold)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Include(a => a.Interviews)
                .OrderByDescending(a => a.AI_Score)
                .ToListAsync();
        }

        /// <summary>
        /// Get application by candidate and job
        /// </summary>
        public async Task<Application> GetByCandidateAndJobAsync(int candidateId, int jobId)
        {
            return await _context.Applications
                .FirstOrDefaultAsync(a => a.CandidateId == candidateId && a.JobId == jobId);
        }

        /// <summary>
        /// Get application with full details
        /// </summary>
        public async Task<Application> GetApplicationWithDetailsAsync(int applicationId)
        {
            return await _context.Applications
                .Where(a => a.ApplicationId == applicationId)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Recruiter)
                .Include(a => a.Interviews)
                    .ThenInclude(i => i.Feedbacks)
                .Include(a => a.Reviewer)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get applications by date range
        /// </summary>
        public async Task<IEnumerable<Application>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Applications
                .Where(a => a.AppliedAt >= startDate && a.AppliedAt <= endDate)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications for a recruiter's jobs
        /// </summary>
        public async Task<IEnumerable<Application>> GetByRecruiterAsync(int recruiterId)
        {
            return await _context.Applications
                .Where(a => a.Job.PostedBy == recruiterId)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Include(a => a.Interviews)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get recent applications (last N days)
        /// </summary>
        public async Task<IEnumerable<Application>> GetRecentApplicationsAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _context.Applications
                .Where(a => a.AppliedAt >= cutoffDate)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications by AI score range
        /// </summary>
        public async Task<IEnumerable<Application>> GetByAIScoreRangeAsync(double minScore, double maxScore)
        {
            return await _context.Applications
                .Where(a => a.AI_Score.HasValue && a.AI_Score.Value >= minScore && a.AI_Score.Value <= maxScore)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .OrderByDescending(a => a.AI_Score)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications with status history
        /// </summary>
        public async Task<Application> GetApplicationWithStatusHistoryAsync(int applicationId)
        {
            // Note: You would need a StatusHistory entity for this
            // This is a placeholder implementation
            return await GetApplicationWithDetailsAsync(applicationId);
        }

        /// <summary>
        /// Get count of applications by status for a job
        /// </summary>
        public async Task<Dictionary<ApplicationStatus, int>> GetApplicationCountByStatusAsync(int jobId)
        {
            var applications = await _context.Applications
                .Where(a => a.JobId == jobId)
                .ToListAsync();

            return applications
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Get applications by source
        /// </summary>
        public async Task<IEnumerable<Application>> GetBySourceAsync(string source)
        {
            return await _context.Applications
                .Where(a => a.Source.ToLower() == source.ToLower())
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications that are pending review
        /// </summary>
        public async Task<IEnumerable<Application>> GetPendingReviewApplicationsAsync()
        {
            return await _context.Applications
                .Where(a => a.Status == ApplicationStatus.Submitted)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .OrderBy(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications with interview scheduled
        /// </summary>
        public async Task<IEnumerable<Application>> GetApplicationsWithInterviewsAsync()
        {
            return await _context.Applications
                .Where(a => a.Status == ApplicationStatus.InterviewScheduled)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Include(a => a.Interviews)
                .OrderBy(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications by hiring manager
        /// </summary>
        public async Task<IEnumerable<Application>> GetByHiringManagerAsync(int hiringManagerId)
        {
            // This would need a relationship between JobPosting and HiringManager
            // Placeholder implementation
            return await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Where(a => a.Job.Department == "Engineering") // Placeholder
                .ToListAsync();
        }

        /// <summary>
        /// Search applications by candidate name or job title
        /// </summary>
        public async Task<IEnumerable<Application>> SearchApplicationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var term = searchTerm.ToLower();
            return await _context.Applications
                .Where(a => a.Candidate.FirstName.ToLower().Contains(term) ||
                            a.Candidate.LastName.ToLower().Contains(term) ||
                            a.Job.Title.ToLower().Contains(term) ||
                            a.Job.Department.ToLower().Contains(term))
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get application statistics for dashboard
        /// </summary>
        public async Task<ApplicationStatistics> GetApplicationStatisticsAsync()
        {
            var applications = await _context.Applications
                .Include(a => a.Job)
                .ToListAsync();

            var stats = new ApplicationStatistics
            {
                TotalApplications = applications.Count,
                Submitted = applications.Count(a => a.Status == ApplicationStatus.Submitted),
                UnderReview = applications.Count(a => a.Status == ApplicationStatus.UnderReview),
                Shortlisted = applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
                InterviewScheduled = applications.Count(a => a.Status == ApplicationStatus.InterviewScheduled),
                Interviewed = applications.Count(a => a.Status == ApplicationStatus.Interviewed),
                Hired = applications.Count(a => a.Status == ApplicationStatus.Hired),
                Rejected = applications.Count(a => a.Status == ApplicationStatus.Rejected),
                Withdrawn = applications.Count(a => a.Status == ApplicationStatus.Withdrawn),
                OnHold = applications.Count(a => a.Status == ApplicationStatus.OnHold),
                LastApplicationDate = applications.Any() ? applications.Max(a => a.AppliedAt) : DateTime.UtcNow
            };

            if (applications.Any(a => a.AI_Score.HasValue))
            {
                var scores = applications.Where(a => a.AI_Score.HasValue).Select(a => a.AI_Score.Value);
                stats.AverageAIScore = scores.Average();
                stats.HighestAIScore = scores.Max();
                stats.LowestAIScore = scores.Min();
            }

            stats.ApplicationsByDepartment = applications
                .GroupBy(a => a.Job.Department)
                .ToDictionary(g => g.Key, g => g.Count());

            stats.ApplicationsBySource = applications
                .GroupBy(a => a.Source ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            stats.ApplicationsByMonth = applications
                .GroupBy(a => a.AppliedAt.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }

        /// <summary>
        /// Bulk update application status
        /// </summary>
        public async Task<int> BulkUpdateStatusAsync(List<int> applicationIds, ApplicationStatus newStatus, string notes = null)
        {
            var applications = await _context.Applications
                .Where(a => applicationIds.Contains(a.ApplicationId))
                .ToListAsync();

            foreach (var application in applications)
            {
                application.Status = newStatus;
                application.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(notes))
                    application.Notes = notes;

                // Update timestamps based on status
                switch (newStatus)
                {
                    case ApplicationStatus.UnderReview:
                        application.ReviewedAt = DateTime.UtcNow;
                        break;
                    case ApplicationStatus.Shortlisted:
                        application.ShortlistedAt = DateTime.UtcNow;
                        break;
                    case ApplicationStatus.Interviewed:
                        application.InterviewedAt = DateTime.UtcNow;
                        break;
                    case ApplicationStatus.Hired:
                        application.HiredAt = DateTime.UtcNow;
                        break;
                    case ApplicationStatus.Rejected:
                        application.RejectedAt = DateTime.UtcNow;
                        break;
                }
            }

            await _context.SaveChangesAsync();
            return applications.Count;
        }

        /// <summary>
        /// Get applications by department
        /// </summary>
        public async Task<IEnumerable<Application>> GetByDepartmentAsync(string department)
        {
            return await _context.Applications
                .Where(a => a.Job.Department.ToLower() == department.ToLower())
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications with AI scores for a specific job
        /// </summary>
        public async Task<IEnumerable<Application>> GetJobApplicationsWithAIScoresAsync(int jobId, double? threshold = null)
        {
            var query = _context.Applications
                .Where(a => a.JobId == jobId && a.AI_Score.HasValue);

            if (threshold.HasValue)
                query = query.Where(a => a.AI_Score.Value >= threshold.Value);

            return await query
                .Include(a => a.Candidate)
                .OrderByDescending(a => a.AI_Score)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications for a candidate with job details
        /// </summary>
        public async Task<IEnumerable<Application>> GetCandidateApplicationsWithJobDetailsAsync(int candidateId)
        {
            return await _context.Applications
                .Where(a => a.CandidateId == candidateId)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Recruiter)
                .Include(a => a.Interviews)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get total application count for a job
        /// </summary>
        public async Task<int> GetApplicationCountForJobAsync(int jobId)
        {
            return await _context.Applications
                .CountAsync(a => a.JobId == jobId);
        }

        /// <summary>
        /// Get active applications for a candidate
        /// </summary>
        public async Task<IEnumerable<Application>> GetActiveApplicationsForCandidateAsync(int candidateId)
        {
            var activeStatuses = new[]
            {
                ApplicationStatus.Submitted,
                ApplicationStatus.UnderReview,
                ApplicationStatus.Shortlisted,
                ApplicationStatus.InterviewScheduled,
                ApplicationStatus.Interviewed,
                ApplicationStatus.OnHold
            };

            return await _context.Applications
                .Where(a => a.CandidateId == candidateId && activeStatuses.Contains(a.Status))
                .Include(a => a.Job)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications with feedback
        /// </summary>
        public async Task<Application> GetApplicationWithFeedbackAsync(int applicationId)
        {
            return await _context.Applications
                .Where(a => a.ApplicationId == applicationId)
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Include(a => a.Interviews)
                    .ThenInclude(i => i.Feedbacks)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Check if candidate has applied to job
        /// </summary>
        public async Task<bool> HasCandidateAppliedAsync(int candidateId, int jobId)
        {
            return await _context.Applications
                .AnyAsync(a => a.CandidateId == candidateId && a.JobId == jobId);
        }
    }
}