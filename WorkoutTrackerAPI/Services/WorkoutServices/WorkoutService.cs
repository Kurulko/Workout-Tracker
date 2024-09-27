using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.WorkoutServices;

namespace WorkoutTrackerAPI.Services;

public class WorkoutService : BaseWorkoutService<Workout>, IWorkoutService
{
    readonly UserRepository userRepository;
    public WorkoutService(WorkoutRepository baseWorkoutRepository, UserRepository userRepository) : base(baseWorkoutRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException workoutIsNullException = new (nameof(Workout));
    readonly InvalidIDException invalidWorkoutIDException = new (nameof(Workout));
    readonly NotFoundException workoutNotFoundException = new (nameof(Workout));
    readonly ArgumentNullOrEmptyException workoutNameIsNullOrEmptyException = new("Workout name");


    public async Task<ServiceResult<Workout>> AddUserWorkoutAsync(string userId, Workout workout)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<Workout>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<Workout>.Fail(userNotFoundException);

        if (workout is null)
            return ServiceResult<Workout>.Fail(workoutIsNullException);

        if (workout.Id != 0)
            return ServiceResult<Workout>.Fail(InvalidEntryIDWhileAddingStr(nameof(Workout), "workout"));

        try
        {
            workout.UserId = userId;
            await baseWorkoutRepository.AddAsync(workout);

            return ServiceResult<Workout>.Ok(workout);
        }
        catch (Exception ex)
        {
            return ServiceResult<Workout>.Fail(FailedToAction("workout", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteUserWorkoutAsync(string userId, long workoutId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (workoutId < 1)
            return ServiceResult.Fail(invalidWorkoutIDException);

        Workout? workout = await baseWorkoutRepository.GetByIdAsync(workoutId);

        if (workout is null)
            return ServiceResult.Fail(workoutNotFoundException);

        if (workout.UserId != userId)
            return ServiceResult.Fail(UserNotHavePermissionStr("delete", "workout"));

        try
        {
            await baseWorkoutRepository.RemoveAsync(workoutId);
            return ServiceResult.Ok();
        }
        catch
        {
            return ServiceResult.Fail(FailedToAction("workout", "delete"));
        }
    }

    public async Task<ServiceResult<Workout>> GetUserWorkoutByIdAsync(string userId, long workoutId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<Workout>.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<Workout>.Fail(userNotFoundException);

        if (workoutId < 1)
            return ServiceResult<Workout>.Fail(invalidWorkoutIDException);

        try
        {
            var userWorkoutById = await baseWorkoutRepository.GetByIdAsync(workoutId);
            return ServiceResult<Workout>.Ok(userWorkoutById);
        }
        catch (Exception ex)
        {
            return ServiceResult<Workout>.Fail(FailedToAction("workout", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Workout>> GetUserWorkoutByNameAsync(string userId, string name)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<Workout>.Fail(userIdIsNullOrEmptyException);

        if (string.IsNullOrEmpty(name))
            return ServiceResult<Workout>.Fail(workoutNameIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<Workout>.Fail(userNotFoundException);

        try
        {
            var userWorkoutByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Workout>.Ok(userWorkoutByName);
        }
        catch (Exception ex)
        {
            return ServiceResult<Workout>.Fail(FailedToAction("workout by name", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<IQueryable<Workout>>> GetUserWorkoutsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<IQueryable<Workout>>.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<IQueryable<Workout>>.Fail(userNotFoundException);

        try
        {
            var userWorkouts = await baseWorkoutRepository.FindAsync(e => e.UserId == userId);
            return ServiceResult<IQueryable<Workout>>.Ok(userWorkouts);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Workout>>.Fail(FailedToAction("workouts", "get", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateUserWorkoutAsync(string userId, Workout workout)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (workout is null)
            return ServiceResult.Fail(workoutIsNullException);

        if (workout.Id < 1)
            return ServiceResult.Fail(invalidWorkoutIDException);

        try
        {
            Workout? _workout = await baseWorkoutRepository.GetByIdAsync(workout.Id);

            if (_workout is null)
                return ServiceResult.Fail(workoutNotFoundException);

            if (_workout.UserId != userId)
                return ServiceResult.Fail(UserNotHavePermissionStr("update", "workout"));

            await baseWorkoutRepository.UpdateAsync(workout);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToAction("workout", "update", ex.Message));
        }
    }

    public async Task<bool> UserWorkoutExistsAsync(string userId, long workoutId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

       if (!(await userRepository.UserExistsAsync(userId)))
            throw userNotFoundException;

        if (workoutId < 1)
            throw invalidWorkoutIDException;

        return await baseWorkoutRepository.ExistsAsync(workoutId);
    }

    public async Task<bool> UserWorkoutExistsByNameAsync(string userId, string name)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

       if (!(await userRepository.UserExistsAsync(userId)))
            throw userNotFoundException;

        if (string.IsNullOrEmpty(userId))
            throw workoutNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }
}
