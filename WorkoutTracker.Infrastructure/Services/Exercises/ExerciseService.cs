using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Infrastructure.Validators.Services.Exercises;
using WorkoutTracker.Infrastructure.Validators.Services.Muscles;

namespace WorkoutTracker.Infrastructure.Services.Exercises;

internal class ExerciseService : BaseWorkoutService<ExerciseService, Exercise>, IExerciseService
{
    readonly IExerciseRepository exerciseRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly IExerciseAliasRepository exerciseAliasRepository;
    readonly ExerciseServiceValidator exerciseServiceValidator;
    readonly IEquipmentRepository equipmentRepository;
    readonly IMuscleRepository muscleRepository;
    readonly IFileService fileService;
    public ExerciseService (
        IExerciseRepository exerciseRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IExerciseAliasRepository exerciseAliasRepository,
        IEquipmentRepository equipmentRepository,
        IMuscleRepository muscleRepository,
        IFileService fileService,
        ExerciseServiceValidator exerciseServiceValidator,
        ILogger<ExerciseService> logger
    ) : base(exerciseRepository, logger)
    {
        this.exerciseRepository = exerciseRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.exerciseAliasRepository = exerciseAliasRepository;
        this.equipmentRepository = equipmentRepository;
        this.muscleRepository = muscleRepository;
        this.fileService = fileService;
        this.exerciseServiceValidator = exerciseServiceValidator;
    }

    readonly string exercisePhotosDirectory = Path.Combine("images", "exercises");
    const int maxExerciseImageSizeInMB = 3;


    #region Internal Exercises

    const string internalExerciseEntityName = "internal exercise";

    public async Task<Exercise> AddInternalExerciseAsync(Exercise exercise, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateAddInternalAsync(exercise, cancellationToken);

        return await exerciseRepository.AddAsync(exercise, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "add"));
    }

    public async Task DeleteInternalExerciseAsync(long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateDeleteInternalAsync(exerciseId, cancellationToken);

        var exercise = (await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken))!;
        string? exerciseImage = exercise.Image;

        await exerciseRepository.RemoveAsync(exerciseId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "delete"));

        if (!string.IsNullOrEmpty(exerciseImage))
            fileService.DeleteFile(exerciseImage);
    }

    public async Task<Exercise?> GetInternalExerciseByIdAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetInternalByIdAsync(exerciseId, cancellationToken);

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetInternalExerciseByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetInternalByNameAsync(name, cancellationToken);

        var exercise = await exerciseRepository.GetByNameAsync(name, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetInternalExerciseByIdWithDetailsAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetInternalByIdWithDetailsAsync(userId, exerciseId, cancellationToken);

        var exercise = await exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetInternalExerciseByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetInternalByNameWithDetailsAsync(userId, name, cancellationToken);

        var exercise = await exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<IEnumerable<Exercise>> GetInternalExercisesAsync(ExerciseType? exerciseType, string? filterQuery, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetAllInternalAsync(exerciseType, cancellationToken);

        var exercises = exerciseRepository.GetInternalExercises(exerciseType, filterQuery);

        return await exercises.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("internal exercises", "get"));
    }

    public async Task UpdateInternalExerciseAsync(Exercise exercise, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateInternalAsync(exercise, cancellationToken);

        await exerciseRepository.UpdatePartialAsync(exercise.Id, ExerciseUpdateAction(exercise), cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "update"));
    }


    public async Task UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateInternalMusclesAsync(exerciseId, muscleIds, cancellationToken);

        var updateWorkingMusclesAction = new Action<Exercise>(e =>
        {
            e.WorkingMuscles = [..muscleRepository.FindByIds(muscleIds)];
        });

        await exerciseRepository.UpdatePartialAsync(exerciseId, updateWorkingMusclesAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{internalExerciseEntityName}'s muscles", "update"));
    }

    public async Task UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentIds, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateInternalEquipmentsAsync(exerciseId, equipmentIds, cancellationToken);

        var updateEquipmentsAction = new Action<Exercise>(e =>
        {
            e.Equipments = [..equipmentRepository.FindInternalByIds(equipmentIds)];
        });

        await exerciseRepository.UpdatePartialAsync(exerciseId, updateEquipmentsAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{internalExerciseEntityName}'s equipments", "update"));
    }

    public async Task UpdateInternalExerciseAliasesAsync(long exerciseId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateInternalAliasesAsync(exerciseId, aliasesStr, cancellationToken);

        await exerciseAliasRepository.RemoveByExerciseIdAsync(exerciseId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{internalExerciseEntityName} aliases", "delete"));

        await exerciseAliasRepository.AddExerciseAliasesAsync(exerciseId, aliasesStr, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{internalExerciseEntityName} aliases", "add"));
    }

    public async Task UpdateInternalExercisePhotoAsync(long exerciseId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateInternalPhotoAsync(exerciseId, fileUpload, cancellationToken);

        string? image = await fileService.GetImageAsync(fileUpload, exercisePhotosDirectory, maxExerciseImageSizeInMB, false);
        var oldImage = await exerciseRepository.GetExercisePhotoAsync(exerciseId, cancellationToken);

        await (!string.IsNullOrEmpty(image) ?
            exerciseRepository.UpdateExercisePhotoAsync(exerciseId, image!, cancellationToken) :
            exerciseRepository.DeleteExercisePhotoAsync(exerciseId, cancellationToken)
        ).LogExceptionsAsync(_logger, FailedToActionStr($"{internalExerciseEntityName} photo", "update"));

        if (!string.IsNullOrEmpty(oldImage))
            fileService.DeleteFile(oldImage);
    }

    public async Task DeleteInternalExercisePhotoAsync(long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateDeleteInternalPhotoAsync(exerciseId, cancellationToken);

        var oldImage = await exerciseRepository.GetExercisePhotoAsync(exerciseId, cancellationToken);

        if (!string.IsNullOrEmpty(oldImage))
        {
            await exerciseRepository.DeleteExercisePhotoAsync(exerciseId, cancellationToken)
                .LogExceptionsAsync(_logger, FailedToActionStr($"{internalExerciseEntityName} photo", "delete"));

            fileService.DeleteFile(oldImage);
        }
    }


    #endregion

    #region User Exercises

    const string userExerciseEntityName = "user exercise";

    public async Task<Exercise> AddUserExerciseAsync(string userId, Exercise exercise, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateAddOwnedAsync(userId, exercise, cancellationToken);

        exercise.CreatedByUserId = userId;

        return await exerciseRepository.AddAsync(exercise, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "add"));
    }

    public async Task DeleteExerciseFromUserAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateDeleteOwnedAsync(userId, exerciseId, cancellationToken);

        var exercise = (await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken))!;
        string? exerciseImage = exercise.Image;

        await exerciseRepository.RemoveAsync(exerciseId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "delete"));

        if (!string.IsNullOrEmpty(exerciseImage))
            fileService.DeleteFile(exerciseImage);
    }

    public async Task<Exercise?> GetUserExerciseByIdAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetOwnedByIdAsync(userId, exerciseId, cancellationToken);

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetUserExerciseByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetOwnedByNameAsync(userId, name, cancellationToken);

        var exercise = await exerciseRepository.GetByNameAsync(name, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetUserExerciseByIdWithDetailsAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetOwnedByIdAsync(userId, exerciseId, cancellationToken);

        var exercise = await exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetUserExerciseByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetOwnedByNameAsync(userId, name, cancellationToken);

        var exercise = await exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<IEnumerable<Exercise>> GetUserExercisesAsync(string userId, ExerciseType? exerciseType, string? filterQuery, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetAllOwnedAsync(userId, exerciseType, cancellationToken);

        var exercises = exerciseRepository.GetUserExercises(userId, exerciseType, filterQuery);

        return await exercises.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("user exercises", "get"));
    }

    public async Task UpdateUserExerciseAsync(string userId, Exercise exercise, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateOwnedAsync(userId, exercise, cancellationToken);

        await exerciseRepository.UpdatePartialAsync(exercise.Id, ExerciseUpdateAction(exercise), cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "update"));
    }

    public async Task UpdateUserExerciseMusclesAsync(string userId, long exerciseId, IEnumerable<long> muscleIds, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateOwnedMusclesAsync(userId, exerciseId, muscleIds, cancellationToken);

        var updateWorkingMusclesAction = new Action<Exercise>(e =>
        {
            e.WorkingMuscles = [..muscleRepository.FindByIds(muscleIds)];
        });

        await exerciseRepository.UpdatePartialAsync(exerciseId, updateWorkingMusclesAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{userExerciseEntityName}'s muscles", "update"));
    }

    public async Task UpdateUserExerciseEquipmentsAsync(string userId, long exerciseId, IEnumerable<long> equipmentIds, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateOwnedEquipmentsAsync(userId, exerciseId, equipmentIds, cancellationToken);

        var updateEquipmentsAction = new Action<Exercise>(e =>
        {
            e.Equipments = [..equipmentRepository.FindByIds(equipmentIds)];
        });

        await exerciseRepository.UpdatePartialAsync(exerciseId, updateEquipmentsAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{userExerciseEntityName}'s equipments", "update"));
    }

    public async Task UpdateUserExerciseAliasesAsync(string userId, long exerciseId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateOwnedAliasesAsync(userId, exerciseId, aliasesStr, cancellationToken);

        await exerciseAliasRepository.RemoveByExerciseIdAsync(exerciseId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{userExerciseEntityName} aliases", "delete"));

        await exerciseAliasRepository.AddExerciseAliasesAsync(exerciseId, aliasesStr, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{userExerciseEntityName} aliases", "add"));
    }

    public async Task UpdateUserExercisePhotoAsync(string userId, long exerciseId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateUpdateOwnedPhotoAsync(userId, exerciseId, fileUpload, cancellationToken);

        string? image = await fileService.GetImageAsync(fileUpload, exercisePhotosDirectory, maxExerciseImageSizeInMB, true);
        var oldImage = await exerciseRepository.GetExercisePhotoAsync(exerciseId, cancellationToken);

        await (!string.IsNullOrEmpty(image) ?
            exerciseRepository.UpdateExercisePhotoAsync(exerciseId, image!, cancellationToken) :
            exerciseRepository.DeleteExercisePhotoAsync(exerciseId, cancellationToken)
        ).LogExceptionsAsync(_logger, FailedToActionStr($"{userExerciseEntityName} photo", "update"));

        if (!string.IsNullOrEmpty(oldImage))
            fileService.DeleteFile(oldImage);
    }

    public async Task DeleteUserExercisePhotoAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateDeleteOwnedPhotoAsync(userId, exerciseId, cancellationToken);

        var oldImage = await exerciseRepository.GetExercisePhotoAsync(exerciseId, cancellationToken);

        if (!string.IsNullOrEmpty(oldImage))
        {
            await exerciseRepository.DeleteExercisePhotoAsync(exerciseId, cancellationToken)
                .LogExceptionsAsync(_logger, FailedToActionStr($"{userExerciseEntityName} photo", "delete"));

            fileService.DeleteFile(oldImage);
        }
    }


    #endregion

    #region All Exercises

    const string exerciseEntityName = "exercise";

    public async Task<Exercise?> GetExerciseByIdAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetByIdAsync(userId, exerciseId, cancellationToken);

        var exercise = await exerciseRepository.GetByIdAsync(exerciseId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetExerciseByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetByNameAsync(userId, name, cancellationToken);

        var exercise = await exerciseRepository.GetByNameAsync(name, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetExerciseByIdWithDetailsAsync(string userId, long exerciseId, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetByIdAsync(userId, exerciseId, cancellationToken);

        var exercise = await exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetExerciseByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetByNameAsync(userId, name, cancellationToken);

        var exercise = await exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(exerciseEntityName, "get"));

        return exercise;
    }

    public async Task<IEnumerable<Exercise>> GetAllExercisesAsync(string userId, ExerciseType? exerciseType, string? filterQuery, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetAllAsync(userId, exerciseType, cancellationToken);

        var exercises = exerciseRepository.GetAllExercises(userId, exerciseType, filterQuery);

        return await exercises.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("exercises", "get"));
    }

    public async Task<IEnumerable<Exercise>> GetUsedExercisesAsync(string userId, ExerciseType? exerciseType, string? filterQuery, CancellationToken cancellationToken)
    {
        await exerciseServiceValidator.ValidateGetUsedAsync(userId, exerciseType, cancellationToken);

        var exerciseRecords = exerciseRecordRepository.GetUserExerciseRecords(userId);
            
        var exercises = exerciseRecords
            .Select(er => er.Exercise!)
            .Distinct();

        exercises =  exerciseRepository.FilterByExerciseType(exercises, exerciseType);
        exercises =  exerciseRepository.FilterByQuery(exercises, filterQuery);

        return await exercises.ToListAsync()
            .LogExceptionsAsync(_logger, FailedToActionStr("used exercises", "get"));
    }

    #endregion

    public Action<Exercise> ExerciseUpdateAction(Exercise exercise)
    {
        var updateAction = new Action<Exercise>(e =>
        {
            e.Name = exercise.Name;
            //e.Image = exercise.Image;
            e.Description = exercise.Description;
            e.Type = exercise.Type;
        });

        return updateAction;
    }
}
