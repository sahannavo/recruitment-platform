namespace RecruitmentAPI.Repository.Interfaces
{
    /// <summary>
    /// Interface for managing database transactions and coordinating repositories.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }

        // As team members build their features, they will add their repositories here:
        // IJobRepository Jobs { get; }
        // IApplicationRepository Applications { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}