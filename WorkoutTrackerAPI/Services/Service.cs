using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;

namespace WorkoutTrackerAPI.Services;

public abstract class Service<TModel> : BaseService<TModel>
    where TModel : class, IDbModel
{
    protected readonly BaseRepository<TModel> baseRepository;
    public Service(BaseRepository<TModel> baseRepository)
        => this.baseRepository = baseRepository;
}
