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

    public async Task<ExerciseAlias> AddExerciseAliasToExerciseAsync(long exerciseId, ExerciseAlias exerciseAlias)
    {
        await exerciseAliasServiceValidator.ValidateAddAsync(exerciseId, exerciseAlias);

        exerciseAlias.ExerciseId = exerciseId;
        return await exerciseAliasRepository.AddAsync(exerciseAlias)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "add"));
    }

    public async Task DeleteExerciseAliasAsync(long id)
    {
        await exerciseAliasServiceValidator.ValidateDeleteAsync(id);

        await exerciseAliasRepository.RemoveAsync(id)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "delete"));
    }

    public async Task<ExerciseAlias?> GetExerciseAliasByIdAsync(long id)
    {
        await exerciseAliasServiceValidator.ValidateGetByIdAsync(id);

        return await exerciseAliasRepository.GetByIdAsync(id)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "get"));
    }

    public async Task<ExerciseAlias?> GetExerciseAliasByNameAsync(string name)
    {
        await exerciseAliasServiceValidator.ValidateGetByNameAsync(name);

        return await exerciseAliasRepository.GetByNameAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "get"));
    }

    public async Task<IQueryable<ExerciseAlias>> GetExerciseAliasesByExerciseIdAsync(long exerciseId)
    {
        await exerciseAliasServiceValidator.ValidateGetAllAsync(exerciseId);

        return await exerciseAliasRepository.FindAsync(er => er.ExerciseId == exerciseId)
            .LogExceptionsAsync(_logger, FailedToActionStr("exercise aliases", "get"));
    }

    public async Task UpdateExerciseAliasAsync(ExerciseAlias exerciseAlias)
    {
        await exerciseAliasServiceValidator.ValidateUpdateAsync(exerciseAlias);

        var _exerciseAlias = (await exerciseAliasRepository.GetByIdAsync(exerciseAlias.Id))!;

        _exerciseAlias.Name = exerciseAlias.Name;
        _exerciseAlias.ExerciseId = exerciseAlias.ExerciseId;

        await exerciseAliasRepository.UpdateAsync(exerciseAlias)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseAliasEntityName, "update"));
    }
}
