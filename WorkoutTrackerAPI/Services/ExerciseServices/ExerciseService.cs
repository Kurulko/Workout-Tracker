using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Services.FileServices;

namespace WorkoutTrackerAPI.Services;

public class ExerciseService : BaseWorkoutService<Exercise>, IExerciseService
{
    readonly UserRepository userRepository;
    readonly EquipmentRepository equipmentRepository;
    readonly MuscleRepository muscleRepository;
    readonly IFileService fileService;
    public ExerciseService(ExerciseRepository baseWorkoutRepository, UserRepository userRepository, EquipmentRepository equipmentRepository, MuscleRepository muscleRepository, IFileService fileService) : base(baseWorkoutRepository)
    {
        this.userRepository = userRepository;
        this.equipmentRepository = equipmentRepository;
        this.muscleRepository = muscleRepository;
        this.fileService = fileService;
    }

    readonly EntryNullException exerciseIsNullException = new (nameof(Exercise));
    readonly InvalidIDException invalidExerciseIDException = new (nameof(Exercise));
    readonly ArgumentNullOrEmptyException exerciseNameIsNullOrEmptyException = new("Exercise name");

    NotFoundException ExerciseNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID(nameof(Exercise), id);
    NotFoundException ExerciseNotFoundByNameException(string name)
        => NotFoundException.NotFoundExceptionByName(nameof(Exercise), name);


    ArgumentException InvalidExerciseIDWhileAddingException => InvalidEntryIDWhileAddingException(nameof(Exercise), "exercise");

    public async Task<ServiceResult<Exercise>> AddInternalExerciseAsync(Exercise exercise)
    {
        try
        {
            if (exercise is null)
                throw  exerciseIsNullException;

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw new UnauthorizedAccessException("Exercise entry cannot be created by user.");

            if (exercise.Id != 0)
                throw InvalidExerciseIDWhileAddingException;

            if (await baseWorkoutRepository.ExistsByNameAsync(exercise.Name))
                throw EntryNameMustBeUnique(nameof(Exercise));

            await baseWorkoutRepository.AddAsync(exercise);
            return ServiceResult<Exercise>.Ok(exercise);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult<Exercise>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("internal exercise", "add", ex));
        }
    }

    public async Task<ServiceResult<Exercise>> AddUserExerciseAsync(string userId, Exercise exercise)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exercise is null)
                throw exerciseIsNullException;

            if (exercise.Id != 0)
                throw InvalidExerciseIDWhileAddingException;

            if (await baseWorkoutRepository.ExistsByNameAsync(exercise.Name))
                throw EntryNameMustBeUnique(nameof(Exercise));

            exercise.CreatedByUserId = userId;
            await baseWorkoutRepository.AddAsync(exercise);

            return ServiceResult<Exercise>.Ok(exercise);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Exercise>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("user exercise", "add", ex));
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
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("internal exercise", "delete"));
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
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("user exercise", "delete"));
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

    public async Task<ServiceResult<Exercise>> GetInternalExerciseByIdAsync(long exerciseId)
    {
        try
        {
            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exerciseById = await baseWorkoutRepository.GetByIdAsync(exerciseId);

            if (exerciseById != null && exerciseById.CreatedByUserId != null)
                throw UserNotHavePermissionException("get", "internal exercise");

            return ServiceResult<Exercise>.Ok(exerciseById);
        }
        catch (ArgumentException argEx)
        {
            return ServiceResult<Exercise>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("internal exercise", "get", ex));
        }
    }

    public async Task<ServiceResult<Exercise>> GetInternalExerciseByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                throw exerciseNameIsNullOrEmptyException;

            var exerciseByName = await baseWorkoutRepository.GetByNameAsync(name);

            if (exerciseByName != null && exerciseByName.CreatedByUserId != null)
                throw UserNotHavePermissionException("get", "internal exercise by name");

            return ServiceResult<Exercise>.Ok(exerciseByName);
        }
        catch (ArgumentException argEx)
        {
            return ServiceResult<Exercise>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("internal exercise by name", "get", ex));
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
            return ServiceResult<IQueryable<Exercise>>.Fail(FailedToActionStr("internal exercises", "get", ex));
        }
    }

    public async Task<ServiceResult<Exercise>> GetUserExerciseByIdAsync(string userId, long exerciseId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var userExerciseById = await baseWorkoutRepository.GetByIdAsync(exerciseId);

            if (userExerciseById != null && userExerciseById.CreatedByUserId != userId)
                throw UserNotHavePermissionException("get", "user exercise");

            return ServiceResult<Exercise>.Ok(userExerciseById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Exercise>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("user exercise", "get", ex));
        }
    }

    public async Task<ServiceResult<Exercise>> GetUserExerciseByNameAsync(string userId, string name)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw exerciseNameIsNullOrEmptyException;

            var userExerciseByName = await baseWorkoutRepository.GetByNameAsync(name);

            if (userExerciseByName != null && userExerciseByName.CreatedByUserId != userId)
                throw UserNotHavePermissionException("get", "user exercise by name");

            return ServiceResult<Exercise>.Ok(userExerciseByName);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Exercise>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("user exercise by name", "get", ex));
        }
    }

    public async Task<ServiceResult<Exercise>> GetExerciseByIdAsync(string userId, long exerciseId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exerciseById = await baseWorkoutRepository.GetByIdAsync(exerciseId);

            if (exerciseById != null && (exerciseById.CreatedByUserId != userId && exerciseById.CreatedByUserId != null))
                throw UserNotHavePermissionException("get", "exercise");

            return ServiceResult<Exercise>.Ok(exerciseById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Exercise>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("exercise", "get", ex));
        }
    }

    public async Task<ServiceResult<Exercise>> GetExerciseByNameAsync(string userId, string name)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw exerciseNameIsNullOrEmptyException;

            var exerciseByName = await baseWorkoutRepository.GetByNameAsync(name);

            if (exerciseByName != null && (exerciseByName.CreatedByUserId != userId && exerciseByName.CreatedByUserId != null))
                throw UserNotHavePermissionException("get", "exercise by name");

            return ServiceResult<Exercise>.Ok(exerciseByName);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Exercise>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("exercise by name", "get", ex));
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
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(FailedToActionStr("user exercises", "get", ex));
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
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(FailedToActionStr("exercises", "get", ex));
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

            _exercise.Name = exercise.Name;
            _exercise.Image = exercise.Image;
            _exercise.Description = exercise.Description;
            _exercise.Type = exercise.Type;

            await baseWorkoutRepository.UpdateAsync(_exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("exercise", "update", ex));
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

            _exercise.Name = exercise.Name;
            _exercise.Image = exercise.Image;
            _exercise.Description = exercise.Description;
            _exercise.Type = exercise.Type;

            await baseWorkoutRepository.UpdateAsync(_exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("user exercise", "update", ex));
        }
    }

    public async Task<ServiceResult> UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds)
    {
        if (exerciseId < 1)
            throw invalidExerciseIDException;

        try
        {
            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundByIDException(exerciseId);

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw UserNotHavePermissionException("update", "internal exercise's muscles");

            exercise.WorkingMuscles = await muscleRepository.FindAsync(m => muscleIds.Contains(m.Id));

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("internal exercise's muscles", "update", ex));
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

            exercise.WorkingMuscles = await muscleRepository.FindAsync(m => muscleIds.Contains(m.Id));

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("user exercise's muscles", "update", ex));
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

            exercise.Equipments = await equipmentRepository.FindAsync(m => equipmentIds.Contains(m.Id));

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("internal exercise's equipments", "update", ex));
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

            exercise.Equipments = await equipmentRepository.FindAsync(m => equipmentIds.Contains(m.Id));

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("user exercise's equipments", "update", ex));
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
}
