using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.ExerciseServices;

namespace WorkoutTrackerAPI.Services;

public class ExerciseService : BaseWorkoutService<Exercise>, IExerciseService
{
    readonly UserRepository userRepository;
    public ExerciseService(ExerciseRepository baseWorkoutRepository, UserRepository userRepository) : base(baseWorkoutRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException ExerciseIsNullException = new (nameof(Exercise));
    readonly InvalidIDException InvalidExerciseIDException = new (nameof(Exercise));
    readonly NotFoundException ExerciseNotFoundException = new (nameof(Exercise));
    readonly ArgumentNullOrEmptyException ExerciseNameIsNullOrEmptyException = new("Exercise name");

    ArgumentException InvalidExerciseIDWhileAddingException => InvalidEntryIDWhileAddingException(nameof(Exercise), "exercise");

    public async Task<ServiceResult<Exercise>> AddInternalExerciseAsync(Exercise exercise)
    {
        try
        {
            if (exercise is null)
                throw  ExerciseIsNullException;

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw new UnauthorizedAccessException("Exercise entry cannot be created by user.");

            if (exercise.Id != 0)
                throw InvalidExerciseIDWhileAddingException;

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
                throw ExerciseIsNullException;

            if (exercise.Id != 0)
                throw InvalidExerciseIDWhileAddingException;

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
                throw InvalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundException;

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw UserNotHavePermissionException("delete", "internal exercise");

            await baseWorkoutRepository.RemoveAsync(exerciseId);
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
                throw InvalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw ExerciseNotFoundException;

            if (exercise.CreatedByUserId != userId)
                throw UserNotHavePermissionException("delete", "exercise");

            await baseWorkoutRepository.RemoveAsync(exerciseId);
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
            throw InvalidExerciseIDException;

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
            throw ExerciseNameIsNullOrEmptyException;

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
                throw InvalidExerciseIDException;

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
                throw ExerciseNameIsNullOrEmptyException;

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

    public async Task<ServiceResult<IQueryable<Exercise>>> GetInternalExercisesAsync()
    {
        try
        {
            var exercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == null);
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
                throw InvalidExerciseIDException;

            var userExerciseById = await baseWorkoutRepository.GetByIdAsync(exerciseId);
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
                throw ExerciseNameIsNullOrEmptyException;

            var userExerciseByName = await baseWorkoutRepository.GetByNameAsync(name);
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

    public async Task<ServiceResult<IQueryable<Exercise>>> GetUserExercisesAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userExercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == userId);
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

    public async Task<ServiceResult<IQueryable<Exercise>>> GetAllExercisesAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var exercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == userId || e.CreatedByUserId == null);
            return ServiceResult<IQueryable<Exercise>>.Ok(exercises);
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
                throw ExerciseIsNullException;

            if (exercise.Id < 1)
                throw InvalidExerciseIDException;

            var _exercise = await baseWorkoutRepository.GetByIdAsync(exercise.Id) ?? throw ExerciseNotFoundException;

            if (!string.IsNullOrEmpty(_exercise.CreatedByUserId))
                throw UserNotHavePermissionException("update", "internal exercise");

            _exercise.Name = exercise.Name;
            _exercise.Image = exercise.Image;
            _exercise.Description = exercise.Description;
            _exercise.Type = exercise.Type;
            _exercise.WorkingMuscles = exercise.WorkingMuscles;

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
                throw ExerciseIsNullException;

            if (exercise.Id < 1)
                throw InvalidExerciseIDException;

            var _exercise = await baseWorkoutRepository.GetByIdAsync(exercise.Id) ?? throw ExerciseNotFoundException;

            if (_exercise.CreatedByUserId != userId)
                throw UserNotHavePermissionException("update", "exercise");

            _exercise.Name = exercise.Name;
            _exercise.Image = exercise.Image;
            _exercise.Description = exercise.Description;
            _exercise.Type = exercise.Type;
            _exercise.WorkingMuscles = exercise.WorkingMuscles;

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

    public async Task<bool> UserExerciseExistsAsync(string userId, long exerciseId)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (exerciseId < 1)
            throw InvalidExerciseIDException;

        return await baseWorkoutRepository.ExistsAsync(exerciseId);
    }

    public async Task<bool> UserExerciseExistsByNameAsync(string userId, string name)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (string.IsNullOrEmpty(name))
            throw ExerciseNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }
}
