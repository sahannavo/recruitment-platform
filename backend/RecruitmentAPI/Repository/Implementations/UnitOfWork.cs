using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using RecruitmentAPI.Data;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IGenericRepository<User>? _users;
        private IGenericRepository<Admin>? _admins;
        private IGenericRepository<RecruitmentAnalytic>? _analytics;
        private IGenericRepository<Notification>? _notifications;
        private IAdminRepository? _adminRepository;
        private IAnalyticsRepository? _analyticsRepository;
        private IJobRepository? _jobs;
        private IApplicationRepository? _applications;
        private ICandidateRepository? _candidates;
        private IInterviewRepository? _interviews;
        private IFeedbackRepository? _feedbacks;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
        public IGenericRepository<Admin> Admins => _admins ??= new GenericRepository<Admin>(_context);
        public IGenericRepository<RecruitmentAnalytic> RecruitmentAnalytics => _analytics ??= new GenericRepository<RecruitmentAnalytic>(_context);
        public IGenericRepository<Notification> Notifications => _notifications ??= new GenericRepository<Notification>(_context);

        public IAdminRepository AdminRepository => _adminRepository ??= new AdminRepository(_context);
        public IAnalyticsRepository AnalyticsRepository => _analyticsRepository ??= new AnalyticsRepository(_context);
        public IJobRepository Jobs => _jobs ??= new JobRepository(_context);
        public IApplicationRepository Applications => _applications ??= new ApplicationRepository(_context);
        public ICandidateRepository Candidates => _candidates ??= new CandidateRepository(_context);
        public IInterviewRepository Interviews => _interviews ??= new InterviewRepository(_context);
        public IFeedbackRepository Feedbacks => _feedbacks ??= new FeedbackRepository(_context);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}