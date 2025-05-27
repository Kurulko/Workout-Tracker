using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Infrastructure.Validators.Services.Exercises;

namespace WorkoutTracker.Infrastructure.Services.Exercises;

internal class ExerciseAliasService : BaseWorkoutService<ExerciseAliasService, ExerciseAlias>, IExerciseAliasService
{
    readonly IExerciseAliasRepository exerciseAliasRepository;
    readonly ExerciseAliasServiceValidator exerciseAliasServiceValidator;

    public ExerciseAliasService(
        IExerciseAliasRepository exerciseAliasRepository,
        ExerciseAliasServiceValidator exerciseAliasServiceValidator,
        ILogger<ExerciseAliasService> logger
    ) : base(exerciseAliasRepository, logger)
    {
        this.exerciseAliasRepository = exerciseAliasRepository;
        this.exerciseAliasServiceValidator = exerciseAliasServiceValidator;
    }

    const string exerciseAliasEntityName = "exercise alias";

    public async Task<ExerciseAlias> AddExerciseAliasToExerciseAsync(long exerciseId, ExerciseAlias exerciseAlias, CancellationToken cancellationToken)
    {
        await exerciseAliasServiceValidator.ValidateAddAsync(exerciseId, exerciseAlias, cancellationToken);

        exerciseAlias.ExerciseId = exerciseId;

        return await exerciseAliasRepository.AddAsync(exerciseAlias, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "add"));
    }

    public async Task DeleteExerciseAliasAsync(long id, CancellationToken cancellationToken)
    {
        await exerciseAliasServiceValidator.ValidateDeleteAsync(id, cancellationToken);

        await exerciseAliasRepository.RemoveAsync(id, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "delete"));
    }

    public async Task<ExerciseAlias?> GetExerciseAliasByIdAsync(long id, CancellationToken cancellationToken)
    {
        await exerciseAliasServiceValidator.ValidateGetByIdAsync(id, cancellationToken);

        return await exerciseAliasRepository.GetByIdAsync(id, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "get"));
    }

    public async Task<ExerciseAlias?> GetExerciseAliasByNameAsync(string name, CancellationToken cancellationToken)
    {
        await exerciseAliasServiceValidator.ValidateGetByNameAsync(name, cancellationToken);

        return await exerciseAliasRepository.GetByNameAsync(name, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "get"));
    }

    public async Task<IEnumerable<ExerciseAlias>> GetExerciseAliasesByExerciseIdAsync(long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseAliasServiceValidator.ValidateGetAllAsync(exerciseId, cancellationToken);

        var exerciseAliases = exerciseAliasRepository.GetExerciseAliasesByExerciseId(exerciseId);

        return await exerciseAliases.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("exercise aliases", "get"));
    }

    public async Task UpdateExerciseAliasAsync(ExerciseAlias exerciseAlias, CancellationToken cancellationToken)
    {
        await exerciseAliasServiceValidator.ValidateUpdateAsync(exerciseAlias, cancellationToken);

        var updateAction = new Action<ExerciseAlias>(e =>
        {
            e.Name = exerciseAlias.Name;
            e.ExerciseId = exerciseAlias.ExerciseId;
        });

        await exerciseAliasRepository.UpdatePartialAsync(exerciseAlias.Id, updateAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "update"));
    }
}
