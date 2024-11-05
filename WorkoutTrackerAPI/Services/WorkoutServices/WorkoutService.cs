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

    readonly EntryNullException workoutIsNullException = new(nameof(Workout));
    readonly InvalidIDException invalidWorkoutIDException = new(nameof(Workout));
    readonly NotFoundException workoutNotFoundException = new(nameof(Workout));
    readonly ArgumentNullOrEmptyException workoutNameIsNullOrEmptyException = new("Workout name");


    public async Task<ServiceResult<Workout>> AddUserWorkoutAsync(string userId, Workout workout)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workout is null)
                throw workoutIsNullException;

            workout.UserId = userId;

            workout.CountOfTrainings++;

            workout.SumOfWeight += workout.Weight;
            workout.SumOfTime += workout.Time;

            if (workout.Id == 0)
            {
                workout.Created = workout.Started = DateTime.Now;
                await baseWorkoutRepository.AddAsync(workout);
            }
            else
            {
                await UpdateUserWorkoutAsync(userId, workout);
            }

            return ServiceResult<Workout>.Ok(workout);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Workout>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Workout>.Fail(FailedToActionStr("workout", "add", ex));
        }
    }

    public async Task<ServiceResult> DeleteUserWorkoutAsync(string userId, long workoutId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutId < 1)
                throw invalidWorkoutIDException;

            var workout = await baseWorkoutRepository.GetByIdAsync(workoutId) ?? throw workoutNotFoundException;

            if (workout.UserId != userId)
                throw UserNotHavePermissionException("delete", "workout");

            await baseWorkoutRepository.RemoveAsync(workoutId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("workout", "delete"));
        }
    }

    public async Task<ServiceResult<Workout>> GetUserWorkoutByIdAsync(string userId, long workoutId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutId < 1)
                throw invalidWorkoutIDException;

            var userWorkoutById = await baseWorkoutRepository.GetByIdAsync(workoutId);
            return ServiceResult<Workout>.Ok(userWorkoutById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Workout>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Workout>.Fail(FailedToActionStr("workout", "get", ex));
        }
    }

    public async Task<ServiceResult<Workout>> GetUserWorkoutByNameAsync(string userId, string name)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw workoutNameIsNullOrEmptyException;

            var userWorkoutByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Workout>.Ok(userWorkoutByName);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Workout>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Workout>.Fail(FailedToActionStr("workout by name", "get", ex));
        }
    }

    public async Task<ServiceResult<IQueryable<Workout>>> GetUserWorkoutsAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userWorkouts = await baseWorkoutRepository.FindAsync(e => e.UserId == userId);
            return ServiceResult<IQueryable<Workout>>.Ok(userWorkouts);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<Workout>>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Workout>>.Fail(FailedToActionStr("workouts", "get", ex));
        }
    }

    public async Task<ServiceResult> UpdateUserWorkoutAsync(string userId, Workout workout)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workout is null)
                throw workoutIsNullException;

            if (workout.Id < 1)
                throw invalidWorkoutIDException;

            var _workout = await baseWorkoutRepository.GetByIdAsync(workout.Id) ?? throw workoutNotFoundException;

            if (_workout.UserId != userId)
                throw UserNotHavePermissionException("update", "workout");

            _workout.Name = workout.Name;
            _workout.CountOfTrainings = workout.CountOfTrainings;
            _workout.Weight = workout.Weight;
            _workout.Time = workout.Time;
            _workout.SumOfWeight = workout.SumOfWeight;
            _workout.SumOfTime = workout.SumOfTime;
            _workout.Exercises = workout.Exercises;

            await baseWorkoutRepository.UpdateAsync(_workout);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("workout", "update", ex));
        }
    }

    public async Task<bool> UserWorkoutExistsAsync(string userId, long workoutId)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (workoutId < 1)
            throw invalidWorkoutIDException;

        return await baseWorkoutRepository.ExistsAsync(workoutId);
    }

    public async Task<bool> UserWorkoutExistsByNameAsync(string userId, string name)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (string.IsNullOrEmpty(name))
            throw workoutNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }
}
