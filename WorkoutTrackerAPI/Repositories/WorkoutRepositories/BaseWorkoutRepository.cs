using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WorkoutTrackerAPI.Repositories;

public class BaseWorkoutRepository<T> : DbModelRepository<T>, IBaseWorkoutRepository<T>
    where T : WorkoutModel
{
    public BaseWorkoutRepository(WorkoutDbContext db) : base(db)
    {
    }
    public virtual async Task<T?> GetByNameAsync(string name)
        => await DbSetAsNoTracking.SingleOrDefaultAsync(m => m.Name == name); 

    public virtual async Task<bool> ExistsByNameAsync(string name)
        => await dbSet.AnyAsync(m => m.Name == name);
}
