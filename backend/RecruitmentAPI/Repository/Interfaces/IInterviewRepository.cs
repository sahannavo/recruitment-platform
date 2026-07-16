using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces;

/// <summary>
/// Repository interface for Interview operations
/// </summary>
public interface IInterviewRepository
{
    /// <summary>
    /// Get interview by ID
    /// </summary>
    Task<Interview?> GetByIdAsync(int interviewId);

    /// <summary>
    /// Get all interviews for a specific candidate
    /// </summary>
    Task<IEnumerable<Interview>> GetByCandidateAsync(int candidateId);

    /// <summary>
    /// Get all interviews managed by a specific recruiter
    /// </summary>
    Task<IEnumerable<Interview>> GetByRecruiterAsync(int recruiterId);

    /// <summary>
    /// Get all interviews for a specific hiring manager's department
    /// </summary>
    Task<IEnumerable<Interview>> GetByHiringManagerAsync(int managerId);

    /// <summary>
    /// Get interviews within a specific date range
    /// </summary>
    Task<IEnumerable<Interview>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get all interviews
    /// </summary>
    Task<IEnumerable<Interview>> GetAllAsync();

    /// <summary>
    /// Add a new interview
    /// </summary>
    Task<Interview> AddAsync(Interview interview);

    /// <summary>
    /// Update an existing interview
    /// </summary>
    Task UpdateAsync(Interview interview);

    /// <summary>
    /// Delete an interview
    /// </summary>
    Task DeleteAsync(int interviewId);

    /// <summary>
    /// Check if an interview exists
    /// </summary>
    Task<bool> ExistsAsync(int interviewId);

    /// <summary>
    /// Get interviews by status
    /// </summary>
    Task<IEnumerable<Interview>> GetByStatusAsync(string status);

    /// <summary>
    /// Get interviews by application ID
    /// </summary>
    Task<IEnumerable<Interview>> GetByApplicationIdAsync(int applicationId);
}
