using System.Linq.Expressions;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Repositories;

public interface IBaseRepository<T> where T : class, IDbModel
{
    Task<T?> GetByIdAsync(long key);
    Task<IQueryable<T>> GetAllAsync();
    Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task RemoveAsync(long key);
    Task RemoveRangeAsync(IEnumerable<T> entities);
    Task<bool> ExistsAsync(long key);
    Task SaveChangesAsync();
}
