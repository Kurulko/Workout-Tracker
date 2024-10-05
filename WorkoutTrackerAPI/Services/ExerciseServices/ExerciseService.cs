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

    string invalidExerciseIDWhileAddingStr => InvalidEntryIDWhileAddingStr(nameof(Exercise), "exercise");

    public async Task<ServiceResult<Exercise>> AddExerciseAsync(Exercise exercise)
    {
        if (exercise is null)
            return ServiceResult<Exercise>.Fail(exerciseIsNullException);

        if (exercise.IsCreatedByUser)
            return ServiceResult<Exercise>.Fail("Exercise entry cannot be created by user.");

        if (exercise.Id != 0)
            return ServiceResult<Exercise>.Fail(invalidExerciseIDWhileAddingStr);

        try
        {
            await baseWorkoutRepository.AddAsync(exercise);
            return ServiceResult<Exercise>.Ok(exercise);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToAction("exercise", "add", ex.Message));
        }
    }

    public async Task<ServiceResult<Exercise>> AddUserExerciseAsync(string userId, Exercise exercise)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<Exercise>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<Exercise>.Fail(userNotFoundException);

        if (exercise is null)
            return ServiceResult<Exercise>.Fail(exerciseIsNullException);

        if (exercise.Id != 0)
            return ServiceResult<Exercise>.Fail(invalidExerciseIDWhileAddingStr);

        try
        {
            exercise.CreatedByUserId = userId;
            await baseWorkoutRepository.AddAsync(exercise);

            return ServiceResult<Exercise>.Ok(exercise);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToAction("user exercise", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteExerciseAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return ServiceResult.Fail(invalidExerciseIDException);

        Exercise? exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId);

        if (exercise is null)
            return ServiceResult.Fail(exerciseNotFoundException);

        if (exercise.IsCreatedByUser)
            return ServiceResult.Fail(UserNotHavePermissionStr("delete", "exercise"));

        try
        {
            await baseWorkoutRepository.RemoveAsync(exerciseId);
            return ServiceResult.Ok();
        }
        catch
        {
            return ServiceResult.Fail(FailedToAction("exercise", "delete"));
        }
    }

    public async Task<ServiceResult> DeleteExerciseFromUserAsync(string userId, long exerciseId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (exerciseId < 1)
            return ServiceResult.Fail(invalidExerciseIDException);

        Exercise? exercise = await baseWorkoutRepository.GetByIdAsync(exerciseId);

        if (exercise is null)
            return ServiceResult.Fail(exerciseNotFoundException);

        if (exercise.CreatedByUserId != userId)
            return ServiceResult.Fail(UserNotHavePermissionStr("delete", "exercise"));

        try
        {
            await baseWorkoutRepository.RemoveAsync(exerciseId);
            return ServiceResult.Ok();
        }
        catch
        {
            return ServiceResult.Fail(FailedToAction("user exercise", "delete"));
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
        if (exerciseId < 1)
            return ServiceResult<Exercise>.Fail(invalidExerciseIDException);

        try
        {
            var exerciseById = await baseWorkoutRepository.GetByIdAsync(exerciseId);
            return ServiceResult<Exercise>.Ok(exerciseById);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToAction("exercise", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Exercise>> GetExerciseByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ServiceResult<Exercise>.Fail(exerciseNameIsNullOrEmptyException);

        try
        {
            var exerciseByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Exercise>.Ok(exerciseByName);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToAction("exercise by name", "get", ex.Message));
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
            return ServiceResult<IQueryable<Exercise>>.Fail(FailedToAction("exercises", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Exercise>> GetUserExerciseByIdAsync(string userId, long exerciseId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<Exercise>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<Exercise>.Fail(userNotFoundException);

        if (exerciseId < 1)
            return ServiceResult<Exercise>.Fail(invalidExerciseIDException);

        try
        {
            var userExerciseById = await baseWorkoutRepository.GetByIdAsync(exerciseId);
            return ServiceResult<Exercise>.Ok(userExerciseById);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToAction("user exercise", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Exercise>> GetUserExerciseByNameAsync(string userId, string name)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<Exercise>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<Exercise>.Fail(userNotFoundException);

        if (string.IsNullOrEmpty(name))
            return ServiceResult<Exercise>.Fail(exerciseNameIsNullOrEmptyException);

        try
        {
            var userExerciseByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Exercise>.Ok(userExerciseByName);
        }
        catch (Exception ex)
        {
            return ServiceResult<Exercise>.Fail(FailedToAction("user exercise by name", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<IQueryable<Exercise>>> GetUserExercisesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<IQueryable<Exercise>>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<IQueryable<Exercise>>.Fail(userNotFoundException);

        try
        {
            var userExercises = await baseWorkoutRepository.FindAsync(e => e.CreatedByUserId == userId);
            return ServiceResult<IQueryable<Exercise>>.Ok(userExercises);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Exercise>>.Fail(FailedToAction("user exercises", "get", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateExerciseAsync(Exercise exercise)
    {
        if (exercise is null)
            return ServiceResult.Fail(exerciseIsNullException);

        if (exercise.Id < 1)
            return ServiceResult.Fail(invalidExerciseIDException);

        try
        {
            Exercise? _exercise = await baseWorkoutRepository.GetByIdAsync(exercise.Id);

            if (_exercise is null)
                return ServiceResult.Fail(new NotFoundException(nameof(Exercise)));

            if (_exercise.IsCreatedByUser)
                return ServiceResult.Fail(UserNotHavePermissionStr("update", "exercise"));

            await baseWorkoutRepository.UpdateAsync(exercise);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToAction("exercise", "update", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateUserExerciseAsync(string userId, Exercise exercise)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (exercise is null)
            return ServiceResult.Fail(exerciseIsNullException);

        if (exercise.Id < 1)
            return ServiceResult.Fail(invalidExerciseIDException);

        try
        {
            Exercise? _exercise = await baseWorkoutRepository.GetByIdAsync(exercise.Id);

            if (_exercise is null)
                return ServiceResult.Fail(exerciseNotFoundException);

            if (_exercise.CreatedByUserId != userId)
                return ServiceResult.Fail(UserNotHavePermissionStr("update", "exercise"));

            await baseWorkoutRepository.UpdateAsync(exercise);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToAction("user exercise", "update", ex.Message));
        }
    }

    public async Task<bool> UserExerciseExistsAsync(string userId, long exerciseId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (!(await userRepository.UserExistsAsync(userId)))
            throw userNotFoundException;

        if (exerciseId < 1)
            throw invalidExerciseIDException;

        return await baseWorkoutRepository.ExistsAsync(exerciseId);
    }

    public async Task<bool> UserExerciseExistsByNameAsync(string userId, string name)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (!(await userRepository.UserExistsAsync(userId)))
            throw userNotFoundException;

        if (string.IsNullOrEmpty(name))
            throw exerciseNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }
}
