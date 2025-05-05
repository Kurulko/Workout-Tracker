using System.Linq.Expressions;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Base;

public interface IBaseRepository<T> where T : class, IDbModel
{
    Task<T?> GetByIdAsync(long key);
    Task<IQueryable<T>> GetAllAsync();
    Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task RemoveAsync(long key);
    Task RemoveRangeAsync(IEnumerable<T> entities);
    Task<bool> ExistsAsync(long key);
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    Task<bool> AnyAsync();
    Task SaveChangesAsync();
}
