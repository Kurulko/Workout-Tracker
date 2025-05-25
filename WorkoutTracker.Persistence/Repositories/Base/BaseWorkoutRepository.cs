using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Base;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Base;

internal class BaseWorkoutRepository<T> : DbModelRepository<T>, IBaseWorkoutRepository<T>
    where T : BaseWorkoutModel
{
    public BaseWorkoutRepository(WorkoutDbContext db) : base(db)
    {
    }

    public virtual async Task<T?> GetByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(BaseWorkoutModel.Name));

        var workoutModels = await GetAllAsync();
        return await workoutModels.SingleOrDefaultAsync(m => m.Name == name);
    }

    public virtual async Task<bool> ExistsByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(BaseWorkoutModel.Name));

        var workoutModels = await GetAllAsync();
        return await workoutModels.AnyAsync(m => m.Name == name);
    }
}
