using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Base;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Base;

internal class DbModelRepository<T> : IDisposable, IBaseRepository<T>
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

    public virtual async Task<T> AddAsync(T model)
    {
        ArgumentValidator.ThrowIfIdNonZero(model.Id, entityName);

        await dbSet.AddAsync(model);
        await SaveChangesAsync();
        return model;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
            ArgumentValidator.ThrowIfIdNonZero(entity.Id, entityName);

        await dbSet.AddRangeAsync(entities);
        await SaveChangesAsync();
    }

    public virtual async Task RemoveAsync(long key)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        var model = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, key, entityName);

        dbSet.Remove(model);
        await SaveChangesAsync();
    }

    public virtual async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
            ArgumentValidator.ThrowIfIdNonPositive(entity.Id, entityName);

        dbSet.RemoveRange(entities);
        await SaveChangesAsync();
    }

    public virtual async Task<IQueryable<T>> GetAllAsync()
        => await Task.FromResult(dbSet);

    public virtual async Task<T?> GetByIdAsync(long key)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        var models = await GetAllAsync();
        return await models.SingleOrDefaultAsync(m => m.Id == key);
    }

    public virtual async Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> expression)
    {
        var models = await GetAllAsync();
        return models.Where(expression);
    }

    public virtual async Task<bool> ExistsAsync(long key)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await dbSet.AnyAsync(m => m.Id == key);
    }

    public virtual async Task UpdateAsync(T model)
    {
        ArgumentValidator.ThrowIfIdNonPositive(model.Id, entityName);

        dbSet.Update(model);
        await SaveChangesAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
       => await dbSet.AnyAsync(expression);

    public async Task<bool> AnyAsync()
        => await dbSet.AnyAsync();

    public virtual async Task SaveChangesAsync()
        => await db.SaveChangesAsync();

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