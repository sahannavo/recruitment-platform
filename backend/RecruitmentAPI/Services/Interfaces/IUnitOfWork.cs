using System;
using System.Threading.Tasks;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Interfaces
{
    /// <summary>
    /// Unit of Work contract providing access to all repositories and transaction management.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Generic repositories
        IUserRepository Users { get; }
        IGenericRepository<Admin> Admins { get; }
        IGenericRepository<RecruitmentAnalytic> RecruitmentAnalytics { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<Recruiter> Recruiters { get; }
        IGenericRepository<Document> Documents { get; }
        IGenericRepository<HiringManager> HiringManagers { get; }

        // Specialised repositories
        IAdminRepository AdminRepository { get; }
        IAnalyticsRepository AnalyticsRepository { get; }
        IJobRepository Jobs { get; }
        IApplicationRepository Applications { get; }
        ICandidateRepository Candidates { get; }
        IInterviewRepository Interviews { get; }
        IFeedbackRepository Feedbacks { get; }

        // Transaction management
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}