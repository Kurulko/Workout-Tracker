using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Infrastructure.Validators.Services.Exercises;

namespace WorkoutTracker.Infrastructure.Services.Exercises;

internal class ExerciseService : BaseWorkoutService<ExerciseService, Exercise>, IExerciseService
{
    readonly IExerciseRepository exerciseRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly ExerciseServiceValidator exerciseServiceValidator;
    readonly IEquipmentRepository equipmentRepository;
    readonly IMuscleRepository muscleRepository;
    readonly IFileService fileService;
    public ExerciseService (
        IExerciseRepository exerciseRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IEquipmentRepository equipmentRepository,
        IMuscleRepository muscleRepository,
        IFileService fileService,
        ExerciseServiceValidator exerciseServiceValidator,
        ILogger<ExerciseService> logger
    ) : base(exerciseRepository, logger)
    {
        this.exerciseRepository = exerciseRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.equipmentRepository = equipmentRepository;
        this.muscleRepository = muscleRepository;
        this.fileService = fileService;
        this.exerciseServiceValidator = exerciseServiceValidator;
    }


    #region Internal Exercises

    const string internalExerciseEntityName = "internal exercise";

    public async Task<Exercise> AddInternalExerciseAsync(Exercise exercise)
    {
        await exerciseServiceValidator.ValidateAddInternalAsync(exercise);

        return await baseWorkoutRepository.AddAsync(exercise)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "add"));
    }

    public async Task DeleteInternalExerciseAsync(long exerciseId)
    {
        await exerciseServiceValidator.ValidateDeleteInternalAsync(exerciseId);

        var exercise = (await baseWorkoutRepository.GetByIdAsync(exerciseId))!;
        string? exerciseImage = exercise.Image;

        await baseWorkoutRepository.RemoveAsync(exerciseId)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "delete"));

        if (!string.IsNullOrEmpty(exerciseImage))
        {
            fileService.DeleteFile(exerciseImage);
        }
    }

    public async Task<Exercise?> GetInternalExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false)
    {
        await exerciseServiceValidator.ValidateGetInternalByIdAsync(userId, exerciseId, withDetails);

        var exercise = await (withDetails ?
            exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId) :
            baseWorkoutRepository.GetByIdAsync(exerciseId)
        )
        .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetInternalExerciseByNameAsync(string userId, string name, bool withDetails = false)
    {
        await exerciseServiceValidator.ValidateGetInternalByNameAsync(userId, name, withDetails);

        var exercise = await (withDetails ?
            exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId) :
            baseWorkoutRepository.GetByNameAsync(name)
        )
        .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<IQueryable<Exercise>> GetInternalExercisesAsync(ExerciseType? exerciseType = null)
    {
        await exerciseServiceValidator.ValidateGetAllInternalAsync(exerciseType);

        var exercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == null)
            .LogExceptionsAsync(_logger, FailedToActionStr("internal exercises", "get"));

        if (exerciseType is ExerciseType _exerciseType)
            exercises = exercises.Where(e => e.Type == exerciseType);

        return exercises;
    }

    public async Task UpdateInternalExerciseAsync(Exercise exercise)
    {
        await exerciseServiceValidator.ValidateUpdateInternalAsync(exercise);

        var _exercise = (await baseWorkoutRepository.GetByIdAsync(exercise.Id))!;

        _exercise.Name = exercise.Name;
        _exercise.Image = exercise.Image;
        _exercise.Description = exercise.Description;
        _exercise.Type = exercise.Type;

        await baseWorkoutRepository.UpdateAsync(_exercise)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalExerciseEntityName, "update"));
    }


    public async Task UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds)
    {
        await exerciseServiceValidator.ValidateUpdateInternalMusclesAsync(exerciseId, muscleIds);

        var exercise = (await baseWorkoutRepository.GetByIdAsync(exerciseId))!;

        exercise.WorkingMuscles = (await muscleRepository.FindAsync(m => muscleIds.Contains(m.Id))).ToList();

        await baseWorkoutRepository.UpdateAsync(exercise)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{internalExerciseEntityName}'s muscles", "update"));
    }

    public async Task UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentIds)
    {
        await exerciseServiceValidator.ValidateUpdateInternalEquipmentsAsync(exerciseId, equipmentIds);

        var exercise = (await baseWorkoutRepository.GetByIdAsync(exerciseId))!;

        exercise.Equipments = (await equipmentRepository.FindAsync(m => equipmentIds.Contains(m.Id)))
            .Where(e => e.OwnedByUserId == null) //only internal equipments for internal exercise
            .ToList();

        await baseWorkoutRepository.UpdateAsync(exercise)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{internalExerciseEntityName}'s equipments", "update"));
    }

    #endregion

    #region User Exercises

    const string userExerciseEntityName = "user exercise";

    public async Task<Exercise> AddUserExerciseAsync(string userId, Exercise exercise)
    {
        await exerciseServiceValidator.ValidateAddOwnedAsync(userId, exercise);

        exercise.CreatedByUserId = userId;

        return await baseWorkoutRepository.AddAsync(exercise)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "add"));
    }

    public async Task DeleteExerciseFromUserAsync(string userId, long exerciseId)
    {
        await exerciseServiceValidator.ValidateDeleteOwnedAsync(userId, exerciseId);

        var exercise = (await baseWorkoutRepository.GetByIdAsync(exerciseId))!;
        string? exerciseImage = exercise.Image;

        await baseWorkoutRepository.RemoveAsync(exerciseId)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "delete"));

        if (!string.IsNullOrEmpty(exerciseImage))
        {
            fileService.DeleteFile(exerciseImage);
        }
    }

    public async Task<Exercise?> GetUserExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false)
    {
        await exerciseServiceValidator.ValidateGetOwnedByIdAsync(userId, exerciseId);

        var exercise = await (withDetails ?
            exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId) :
            baseWorkoutRepository.GetByIdAsync(exerciseId)
        )
        .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetUserExerciseByNameAsync(string userId, string name, bool withDetails = false)
    {
        await exerciseServiceValidator.ValidateGetOwnedByNameAsync(userId, name);

        var exercise = await (withDetails ?
            exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId) :
            baseWorkoutRepository.GetByNameAsync(name)
        )
        .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "get"));

        return exercise;
    }

    public async Task<IQueryable<Exercise>> GetUserExercisesAsync(string userId, ExerciseType? exerciseType = null)
    {
        await exerciseServiceValidator.ValidateGetAllOwnedAsync(userId, exerciseType);

        var exercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == null)
            .LogExceptionsAsync(_logger, FailedToActionStr("user exercises", "get"));

        if (exerciseType is ExerciseType _exerciseType)
            exercises = exercises.Where(e => e.Type == exerciseType);

        return exercises;
    }

    public async Task UpdateUserExerciseAsync(string userId, Exercise exercise)
    {
        await exerciseServiceValidator.ValidateUpdateOwnedAsync(userId, exercise);

        var _exercise = (await baseWorkoutRepository.GetByIdAsync(exercise.Id))!;

        _exercise.Name = exercise.Name;
        _exercise.Image = exercise.Image;
        _exercise.Description = exercise.Description;
        _exercise.Type = exercise.Type;

        await baseWorkoutRepository.UpdateAsync(_exercise)
            .LogExceptionsAsync(_logger, FailedToActionStr(userExerciseEntityName, "update"));
    }

    public async Task UpdateUserExerciseMusclesAsync(string userId, long exerciseId, IEnumerable<long> muscleIds)
    {
        await exerciseServiceValidator.ValidateUpdateOwnedMusclesAsync(userId, exerciseId, muscleIds);

        var exercise = (await baseWorkoutRepository.GetByIdAsync(exerciseId))!;

        exercise.WorkingMuscles = (await muscleRepository.FindAsync(m => muscleIds.Contains(m.Id))).ToList();

        await baseWorkoutRepository.UpdateAsync(exercise)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{userExerciseEntityName}'s muscles", "update"));
    }

    public async Task UpdateUserExerciseEquipmentsAsync(string userId, long exerciseId, IEnumerable<long> equipmentIds)
    {
        await exerciseServiceValidator.ValidateUpdateOwnedEquipmentsAsync(userId, exerciseId, equipmentIds);

        var exercise = (await baseWorkoutRepository.GetByIdAsync(exerciseId))!;

        exercise.Equipments = (await equipmentRepository.FindAsync(m => equipmentIds.Contains(m.Id))).ToList();

        await baseWorkoutRepository.UpdateAsync(exercise)
            .LogExceptionsAsync(_logger, FailedToActionStr($"{userExerciseEntityName}'s equipments", "update"));
    }

    #endregion

    #region All Exercises

    const string exerciseEntityName = "exercise";

    public async Task<Exercise?> GetExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false)
    {
        await exerciseServiceValidator.ValidateGetByIdAsync(userId, exerciseId);

        var exercise = await (withDetails ?
            exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId) :
            baseWorkoutRepository.GetByIdAsync(exerciseId)
        )
        .LogExceptionsAsync(_logger, FailedToActionStr(exerciseEntityName, "get"));

        return exercise;
    }

    public async Task<Exercise?> GetExerciseByNameAsync(string userId, string name, bool withDetails = false)
    {
        await exerciseServiceValidator.ValidateGetByNameAsync(userId, name);

        var exercise = await (withDetails ?
            exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId) :
            baseWorkoutRepository.GetByNameAsync(name)
        )
        .LogExceptionsAsync(_logger, FailedToActionStr(exerciseEntityName, "get"));

        return exercise;
    }

    public async Task<IQueryable<Exercise>> GetAllExercisesAsync(string userId, ExerciseType? exerciseType = null)
    {
        await exerciseServiceValidator.ValidateGetAllAsync(userId, exerciseType);

        var exercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == userId || e.CreatedByUserId == null)
            .LogExceptionsAsync(_logger, FailedToActionStr("exercises", "get"));

        if (exerciseType is ExerciseType _exerciseType)
            exercises = exercises.Where(e => e.Type == exerciseType);

        return exercises;
    }

    public async Task<IQueryable<Exercise>> GetUsedExercisesAsync(string userId, ExerciseType? exerciseType = null)
    {
        await exerciseServiceValidator.ValidateGetUsedAsync(userId, exerciseType);

        var exerciseRecords = await exerciseRecordRepository.GetExerciseRecordsByUserIdAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionStr("used exercises", "get"));

        var exercises = exerciseRecords
            .Select(er => er.Exercise!)
            .Distinct();

        if (exerciseType is ExerciseType _exerciseType)
            exercises = exercises.Where(e => e.Type == exerciseType);

        return exercises;
    }

    #endregion
}
