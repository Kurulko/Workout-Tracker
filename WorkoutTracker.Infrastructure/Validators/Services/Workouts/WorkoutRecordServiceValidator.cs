using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Validators.Models.Users;
using WorkoutTracker.Infrastructure.Validators.Models.Workouts;

namespace WorkoutTracker.Infrastructure.Validators.Services.Workouts;

public class WorkoutRecordServiceValidator
{
    readonly UserValidator userValidator;
    readonly WorkoutValidator workoutValidator;
    readonly WorkoutRecordValidator workoutRecordValidator;
    readonly IWorkoutRecordRepository workoutRecordRepository;

    public WorkoutRecordServiceValidator(
        IWorkoutRecordRepository workoutRecordRepository,
        UserValidator userValidator,
        WorkoutValidator workoutValidator,
        WorkoutRecordValidator workoutRecordValidator
    )
    {
        this.userValidator = userValidator;
        this.workoutValidator = workoutValidator;
        this.workoutRecordValidator = workoutRecordValidator;
        this.workoutRecordRepository = workoutRecordRepository;
    }

    public async Task ValidateAddAsync(string userId, WorkoutRecord workoutRecord, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await workoutRecordValidator.ValidateForAddAsync(workoutRecord, cancellationToken);

        var workout = await workoutValidator.EnsureExistsAsync(workoutRecord.WorkoutId, cancellationToken);

        if (workout.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("add", "workout record");
    }

    public async Task ValidateUpdateAsync(string userId, WorkoutRecord workoutRecord, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var _workoutRecord = await workoutRecordValidator.ValidateForEditAsync(workoutRecord, cancellationToken);

        if (_workoutRecord.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "workout record");

        await workoutValidator.EnsureExistsAsync(workoutRecord.WorkoutId, cancellationToken);
    }

    public async Task ValidateDeleteAsync(string userId, long workoutRecordId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var workoutRecord = await workoutRecordValidator.EnsureExistsAsync(workoutRecordId, cancellationToken);

        if (workoutRecord.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "workout record");
    }

    public async Task ValidateGetByIdAsync(string userId, long workoutRecordId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(workoutRecordId, "Workout Record");

        var workoutRecord = await workoutRecordRepository.GetByIdAsync(workoutRecordId, cancellationToken);

        if (workoutRecord != null && workoutRecord.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "workout record");
    }

    public async Task ValidateGetAllAsync(string userId, long? workoutId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        if (workoutId.HasValue)
        {
            var workout = await workoutValidator.EnsureExistsAsync(workoutId.Value, cancellationToken);

            if (workout.UserId != userId)
                throw UnauthorizedException.HaveNoPermissionToAction("get", "workout records");
        }

        if (range is not null)
            ArgumentValidator.ThrowIfRangeInFuture(range, nameof(range));
    }
}
