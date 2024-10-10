using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;

namespace WorkoutTrackerAPI.Services;

public abstract class DbModelService<TModel> : BaseService<TModel>
    where TModel : class, IDbModel
{
    protected readonly IBaseRepository<TModel> baseRepository;
    public DbModelService(IBaseRepository<TModel> baseRepository)
        => this.baseRepository = baseRepository;
}
