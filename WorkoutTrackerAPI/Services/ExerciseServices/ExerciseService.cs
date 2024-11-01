using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
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

    readonly EntryNullException exerciseIsNullException = new (nameof(Exercise));
    readonly InvalidIDException invalidExerciseIDException = new (nameof(Exercise));
    readonly NotFoundException exerciseNotFoundException = new (nameof(Exercise));
    readonly ArgumentNullOrEmptyException exerciseNameIsNullOrEmptyException = new("Exercise name");

    ArgumentException InvalidExerciseIDWhileAddingException => InvalidEntryIDWhileAddingException(nameof(Exercise), "exercise");

    public async Task<ServiceResult<Exercise>> AddExerciseAsync(Exercise exercise)
    {
        try
        {
            if (exercise is null)
                throw  exerciseIsNullException;

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw new UnauthorizedAccessException("Exercise entry cannot be created by user.");

            if (exercise.Id != 0)
                throw InvalidExerciseIDWhileAddingException;

            await baseWorkoutRepository.AddAsync(exercise);
            return ServiceResult<Exercise>.Ok(exercise);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("exercise", "add", ex.Message));
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

            exercise.CreatedByUserId = userId;
            await baseWorkoutRepository.AddAsync(exercise);

            return ServiceResult<Exercise>.Ok(exercise);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("user exercise", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteExerciseAsync(long exerciseId)
    {
        try
        {
            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw exerciseNotFoundException;

            if (!string.IsNullOrEmpty(exercise.CreatedByUserId))
                throw UserNotHavePermissionException("delete", "exercise");

            await baseWorkoutRepository.RemoveAsync(exerciseId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("exercise", "delete"));
        }
    }

    public async Task<ServiceResult> DeleteExerciseFromUserAsync(string userId, long exerciseId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId) ?? throw exerciseNotFoundException;

            if (exercise.CreatedByUserId != userId)
                throw UserNotHavePermissionException("delete", "exercise");

            await baseWorkoutRepository.RemoveAsync(exerciseId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("user exercise", "delete"));
        }
    }

    public async Task<bool> ExerciseExistsAsync(long exerciseId)
    {
        if (exerciseId < 1)
            throw invalidExerciseIDException;

        return await baseWorkoutRepository.ExistsAsync(exerciseId);
    }

    public async Task<bool> ExerciseExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw exerciseNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }

    public async Task<ServiceResult<Exercise>> GetExerciseByIdAsync(long exerciseId)
    {
        try
        {
            if (exerciseId < 1)
                throw invalidExerciseIDException;

            var exerciseById = await baseWorkoutRepository.GetByIdAsync(exerciseId);
            return ServiceResult<Exercise>.Ok(exerciseById);
        }
        catch (ArgumentException argEx)
        {
            return ServiceResult<Exercise>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("exercise", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Exercise>> GetExerciseByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                throw exerciseNameIsNullOrEmptyException;

            var exerciseByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Exercise>.Ok(exerciseByName);
        }
        catch (ArgumentException argEx)
        {
            return ServiceResult<Exercise>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("exercise by name", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<IQueryable<Exercise>>> GetExercisesAsync()
    {
        try
        {
            var exercises = await baseWorkoutRepository.GetAllAsync();
            return ServiceResult<IQueryable<Exercise>>.Ok(exercises);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(FailedToActionStr("exercises", "get", ex.Message));
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
            return ServiceResult<Exercise>.Ok(userExerciseById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("user exercise", "get", ex.Message));
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
            return ServiceResult<Exercise>.Ok(userExerciseByName);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Exercise>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToActionStr("user exercise by name", "get", ex.Message));
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
            return ServiceResult<IQueryable<Exercise>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(FailedToActionStr("user exercises", "get", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateExerciseAsync(Exercise exercise)
    {
        try
        {
            if (exercise is null)
                throw exerciseIsNullException;

            if (exercise.Id < 1)
                throw invalidExerciseIDException;

            var _exercise = await baseWorkoutRepository.GetByIdAsync(exercise.Id) ?? throw exerciseNotFoundException;

            if (!string.IsNullOrEmpty(_exercise.CreatedByUserId))
                throw UserNotHavePermissionException("update", "exercise");

            await baseWorkoutRepository.UpdateAsync(exercise);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("exercise", "update", ex.Message));
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

            var _exercise = await baseWorkoutRepository.GetByIdAsync(exercise.Id) ?? throw exerciseNotFoundException;


            if (_exercise.CreatedByUserId != userId)
                throw UserNotHavePermissionException("update", "exercise");

            await baseWorkoutRepository.UpdateAsync(exercise);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("user exercise", "update", ex.Message));
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
