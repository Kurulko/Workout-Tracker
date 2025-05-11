using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;

namespace WorkoutTracker.Infrastructure.Services.Exercises;

internal class ExerciseService : BaseWorkoutService<ExerciseService, Exercise>, IExerciseService
{
    readonly IExerciseRepository exerciseRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly IUserRepository userRepository;
    readonly IEquipmentRepository equipmentRepository;
    readonly IMuscleRepository muscleRepository;
    readonly IFileService fileService;
    public ExerciseService(
        IExerciseRepository exerciseRepository, 
        IUserRepository userRepository, 
        IEquipmentRepository equipmentRepository, 
        IMuscleRepository muscleRepository, 
        IExerciseRecordRepository exerciseRecordRepository, 
        IFileService fileService,
        ILogger<ExerciseService> logger
    ) : base(exerciseRepository, logger)
    {
        this.exerciseRepository = exerciseRepository;
        this.userRepository = userRepository;
        this.equipmentRepository = equipmentRepository;
        this.muscleRepository = muscleRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.fileService = fileService;
    }

    readonly EntryNullException exerciseIsNullException = new (nameof(Exercise));
    readonly InvalidIDException invalidExerciseIDException = new (nameof(Exercise));
    readonly ArgumentNullOrEmptyException exerciseNameIsNullOrEmptyException = new("Exercise name");

    NotFoundException ExerciseNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID(nameof(Exercise), id);
    NotFoundException ExerciseNotFoundByNameException(string name)
        => NotFoundException.NotFoundExceptionByName(nameof(Exercise), name);

    ArgumentException ExerciseNameMustBeUnique()
        => EntryNameMustBeUnique(nameof(Exercise));

    ArgumentException InvalidExerciseIDWhileAddingException => InvalidEntryIDWhileAddingException(nameof(Exercise), "exercise");

    #region Internal Exercises

    public async Task<ServiceResult<Exercise>> AddInternalExerciseAsync(Exercise exercise)
    {
        try
        {
            if (exercise is null)
                throw exerciseIsNullException;

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw new UnauthorizedAccessException("Exercise entry cannot be created by user.");

            if (exercise.Id != 0)
                throw InvalidExerciseIDWhileAddingException;

            if (await baseWorkoutRepository.ExistsByNameAsync(exercise.Name))
                throw ExerciseNameMustBeUnique();

            await baseWorkoutRepository.AddAsync(exercise);
            return ServiceResult<Exercise>.Ok(exercise);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal exercise", "add"));
            throw;
        }
    }

    public async Task<ServiceResult> DeleteInternalExerciseAsync(long exerciseId)
    {
        try
        {
            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundByIDException(exerciseId);

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw UserNotHavePermissionException("delete", "internal exercise");

            string? exerciseImage = exercise.Image;
            await baseWorkoutRepository.RemoveAsync(exerciseId);

            if (!string.IsNullOrEmpty(exerciseImage))
            {
                fileService.DeleteFile(exerciseImage);
            }

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal exercise", "delete"));
            throw;
        }
    }

    public async Task<bool> InternalExerciseExistsAsync(long exerciseId)
    {
        if (exerciseId < 1)
            throw invalidExerciseIDException;

        var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId);

        if (exercise is null)
            return false;

        if (exercise.CreatedByUserId != null)
            throw UserNotHavePermissionException("get", "internal exercise");

        return true;
    }

    public async Task<bool> InternalExerciseExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw exerciseNameIsNullOrEmptyException;

        var exercise = await baseWorkoutRepository.GetByNameAsync(name);

        if (exercise is null)
            return false;

        if (exercise.CreatedByUserId != null)
            throw UserNotHavePermissionException("get", "internal exercise by name");

        return true;
    }

    public async Task<ServiceResult<Exercise>> GetInternalExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false)
    {
        try
        {
            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exerciseById = withDetails ? await exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId) : await baseWorkoutRepository.GetByIdAsync(exerciseId);

            if (exerciseById != null && exerciseById.CreatedByUserId != null)
                throw UserNotHavePermissionException("get", "internal exercise");

            return ServiceResult<Exercise>.Ok(exerciseById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal exercise", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<Exercise>> GetInternalExerciseByNameAsync(string userId, string name, bool withDetails = false)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                throw exerciseNameIsNullOrEmptyException;

            var exerciseByName = withDetails ? await exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId) : await baseWorkoutRepository.GetByNameAsync(name);

            if (exerciseByName != null && exerciseByName.CreatedByUserId != null)
                throw UserNotHavePermissionException("get", "internal exercise by name");

            return ServiceResult<Exercise>.Ok(exerciseByName);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal exercise by name", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<Exercise>>> GetInternalExercisesAsync(ExerciseType? exerciseType = null)
    {
        try
        {
            var exercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == null);

            if (exerciseType is ExerciseType _exerciseType)
                exercises = exercises.Where(e => e.Type == exerciseType);

            return ServiceResult<IQueryable<Exercise>>.Ok(exercises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal exercises", "get"));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateInternalExerciseAsync(Exercise exercise)
    {
        try
        {
            if (exercise is null)
                throw exerciseIsNullException;

            if (exercise.Id < 1)
                throw invalidExerciseIDException;

            var _exercise = await baseWorkoutRepository.GetByIdAsync(exercise.Id) ?? throw ExerciseNotFoundByIDException(exercise.Id);

            if (!string.IsNullOrEmpty(_exercise.CreatedByUserId))
                throw UserNotHavePermissionException("update", "internal exercise");

            var isSameName = _exercise.Name != exercise.Name;
            var isUniqueExerciseName = isSameName || await baseWorkoutRepository.ExistsByNameAsync(exercise.Name);
            if (!isUniqueExerciseName)
                throw ExerciseNameMustBeUnique();

            _exercise.Name = exercise.Name;
            _exercise.Image = exercise.Image;
            _exercise.Description = exercise.Description;
            _exercise.Type = exercise.Type;

            await baseWorkoutRepository.UpdateAsync(_exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("exercise", "update"));
            throw;
        }
    }


    public async Task<ServiceResult> UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds)
    {
        try
        {
            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundByIDException(exerciseId);

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw UserNotHavePermissionException("update", "internal exercise's muscles");

            exercise.WorkingMuscles = (await muscleRepository.FindAsync(m => muscleIds.Contains(m.Id))).ToList();

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal exercise's muscles", "update"));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentIds)
    {
        try
        {
            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundByIDException(exerciseId);

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw UserNotHavePermissionException("update", "internal exercise's equipments");

            exercise.Equipments = (await equipmentRepository.FindAsync(m => equipmentIds.Contains(m.Id)))
                .Where(e => e.OwnedByUserId == null) //only internal equipments for internal exercise
                .ToList();

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal exercise's equipments", "update"));
            throw;
        }
    }

    #endregion

    #region User Exercises

    public async Task<ServiceResult<Exercise>> AddUserExerciseAsync(string userId, Exercise exercise)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exercise is null)
                throw exerciseIsNullException;

            if (exercise.Id != 0)
                throw InvalidExerciseIDWhileAddingException;

            var isUniqueExerciseName = await IsUniqueExerciseNameForUserAsync(exercise.Name, userId);
            if (!isUniqueExerciseName)
                throw ExerciseNameMustBeUnique();

            exercise.CreatedByUserId = userId;
            await baseWorkoutRepository.AddAsync(exercise);

            return ServiceResult<Exercise>.Ok(exercise);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user exercise", "add", userId));
            throw;
        }
    }

    public async Task<ServiceResult> DeleteExerciseFromUserAsync(string userId, long exerciseId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundByIDException(exerciseId);

            if (exercise.CreatedByUserId != userId)
                throw UserNotHavePermissionException("delete", "exercise");

            string? exerciseImage = exercise.Image;
            await baseWorkoutRepository.RemoveAsync(exerciseId);

            if (!string.IsNullOrEmpty(exerciseImage))
            {
                fileService.DeleteFile(exerciseImage);
            }

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user exercise", "delete", userId));
            throw;
        }
    }

    public async Task<ServiceResult<Exercise>> GetUserExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var userExerciseById = withDetails ? await exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId) : await baseWorkoutRepository.GetByIdAsync(exerciseId);

            if (userExerciseById != null && userExerciseById.CreatedByUserId != userId)
                throw UserNotHavePermissionException("get", "user exercise");

            return ServiceResult<Exercise>.Ok(userExerciseById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user exercise", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult<Exercise>> GetUserExerciseByNameAsync(string userId, string name, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw exerciseNameIsNullOrEmptyException;

            var userExerciseByName = withDetails ? await exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId) : await baseWorkoutRepository.GetByNameAsync(name);

            if (userExerciseByName != null && userExerciseByName.CreatedByUserId != userId)
                throw UserNotHavePermissionException("get", "user exercise by name");

            return ServiceResult<Exercise>.Ok(userExerciseByName);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user exercise by name", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<Exercise>>> GetUserExercisesAsync(string userId, ExerciseType? exerciseType = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);
            var userExercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == userId);

            if (exerciseType is ExerciseType _exerciseType)
                userExercises = userExercises.Where(e => e.Type == exerciseType);

            return ServiceResult<IQueryable<Exercise>>.Ok(userExercises);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user exercises", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateUserExerciseAsync(string userId, Exercise exercise)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exercise is null)
                throw exerciseIsNullException;

            if (exercise.Id < 1)
                throw invalidExerciseIDException;

            var _exercise = await baseWorkoutRepository.GetByIdAsync(exercise.Id) ?? throw ExerciseNotFoundByIDException(exercise.Id);

            if (_exercise.CreatedByUserId != userId)
                throw UserNotHavePermissionException("update", "exercise");

            var isSameName = _exercise.Name != exercise.Name;
            var isUniqueExerciseName = isSameName || await IsUniqueExerciseNameForUserAsync(exercise.Name, userId);
            if (!isUniqueExerciseName)
                throw ExerciseNameMustBeUnique();

            _exercise.Name = exercise.Name;
            _exercise.Image = exercise.Image;
            _exercise.Description = exercise.Description;
            _exercise.Type = exercise.Type;

            await baseWorkoutRepository.UpdateAsync(_exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user exercise", "update", userId));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateUserExerciseMusclesAsync(string userId, long exerciseId, IEnumerable<long> muscleIds)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundByIDException(exerciseId);

            if (exercise.CreatedByUserId != userId)
                throw UserNotHavePermissionException("update", "user exercise's muscles");

            exercise.WorkingMuscles = (await muscleRepository.FindAsync(m => muscleIds.Contains(m.Id))).ToList();

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user exercise's muscles", "update", userId));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateUserExerciseEquipmentsAsync(string userId, long exerciseId, IEnumerable<long> equipmentIds)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundByIDException(exerciseId);

            if (exercise.CreatedByUserId != userId)
                throw UserNotHavePermissionException("update", "user exercise's equipments");

            exercise.Equipments = (await equipmentRepository.FindAsync(m => equipmentIds.Contains(m.Id))).ToList();

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user exercise's equipments", "update", userId));
            throw;
        }
    }

    public async Task<bool> UserExerciseExistsAsync(string userId, long exerciseId)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (exerciseId < 1)
            throw invalidExerciseIDException;

        return await baseWorkoutRepository.ExistsAsync(exerciseId);
    }

    public async Task<bool> UserExerciseExistsByNameAsync(string userId, string name)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (string.IsNullOrEmpty(name))
            throw exerciseNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }


    #endregion

    #region All Exercises

    public async Task<ServiceResult<Exercise>> GetExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exerciseById = withDetails ? await exerciseRepository.GetExerciseByIdWithDetailsAsync(exerciseId, userId) : await baseWorkoutRepository.GetByIdAsync(exerciseId);

            if (exerciseById != null && (exerciseById.CreatedByUserId != userId && exerciseById.CreatedByUserId != null))
                throw UserNotHavePermissionException("get", "exercise");

            return ServiceResult<Exercise>.Ok(exerciseById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult<Exercise>> GetExerciseByNameAsync(string userId, string name, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw exerciseNameIsNullOrEmptyException;

            var exerciseByName = withDetails ? await exerciseRepository.GetExerciseByNameWithDetailsAsync(name, userId) : await baseWorkoutRepository.GetByNameAsync(name);

            if (exerciseByName != null && (exerciseByName.CreatedByUserId != userId && exerciseByName.CreatedByUserId != null))
                throw UserNotHavePermissionException("get", "exercise by name");

            return ServiceResult<Exercise>.Ok(exerciseByName);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise by name", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<Exercise>>> GetAllExercisesAsync(string userId, ExerciseType? exerciseType = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);
            var allExercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == userId || e.CreatedByUserId == null);

            if (exerciseType is ExerciseType _exerciseType)
                allExercises = allExercises.Where(e => e.Type == exerciseType);

            return ServiceResult<IQueryable<Exercise>>.Ok(allExercises);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercises", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<Exercise>>> GetUsedExercisesAsync(string userId, ExerciseType? exerciseType = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);
            var usedExercises = (await exerciseRecordRepository.FindAsync(er => er.UserId == userId))
                .Select(er => er.Exercise!)
                .Distinct();

            if (exerciseType is ExerciseType _exerciseType)
                usedExercises = usedExercises.Where(e => e.Type == exerciseType);

            return ServiceResult<IQueryable<Exercise>>.Ok(usedExercises);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("used exercises", "get", userId));
            throw;
        }
    }

    #endregion

    async Task<bool> IsUniqueExerciseNameForUserAsync(string name, string userId)
    {
        var isAnyExerciseNames = await baseWorkoutRepository.AnyAsync(w => w.Name == name && (w.CreatedByUserId == userId || w.CreatedByUserId == null));
        return !isAnyExerciseNames;
    }
}
