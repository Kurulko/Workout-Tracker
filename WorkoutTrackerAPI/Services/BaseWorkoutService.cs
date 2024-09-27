using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories;

namespace WorkoutTrackerAPI.Services;

public class BaseWorkoutService<TModel> : BaseService<TModel>
    where TModel : WorkoutModel
{
    protected readonly BaseWorkoutRepository<TModel> baseWorkoutRepository;
    public BaseWorkoutService(BaseWorkoutRepository<TModel> baseWorkoutRepository)
        => this.baseWorkoutRepository = baseWorkoutRepository;
}
