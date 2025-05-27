using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Base;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Base;

internal abstract class BaseWorkoutRepository<T> : DbModelRepository<T>, IBaseWorkoutRepository<T>
    where T : BaseWorkoutModel
{
    public BaseWorkoutRepository(WorkoutDbContext db) : base(db)
    {
    }

    public virtual async Task<T?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(BaseWorkoutModel.Name));

        return await GetAll().SingleOrDefaultAsync(m => m.Name == name, cancellationToken);
    }

    public virtual async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(BaseWorkoutModel.Name));

        return await dbSet.AnyAsync(m => m.Name == name, cancellationToken);
    }
}
