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
        private bool _disposed;

        // Repository instances (lazy loaded)
        private IUserRepository? _users;
        private IGenericRepository<Admin>? _admins;
        private IGenericRepository<RecruitmentAnalytic>? _analytics;
        private IGenericRepository<Notification>? _notifications;
        private IGenericRepository<Recruiter>? _recruiters;
        private IGenericRepository<Document>? _documents;
        private IGenericRepository<HiringManager>? _hiringManagers;
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

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IGenericRepository<Admin> Admins => _admins ??= new GenericRepository<Admin>(_context);
        public IGenericRepository<RecruitmentAnalytic> RecruitmentAnalytics => _analytics ??= new GenericRepository<RecruitmentAnalytic>(_context);
        public IGenericRepository<Notification> Notifications => _notifications ??= new GenericRepository<Notification>(_context);
        public IGenericRepository<Recruiter> Recruiters => _recruiters ??= new GenericRepository<Recruiter>(_context);
        public IGenericRepository<Document> Documents => _documents ??= new GenericRepository<Document>(_context);
        public IGenericRepository<HiringManager> HiringManagers => _hiringManagers ??= new GenericRepository<HiringManager>(_context);

        public IAdminRepository AdminRepository => _adminRepository ??= new AdminRepository(_context);
        public IAnalyticsRepository AnalyticsRepository => _analyticsRepository ??= new AnalyticsRepository(_context);
        public IJobRepository Jobs => _jobs ??= new JobRepository(_context);
        public IApplicationRepository Applications => _applications ??= new ApplicationRepository(_context);
        public ICandidateRepository Candidates => _candidates ??= new CandidateRepository(_context);
        public IInterviewRepository Interviews => _interviews ??= new InterviewRepository(_context);
        public IFeedbackRepository Feedbacks => _feedbacks ??= new FeedbackRepository(_context);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync() =>
            _transaction = await _context.Database.BeginTransactionAsync();

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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}