using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Repository.Implementations;

/// <summary>
/// Generic Entity Framework Core repository implementation.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.FirstOrDefaultAsync(predicate);

    public virtual async Task AddAsync(T entity) =>
        await _dbSet.AddAsync(entity);

    public virtual void Update(T entity) =>
        _dbSet.Update(entity);

    public virtual void Remove(T entity) =>
        _dbSet.Remove(entity);

    public virtual void Delete(T entity) =>
        _dbSet.Remove(entity);

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate is null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);
    /// <summary>
    /// Gets entities by candidate ID. Override in specific repositories if needed.
    /// </summary>
    /// <param name="candidateId">The candidate ID.</param>
    /// <returns>Entities belonging to the candidate.</returns>
    /// <exception cref="NotSupportedException">Thrown if not overridden in a specific repository.</exception>
    public virtual Task<IEnumerable<T>> GetByCandidateAsync(int candidateId)
    {
        throw new NotSupportedException($"GetByCandidateAsync is not supported for {typeof(T).Name}. Override in a specific repository.");
    }
}