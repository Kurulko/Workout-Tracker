using WorkoutTracker.Infrastructure.Validators.Models.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Users;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Muscles;
using WorkoutTracker.Infrastructure.Validators.Models;

namespace WorkoutTracker.Infrastructure.Validators.Services.Exercises;

public class ExerciseServiceValidator
{
    readonly UserValidator userValidator;
    readonly IExerciseRepository exerciseRepository;
    readonly ExerciseValidator exerciseValidator;
    readonly MuscleValidator muscleValidator;
    readonly EquipmentValidator equipmentValidator;

    public ExerciseServiceValidator(
        UserValidator userValidator,
        IExerciseRepository exerciseRepository,
        ExerciseValidator exerciseValidator,
        MuscleValidator muscleValidator,
        EquipmentValidator equipmentValidator
    )
    {
        this.userValidator = userValidator;
        this.exerciseRepository = exerciseRepository;
        this.exerciseValidator = exerciseValidator;
        this.muscleValidator = muscleValidator;
        this.equipmentValidator = equipmentValidator;
    }

    #region Internal Exercises

    public async Task ValidateAddInternalAsync(Exercise exercise)
    {
        await exerciseValidator.ValidateForAddAsync(exercise);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("create", "internal exercise");
    }

    public async Task ValidateUpdateInternalAsync(Exercise exercise)
    {
        var _exercise = await exerciseValidator.ValidateForEditAsync(exercise);

        if (!string.IsNullOrEmpty(_exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal exercise");
    }

    public async Task ValidateDeleteInternalAsync(long exerciseId)
    {
        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "internal exercise");
    }

    public async Task ValidateGetInternalByIdAsync(string userId, long exerciseId, bool withDetails)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, "Exercise");

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId);

        if (exercise != null && exercise.CreatedByUserId != null)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "internal exercise");

        if (withDetails)
            await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetInternalByNameAsync(string userId, string name, bool withDetails)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Exercise");

        var exercise = await exerciseRepository.GetByNameAsync(name);

        if (exercise != null && exercise.CreatedByUserId != null)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "internal exercise");

        if (withDetails)
            await userValidator.EnsureExistsAsync(userId);
    }

    public Task ValidateGetAllInternalAsync(ExerciseType? exerciseType = null)
    {
        return Task.CompletedTask;
    }

    public async Task ValidateUpdateInternalMusclesAsync(long exerciseId, IEnumerable<long> muscleIds)
    {
        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal exercise's muscles");

        foreach (var muscleId in muscleIds)
            await muscleValidator.EnsureExistsAsync(muscleId);
    }

    public async Task ValidateUpdateInternalEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentIds)
    {
        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal exercise's equipments");

        foreach (var equipmentId in equipmentIds)
            await equipmentValidator.EnsureExistsAsync(equipmentId);
    }

    #endregion

    #region User Exercises

    public async Task ValidateAddOwnedAsync(string userId, Exercise exercise)
    {
        await userValidator.EnsureExistsAsync(userId);
        await exerciseValidator.ValidateForAddAsync(exercise);
    }

    public async Task ValidateUpdateOwnedAsync(string userId, Exercise exercise)
    {
        await userValidator.EnsureExistsAsync(userId);
        await exerciseValidator.ValidateForEditAsync(exercise);

        var _exercise = (await exerciseRepository.GetByIdAsync(exercise.Id))!;

        if (_exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user exercise");
    }

    public async Task ValidateDeleteOwnedAsync(string userId, long exerciseId)
    {
        await userValidator.EnsureExistsAsync(userId);

        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId);

        if (exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "user exercise");
    }

    public async Task ValidateGetOwnedByIdAsync(string userId, long exerciseId)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, "Exercise");

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId);

        if (exercise != null && exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "user exercise");
    }

    public async Task ValidateGetOwnedByNameAsync(string userId, string name)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Exercise");

        var exercise = await exerciseRepository.GetByNameAsync(name);

        if (exercise != null && exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "user exercise");
    }

    public async Task ValidateGetAllOwnedAsync(string userId, ExerciseType? exerciseType = null)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateUpdateOwnedMusclesAsync(string userId, long exerciseId, IEnumerable<long> muscleIds)
    {
        await userValidator.EnsureExistsAsync(userId);

        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId);

        if (exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user exercise's muscles");

        foreach (var muscleId in muscleIds)
            await muscleValidator.EnsureExistsAsync(muscleId);
    }

    public async Task ValidateUpdateOwnedEquipmentsAsync(string userId, long exerciseId, IEnumerable<long> equipmentIds)
    {
        await userValidator.EnsureExistsAsync(userId);

        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId);

        if (exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user exercise's equipments");

        foreach (var equipmentId in equipmentIds)
            await equipmentValidator.EnsureExistsAsync(equipmentId);
    }


    #endregion

    #region All Exercises

    public async Task ValidateGetByIdAsync(string userId, long exerciseId)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, "Exercise");

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId);

        if (exercise != null && (exercise.CreatedByUserId != userId && exercise.CreatedByUserId != null))
            throw UnauthorizedException.HaveNoPermissionToAction("get", "exercise");
    }

    public async Task ValidateGetByNameAsync(string userId, string name)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Exercise");

        var exercise = await exerciseRepository.GetByNameAsync(name);

        if (exercise != null && (exercise.CreatedByUserId != userId && exercise.CreatedByUserId != null))
            throw UnauthorizedException.HaveNoPermissionToAction("get", "exercise");
    }

    public async Task ValidateGetAllAsync(string userId, ExerciseType? exerciseType = null)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUsedAsync(string userId, ExerciseType? exerciseType = null)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    #endregion
}
