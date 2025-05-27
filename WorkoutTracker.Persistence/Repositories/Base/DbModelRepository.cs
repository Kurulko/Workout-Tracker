using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Base;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Base;

internal abstract  class DbModelRepository<T> : IDisposable, IBaseRepository<T>
    where T : class, IDbModel
{
    protected readonly WorkoutDbContext db;
    protected readonly DbSet<T> dbSet;
    public DbModelRepository(WorkoutDbContext db)
    {
        this.db = db;
        dbSet = db.Set<T>();
    }

    protected readonly string entityName = typeof(T).Name;

    public virtual async Task<T> AddAsync(T model, CancellationToken cancellationToken = default)
    {
        ArgumentValidator.ThrowIfIdNonZero(model.Id, entityName);

        await dbSet.AddAsync(model, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return model;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
            ArgumentValidator.ThrowIfIdNonZero(entity.Id, entityName);

        await dbSet.AddRangeAsync(entities, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public virtual async Task RemoveAsync(long key, CancellationToken cancellationToken = default)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        var entity = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, key, entityName, cancellationToken);

        dbSet.Remove(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public virtual async Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
            ArgumentValidator.ThrowIfIdNonPositive(entity.Id, entityName);

        dbSet.RemoveRange(entities);
        await SaveChangesAsync(cancellationToken);
    }

    public virtual IQueryable<T> GetAll()
        => dbSet;

    public virtual async Task<T?> GetByIdAsync(long key, CancellationToken cancellationToken = default)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await GetAll().SingleOrDefaultAsync(m => m.Id == key);
    }

    public virtual IQueryable<T> Find(Expression<Func<T, bool>> expression)
        => GetAll().Where(expression);

    public virtual async Task<bool> ExistsAsync(long key, CancellationToken cancellationToken = default)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await dbSet.AnyAsync(m => m.Id == key, cancellationToken);
    }

    public virtual async Task UpdateAsync(T model, CancellationToken cancellationToken = default)
    {
        ArgumentValidator.ThrowIfIdNonPositive(model.Id, entityName);

        dbSet.Update(model);
        await SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdatePartialAsync(long id, Action<T> updateAction, CancellationToken cancellationToken = default)
    {
        var entity = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, id, entityName, cancellationToken);

        updateAction(entity);
        dbSet.Update(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
       => await dbSet.AnyAsync(expression, cancellationToken);

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => await dbSet.AnyAsync(cancellationToken);

    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await db.SaveChangesAsync(cancellationToken);

    bool disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
            db.Dispose();

        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}