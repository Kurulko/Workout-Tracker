using System.Linq.Expressions;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Base;

public interface IBaseRepository<T> where T : class, IDbModel
{
    Task<T?> GetByIdAsync(long key, CancellationToken cancellationToken = default);

    IQueryable<T> Find(Expression<Func<T, bool>> expression);
    IQueryable<T> GetAll();

    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdatePartialAsync(long id, Action<T> updateAction, CancellationToken cancellationToken = default);

    Task RemoveAsync(long key, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(long key, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
