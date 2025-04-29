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
    {
        var workoutModels = await GetAllAsync();
        return await workoutModels.SingleOrDefaultAsync(m => m.Name == name);
    }

    public virtual async Task<bool> ExistsByNameAsync(string name)
    {
        var workoutModels = await GetAllAsync();
        return await workoutModels.AnyAsync(m => m.Name == name);
    }
}
