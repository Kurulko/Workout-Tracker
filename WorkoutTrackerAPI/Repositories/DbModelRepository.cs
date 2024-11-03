﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WorkoutTrackerAPI.Repositories;

public class DbModelRepository<T> : IDisposable, IBaseRepository<T>
    where T : class, IDbModel
{
    protected readonly WorkoutDbContext db;
    protected readonly DbSet<T> dbSet;
    public DbModelRepository(WorkoutDbContext db)
    {
        this.db = db;
        dbSet = db.Set<T>();
    }

    protected IQueryable<T> DbSetAsNoTracking => dbSet.AsNoTracking();

    public virtual async Task<T> AddAsync(T model)
    {
        if (model.Id != 0)
            throw new DbUpdateException($"Entity of type {typeof(T).Name} should not have an ID assigned.");

        T? existingModel = await GetByIdAsync(model.Id);

        if (existingModel is null)
        {
            await dbSet.AddAsync(model);
            await SaveChangesAsync();
            return model;
        }

        return existingModel;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        if (entities.Any(e => e.Id != 0))
            throw new DbUpdateException($"New entities of type {typeof(T).Name} should not have an ID assigned.");

        await dbSet.AddRangeAsync(entities);
        await SaveChangesAsync();
    }

    public virtual async Task RemoveAsync(long key)
    {
        if (key <= 0)
            throw new DbUpdateException($"Entity of type {typeof(T).Name} must have a positive ID to be removed.");

        T? model = await GetByIdAsync(key) ?? throw new NotFoundException(typeof(T).Name);

        dbSet.Remove(model);
        await SaveChangesAsync();
    }

    public virtual async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        if (entities.Any(e => e.Id <= 0))
            throw new DbUpdateException($"Entities of type {typeof(T).Name} must have a positive ID to be removed.");

        dbSet.RemoveRange(entities);
        await SaveChangesAsync();
    }

    public virtual async Task<IQueryable<T>> GetAllAsync()
        => await Task.FromResult(DbSetAsNoTracking);

    public virtual async Task<T?> GetByIdAsync(long key)
        => await DbSetAsNoTracking.SingleOrDefaultAsync(m => m.Id == key);

    public virtual async Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> expression)
        => await Task.FromResult(DbSetAsNoTracking.Where(expression));

    public virtual async Task<bool> ExistsAsync(long key)
        => await dbSet.AnyAsync(m => m.Id == key);

    public virtual async Task UpdateAsync(T model)
    {
        if (model.Id < 0)
            throw new DbUpdateException($"Modified entities of type {typeof(T).Name} must have a positive ID.");

        dbSet.Update(model);
        await SaveChangesAsync();
    }

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