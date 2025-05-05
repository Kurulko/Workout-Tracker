using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Infrastructure.Services.Base;

internal class BaseWorkoutService<TModel> : BaseService<TModel>
    where TModel : BaseWorkoutModel
{
    protected readonly IBaseWorkoutRepository<TModel> baseWorkoutRepository;
    public BaseWorkoutService(IBaseWorkoutRepository<TModel> baseWorkoutRepository)
        => this.baseWorkoutRepository = baseWorkoutRepository;

    protected ArgumentException EntryNameMustBeUnique(string entryName)
        => new ($"{entryName} name must be unique.");
}
