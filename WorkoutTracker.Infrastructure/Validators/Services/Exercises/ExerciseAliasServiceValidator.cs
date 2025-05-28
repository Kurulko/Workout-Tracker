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

    public async Task ValidateAddAsync(long exerciseId, ExerciseAlias exerciseAlias, CancellationToken cancellationToken)
    {
        await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);
        await exerciseAliasValidator.ValidateForAddAsync(exerciseAlias, cancellationToken);
    }

    public async Task ValidateUpdateAsync(ExerciseAlias exerciseAlias, CancellationToken cancellationToken)
    {
        await exerciseAliasValidator.ValidateForEditAsync(exerciseAlias, cancellationToken);
        await exerciseValidator.EnsureExistsAsync(exerciseAlias.ExerciseId, cancellationToken);
    }

    public async Task ValidateDeleteAsync(long exerciseAliasId, CancellationToken cancellationToken)
    {
        await exerciseAliasValidator.EnsureExistsAsync(exerciseAliasId, cancellationToken);
    }

    public Task ValidateGetByIdAsync(long exerciseAliasId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseAliasId, "Exercise alias");

        return Task.CompletedTask;
    }

    public Task ValidateGetByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(ExerciseAlias.Name));

        return Task.CompletedTask;
    }

    public Task ValidateGetAllAsync(long exerciseId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, "Exercise");

        return Task.CompletedTask;
    }
}
