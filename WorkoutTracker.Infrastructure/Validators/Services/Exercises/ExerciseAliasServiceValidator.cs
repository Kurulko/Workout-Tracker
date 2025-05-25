using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Exercises;

namespace WorkoutTracker.Infrastructure.Validators.Services.Exercises;

public class ExerciseAliasServiceValidator
{
    readonly ExerciseAliasValidator exerciseAliasValidator;
    readonly ExerciseValidator exerciseValidator;

    public ExerciseAliasServiceValidator(
        ExerciseAliasValidator exerciseAliasValidator,
        ExerciseValidator exerciseValidator)
    {
        this.exerciseAliasValidator = exerciseAliasValidator;
        this.exerciseValidator = exerciseValidator;
    }

    public async Task ValidateAddAsync(long exerciseId, ExerciseAlias exerciseAlias)
    {
        await exerciseValidator.EnsureExistsAsync(exerciseId);
        await exerciseAliasValidator.ValidateForAddAsync(exerciseAlias);
    }

    public async Task ValidateUpdateAsync(ExerciseAlias exerciseAlias)
    {
        await exerciseAliasValidator.ValidateForEditAsync(exerciseAlias);
        await exerciseValidator.EnsureExistsAsync(exerciseAlias.ExerciseId);
    }

    public async Task ValidateDeleteAsync(long exerciseAliasId)
    {
        await exerciseAliasValidator.EnsureExistsAsync(exerciseAliasId);
    }

    public Task ValidateGetByIdAsync(long exerciseAliasId)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseAliasId, "Exercise alias");

        return Task.CompletedTask;
    }

    public Task ValidateGetByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(ExerciseAlias.Name));

        return Task.CompletedTask;
    }

    public Task ValidateGetAllAsync(long exerciseId)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, "Exercise");

        return Task.CompletedTask;
    }
}
