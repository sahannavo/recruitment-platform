using System;
using System.Threading.Tasks;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Repository.Interfaces;

/// <summary>
/// Unit of Work contract providing access to all repositories and transaction management.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Generic repositories
    IGenericRepository<User> Users { get; }
    IGenericRepository<Admin> Admins { get; }
    IGenericRepository<RecruitmentAnalytic> RecruitmentAnalytics { get; }
    IGenericRepository<Notification> Notifications { get; }

    // Specialised repositories
    IAdminRepository AdminRepository { get; }
    IAnalyticsRepository AnalyticsRepository { get; }
    IJobRepository Jobs { get; }
    IApplicationRepository Applications { get; }
    ICandidateRepository Candidates { get; } // Assure ICandidateRepository is defined in this same namespace folder!
    IInterviewRepository Interviews { get; }
    IFeedbackRepository Feedbacks { get; }

    // Transaction management
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}