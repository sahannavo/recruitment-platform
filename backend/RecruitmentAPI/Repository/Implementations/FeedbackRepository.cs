using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations;

/// <summary>
/// Implementation of Feedback repository
/// </summary>
public class FeedbackRepository : IFeedbackRepository
{
    private readonly ApplicationDbContext _context;

    public FeedbackRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InterviewFeedback?> GetByIdAsync(int feedbackId)
    {
        return await _context.InterviewFeedbacks
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.Candidate)
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.JobPosting)
            .Include(f => f.Manager)
            .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
    }

    public async Task<IEnumerable<InterviewFeedback>> GetByInterviewAsync(int interviewId)
    {
        return await _context.InterviewFeedbacks
            .Include(f => f.Manager)
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.Candidate)
            .Where(f => f.InterviewId == interviewId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<InterviewFeedback>> GetByManagerAsync(int managerId)
    {
        return await _context.InterviewFeedbacks
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.Candidate)
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.JobPosting)
            .Where(f => f.ManagerId == managerId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<InterviewFeedback>> GetByDecisionAsync(string decision)
    {
        return await _context.InterviewFeedbacks
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.Candidate)
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.JobPosting)
            .Include(f => f.Manager)
            .Where(f => f.Decision == decision)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<InterviewFeedback>> GetAllAsync()
    {
        return await _context.InterviewFeedbacks
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.Candidate)
            .Include(f => f.Interview)
                .ThenInclude(i => i!.Application)
                    .ThenInclude(a => a!.JobPosting)
            .Include(f => f.Manager)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<InterviewFeedback> AddAsync(InterviewFeedback feedback)
    {
        await _context.InterviewFeedbacks.AddAsync(feedback);
        return feedback;
    }

    public async Task UpdateAsync(InterviewFeedback feedback)
    {
        feedback.UpdatedAt = DateTime.UtcNow;
        _context.InterviewFeedbacks.Update(feedback);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int feedbackId)
    {
        var feedback = await _context.InterviewFeedbacks.FindAsync(feedbackId);
        if (feedback != null)
        {
            _context.InterviewFeedbacks.Remove(feedback);
        }
    }

    public async Task<bool> ExistsByInterviewAndManagerAsync(int interviewId, int managerId)
    {
        return await _context.InterviewFeedbacks
            .AnyAsync(f => f.InterviewId == interviewId && f.ManagerId == managerId);
    }
}
