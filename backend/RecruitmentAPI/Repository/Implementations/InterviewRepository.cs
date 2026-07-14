using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations;

/// <summary>
/// Implementation of Interview repository
/// </summary>
public class InterviewRepository : IInterviewRepository
{
    private readonly ApplicationDbContext _context;

    public InterviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Interview?> GetByIdAsync(int interviewId)
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a!.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a!.JobPosting)
            .Include(i => i.Feedbacks)
            .FirstOrDefaultAsync(i => i.InterviewId == interviewId);
    }

    public async Task<IEnumerable<Interview>> GetByCandidateAsync(int candidateId)
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a!.JobPosting)
            .Include(i => i.Feedbacks)
            .Where(i => i.Application!.CandidateId == candidateId)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetByRecruiterAsync(int recruiterId)
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a!.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a!.JobPosting)
            .Include(i => i.Feedbacks)
            .Where(i => i.Application!.JobPosting!.PostedBy == recruiterId)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetByHiringManagerAsync(int managerId)
    {
        // Assuming hiring managers can see interviews for their department
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a!.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a!.JobPosting)
            .Include(i => i.Feedbacks)
            .Where(i => i.Feedbacks.Any(f => f.ManagerId == managerId))
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a!.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a!.JobPosting)
            .Include(i => i.Feedbacks)
            .Where(i => i.ScheduledAt >= startDate && i.ScheduledAt <= endDate)
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetAllAsync()
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a!.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a!.JobPosting)
            .Include(i => i.Feedbacks)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync();
    }

    public async Task<Interview> AddAsync(Interview interview)
    {
        await _context.Interviews.AddAsync(interview);
        return interview;
    }

    public async Task UpdateAsync(Interview interview)
    {
        interview.UpdatedAt = DateTime.UtcNow;
        _context.Interviews.Update(interview);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int interviewId)
    {
        var interview = await _context.Interviews.FindAsync(interviewId);
        if (interview != null)
        {
            _context.Interviews.Remove(interview);
        }
    }

    public async Task<bool> ExistsAsync(int interviewId)
    {
        return await _context.Interviews.AnyAsync(i => i.InterviewId == interviewId);
    }

    public async Task<IEnumerable<Interview>> GetByStatusAsync(string status)
    {
        return await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a!.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a!.JobPosting)
            .Include(i => i.Feedbacks)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync();
    }
}
