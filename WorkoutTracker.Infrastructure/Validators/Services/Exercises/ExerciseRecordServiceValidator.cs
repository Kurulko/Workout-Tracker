using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Extensions.Exercises;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Infrastructure.Validators.Models.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Exercises.ExerciseRecords;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services.Exercises;

public class ExerciseRecordServiceValidator
{
    readonly UserValidator userValidator;
    readonly IExerciseRecordGroupRepository exerciseRecordGroupRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly ExerciseRecordValidator exerciseRecordValidator;
    readonly ExerciseValidator exerciseValidator;

    public ExerciseRecordServiceValidator(
        UserValidator userValidator,
        IExerciseRecordGroupRepository exerciseRecordGroupRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        ExerciseRecordValidator exerciseRecordValidator,
        ExerciseValidator exerciseValidator)
    {
        this.userValidator = userValidator;
        this.exerciseRecordGroupRepository = exerciseRecordGroupRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.exerciseRecordValidator = exerciseRecordValidator;
        this.exerciseValidator = exerciseValidator;
    }

    public async Task ValidateAddAsync(long exerciseRecordGroupId, string userId, ExerciseRecord exerciseRecord, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var exerciseRecordGroup = await ArgumentValidator.EnsureExistsByIdAsync(
            exerciseRecordGroupRepository.GetByIdAsync,
            exerciseRecordGroupId,
            "Exercise record group",
            cancellationToken
        );

        if (exerciseRecordGroup.GetUserId() != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("add", "exercise record");

        await exerciseRecordValidator.ValidateForAddAsync(exerciseRecord, cancellationToken);
        
        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseRecord.ExerciseId, cancellationToken);

        if (exercise.CreatedByUserId != null && exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("add", "exercise record");

    }

    public async Task ValidateUpdateAsync(string userId, ExerciseRecord exerciseRecord, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        var _exerciseRecord = await exerciseRecordValidator.ValidateForEditAsync(exerciseRecord, cancellationToken);

        if (_exerciseRecord.GetUserId() != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "exercise record");

        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseRecord.ExerciseId, cancellationToken);

        if (exercise.CreatedByUserId != null && exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "exercise record");
    }

    public async Task ValidateDeleteAsync(string userId, long exerciseRecordId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await exerciseRecordValidator.EnsureExistsAsync(exerciseRecordId, cancellationToken);

        var ownerUserId = await ArgumentValidator.EnsureExistsByIdAsync(
            exerciseRecordRepository.GetUserIdByExerciseRecordIdAsync,
            exerciseRecordId,
            "Exercise record",
            cancellationToken
        );

        if (ownerUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "exercise record");
    }

    public async Task ValidateGetAsync(string userId, long exerciseRecordId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(exerciseRecordId, "Exercise record");

        var ownerUserId = await exerciseRecordRepository.GetUserIdByExerciseRecordIdAsync(exerciseRecordId, cancellationToken);

        if (ownerUserId != null && ownerUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "exercise record");
    }

    public async Task ValidateGetAllAsync(string userId, long? exerciseId, ExerciseType? exerciseType, DateTimeRange? range, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        if (exerciseId.HasValue)
        {
            var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId.Value, cancellationToken);

            if (exercise.CreatedByUserId != null && exercise.CreatedByUserId != userId)
                throw UnauthorizedException.HaveNoPermissionToAction("get", "exercise records");
        }

        if (range is not null)
            ArgumentValidator.ThrowIfRangeInFuture(range, nameof(range));
    }
}
