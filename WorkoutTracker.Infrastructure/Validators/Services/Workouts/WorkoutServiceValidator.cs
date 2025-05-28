using System.Xml.Linq;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Validators.Models.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Exercises.ExerciseSets;
using WorkoutTracker.Infrastructure.Validators.Models.Users;
using WorkoutTracker.Infrastructure.Validators.Models.Workouts;

namespace WorkoutTracker.Infrastructure.Validators.Services.Workouts;

public class WorkoutServiceValidator
{
    readonly UserValidator userValidator;
    readonly WorkoutValidator workoutValidator;
    readonly ExerciseValidator exerciseValidator;
    readonly ExerciseSetGroupValidator exerciseSetGroupValidator;
    readonly ExerciseSetValidator exerciseSetValidator;
    readonly IWorkoutRepository workoutRepository;

    public WorkoutServiceValidator(
        UserValidator userValidator,
        WorkoutValidator workoutValidator,
        ExerciseValidator exerciseValidator,
        ExerciseSetGroupValidator exerciseSetGroupValidator,
        ExerciseSetValidator exerciseSetValidator,
        IWorkoutRepository workoutRepository
    )
    {
        this.userValidator = userValidator;
        this.workoutValidator = workoutValidator;
        this.exerciseValidator = exerciseValidator;
        this.exerciseSetGroupValidator = exerciseSetGroupValidator;
        this.exerciseSetValidator = exerciseSetValidator;
        this.workoutRepository = workoutRepository;
    }

    public async Task ValidateAddAsync(string userId, Workout workout, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await workoutValidator.ValidateForAddAsync(workout, cancellationToken);
    }

    public async Task ValidateUpdateAsync(string userId, Workout workout, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        var _workout = await workoutValidator.ValidateForEditAsync(workout, cancellationToken);

        if (_workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "workout");
    }

    public async Task ValidateDeleteAsync(string userId, long workoutId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        var workout = await workoutValidator.EnsureExistsAsync(workoutId, cancellationToken);

        if (workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "workout");
    }

    public async Task ValidateGetByIdAsync(string userId, long workoutId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(workoutId, "Workout");

        var workout = await workoutRepository.GetByIdAsync(workoutId, cancellationToken);

        if (workout != null && workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "workout");
    }

    public async Task ValidateGetByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Exercise");

        var workout = await workoutRepository.GetByNameAsync(name, cancellationToken);

        if (workout != null && workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "workout");
    }

    public async Task ValidateGetAllAsync(string userId, long? exerciseId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        if (exerciseId.HasValue)
        {
            var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId.Value, cancellationToken);

            if (exercise.CreatedByUserId != null && exercise.CreatedByUserId != userId)
                throw UnauthorizedException.HaveNoPermissionToAction("get", "workouts");
        }
    }

    public async Task ValidateAddExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var workout = await workoutValidator.EnsureExistsAsync(workoutId, cancellationToken);

        if (workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("add", "workout exercise set groups");

        foreach (var exerciseSetGroup in exerciseSetGroups)
        {
            await exerciseSetGroupValidator.ValidateForAddAsync(exerciseSetGroup, cancellationToken);

            foreach (var exerciseSet in exerciseSetGroup.ExerciseSets)
                await exerciseSetValidator.ValidateForAddAsync(exerciseSet, cancellationToken);
        }
    }

    public async Task ValidateUpdateExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var workout = await workoutValidator.EnsureExistsAsync(workoutId, cancellationToken);

        if (workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "workout exercise set groups");

        foreach (var exerciseSetGroup in exerciseSetGroups)
        {
            await exerciseSetGroupValidator.ValidateForAddAsync(exerciseSetGroup, cancellationToken);

            foreach (var exerciseSet in exerciseSetGroup.ExerciseSets)
                await exerciseSetValidator.ValidateForAddAsync(exerciseSet, cancellationToken);
        }
    }
    
    public async Task ValidateCompleteAsync(string userId, long workoutId, DateTime date, TimeSpan time, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var workout = await workoutValidator.EnsureExistsAsync(workoutId, cancellationToken);

        if (workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("complete", "workout");

        ArgumentValidator.ThrowIfDateInFuture(date, nameof(date));
    } 
    
    public async Task ValidateUpdatePinnedAsync(string userId, long workoutId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var workout = await workoutValidator.EnsureExistsAsync(workoutId, cancellationToken);

        if (workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("complete", "workout");
    }
}
