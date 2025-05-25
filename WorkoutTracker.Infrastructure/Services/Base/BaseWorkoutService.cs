using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Infrastructure.Services.Base;

internal class BaseWorkoutService<TService, TModel> : BaseService<TService, TModel>
    where TModel : BaseWorkoutModel
    where TService : IBaseService
{
    protected readonly IBaseWorkoutRepository<TModel> baseWorkoutRepository;
    public BaseWorkoutService(IBaseWorkoutRepository<TModel> baseWorkoutRepository, ILogger<TService> logger) : base(logger)
        => this.baseWorkoutRepository = baseWorkoutRepository;

    protected ValidationException EntryNameMustBeUnique(string entryName)
        => new ValidationException($"{entryName} name must be unique.");
}
