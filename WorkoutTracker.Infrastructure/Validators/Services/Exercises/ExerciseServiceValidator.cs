using WorkoutTracker.Infrastructure.Validators.Models.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Users;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Muscles;
using WorkoutTracker.Infrastructure.Validators.Models;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.Infrastructure.Validators.Services.Exercises;

public class ExerciseServiceValidator
{
    readonly UserValidator userValidator;
    readonly IExerciseRepository exerciseRepository;
    readonly ExerciseValidator exerciseValidator;
    readonly MuscleValidator muscleValidator;
    readonly EquipmentValidator equipmentValidator;
    readonly FileUploadModelValidator fileUploadModelValidator;

    public ExerciseServiceValidator(
        UserValidator userValidator,
        IExerciseRepository exerciseRepository,
        ExerciseValidator exerciseValidator,
        MuscleValidator muscleValidator,
        FileUploadModelValidator fileUploadModelValidator,
        EquipmentValidator equipmentValidator
    )
    {
        this.userValidator = userValidator;
        this.exerciseRepository = exerciseRepository;
        this.exerciseValidator = exerciseValidator;
        this.muscleValidator = muscleValidator;
        this.fileUploadModelValidator = fileUploadModelValidator;
        this.equipmentValidator = equipmentValidator;
    }

    #region Internal Exercises

    public async Task ValidateAddInternalAsync(Exercise exercise, CancellationToken cancellationToken)
    {
        await exerciseValidator.ValidateForAddAsync(exercise, cancellationToken);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("create", "internal exercise");
    }

    public async Task ValidateUpdateInternalAsync(Exercise exercise, CancellationToken cancellationToken)
    {
        var _exercise = await exerciseValidator.ValidateForEditAsync(exercise, cancellationToken);

        if (!string.IsNullOrEmpty(_exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal exercise");
    }

    public async Task ValidateDeleteInternalAsync(long exerciseId, CancellationToken cancellationToken)
    {
        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "internal exercise");
    }

    public async Task ValidateGetInternalByIdAsync(long exerciseId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, "Exercise");

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken);

        if (exercise != null && exercise.CreatedByUserId != null)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "internal exercise");
    }

    public async Task ValidateGetInternalByIdWithDetailsAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await ValidateGetInternalByIdAsync(exerciseId, cancellationToken);
    }

    public async Task ValidateGetInternalByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Exercise");

        var exercise = await exerciseRepository.GetByNameAsync(name, cancellationToken);

        if (exercise != null && exercise.CreatedByUserId != null)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "internal exercise");
    }

    public async Task ValidateGetInternalByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await ValidateGetInternalByNameAsync(name, cancellationToken);
    }

    public Task ValidateGetAllInternalAsync(ExerciseType? exerciseType, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task ValidateUpdateInternalMusclesAsync(long exerciseId, IEnumerable<long> muscleIds, CancellationToken cancellationToken)
    {
        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal exercise's muscles");

        foreach (var muscleId in muscleIds)
            await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);
    }

    public async Task ValidateUpdateInternalEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentIds, CancellationToken cancellationToken)
    {
        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal exercise's equipments");

        foreach (var equipmentId in equipmentIds)
            await equipmentValidator.EnsureExistsAsync(equipmentId, cancellationToken);
    }

    public async Task ValidateUpdateInternalAliasesAsync(long exerciseId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);

        if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal exercise aliases");

        foreach (var aliasStr in aliasesStr)
            ArgumentValidator.ThrowIfArgumentNullOrEmpty(aliasStr, nameof(aliasStr));
    }

    public async Task ValidateUpdateInternalPhotoAsync(long muscleId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);

        if (fileUpload != null)
            fileUploadModelValidator.Validate(fileUpload);
    }

    public async Task ValidateDeleteInternalPhotoAsync(long muscleId, CancellationToken cancellationToken)
    {
        await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);
    }

    #endregion

    #region User Exercises

    public async Task ValidateAddOwnedAsync(string userId, Exercise exercise, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await exerciseValidator.ValidateForAddAsync(exercise, cancellationToken);
    }

    public async Task ValidateUpdateOwnedAsync(string userId, Exercise exercise, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await exerciseValidator.ValidateForEditAsync(exercise, cancellationToken);

        var _exercise = (await exerciseRepository.GetByIdAsync(exercise.Id, cancellationToken))!;

        if (_exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user exercise");
    }

    public async Task ValidateDeleteOwnedAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);

        if (exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "user exercise");
    }

    public async Task ValidateGetOwnedByIdAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, "Exercise");

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken);

        if (exercise != null && exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "user exercise");
    }

    public async Task ValidateGetOwnedByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Exercise");

        var exercise = await exerciseRepository.GetByNameAsync(name, cancellationToken);

        if (exercise != null && exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "user exercise");
    }

    public async Task ValidateGetAllOwnedAsync(string userId, ExerciseType? exerciseType, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateUpdateOwnedMusclesAsync(string userId, long exerciseId, IEnumerable<long> muscleIds, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);

        if (exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user exercise's muscles");

        foreach (var muscleId in muscleIds)
            await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);
    }

    public async Task ValidateUpdateOwnedEquipmentsAsync(string userId, long exerciseId, IEnumerable<long> equipmentIds, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);

        if (exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user exercise's equipments");

        foreach (var equipmentId in equipmentIds)
            await equipmentValidator.EnsureExistsAsync(equipmentId, cancellationToken);
    }

    public async Task ValidateUpdateOwnedAliasesAsync(string userId, long exerciseId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var exercise = await exerciseValidator.EnsureExistsAsync(exerciseId, cancellationToken);

        if (exercise.CreatedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user exercise aliases");

        foreach (var aliasStr in aliasesStr)
            ArgumentValidator.ThrowIfArgumentNullOrEmpty(aliasStr, nameof(aliasStr));
    }

    public async Task ValidateUpdateOwnedPhotoAsync(string userId, long muscleId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);

        if (fileUpload != null)
            fileUploadModelValidator.Validate(fileUpload);
    }

    public async Task ValidateDeleteOwnedPhotoAsync(string userId, long muscleId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);
    }


    #endregion

    #region All Exercises

    public async Task ValidateGetByIdAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, "Exercise");

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken);

        if (exercise != null && (exercise.CreatedByUserId != userId && exercise.CreatedByUserId != null))
            throw UnauthorizedException.HaveNoPermissionToAction("get", "exercise");
    }

    public async Task ValidateGetByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Exercise");

        var exercise = await exerciseRepository.GetByNameAsync(name, cancellationToken);

        if (exercise != null && (exercise.CreatedByUserId != userId && exercise.CreatedByUserId != null))
            throw UnauthorizedException.HaveNoPermissionToAction("get", "exercise");
    }

    public async Task ValidateGetAllAsync(string userId, ExerciseType? exerciseType, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUsedAsync(string userId, ExerciseType? exerciseType, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    #endregion
}
