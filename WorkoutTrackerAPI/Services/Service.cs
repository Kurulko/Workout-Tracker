using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;

namespace WorkoutTrackerAPI.Services;

public abstract class Service<TModel> : BaseService<TModel>
    where TModel : class, IDbModel
{
    protected readonly DbModelRepository<TModel> baseRepository;
    public Service(DbModelRepository<TModel> baseRepository)
        => this.baseRepository = baseRepository;
}
