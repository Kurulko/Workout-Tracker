using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;
using WorkoutTrackerAPI.Services.WorkoutServices;

namespace WorkoutTrackerAPI.Services;

public class WorkoutService : BaseWorkoutService<Workout>, IWorkoutService
{
    readonly UserRepository userRepository;
    readonly ExerciseSetRepository exerciseSetRepository;
    readonly ExerciseSetGroupRepository exerciseSetGroupRepository;
    readonly ExerciseRecordRepository exerciseRecordRepository;
    readonly ExerciseRecordGroupRepository exerciseRecordGroupRepository;
    readonly WorkoutRecordRepository workoutRecordRepository;
    public WorkoutService(WorkoutRepository baseWorkoutRepository, 
        ExerciseSetRepository exerciseSetRepository, 
        ExerciseSetGroupRepository exerciseSetGroupRepository, 
        ExerciseRecordRepository exerciseRecordRepository, 
        ExerciseRecordGroupRepository exerciseRecordGroupRepository,
        WorkoutRecordRepository workoutRecordRepository, 
        UserRepository userRepository) : base(baseWorkoutRepository)
    {
        this.userRepository = userRepository;
        this.exerciseSetRepository = exerciseSetRepository;
        this.exerciseSetGroupRepository = exerciseSetGroupRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.exerciseRecordGroupRepository = exerciseRecordGroupRepository;
        this.workoutRecordRepository = workoutRecordRepository;
    }

    readonly EntryNullException workoutIsNullException = new(nameof(Workout));
    readonly InvalidIDException invalidWorkoutIDException = new(nameof(Workout));
    readonly ArgumentNullOrEmptyException workoutNameIsNullOrEmptyException = new("Workout name");

    NotFoundException WorkoutNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID(nameof(Workout), id);

    public async Task<ServiceResult<Workout>> AddUserWorkoutAsync(string userId, Workout workout)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workout is null)
                throw workoutIsNullException;

            if (workout.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(Workout), "workout");

            if (await baseWorkoutRepository.ExistsByNameAsync(workout.Name))
                throw EntryNameMustBeUnique(nameof(Workout));

            Workout _workout = new()
            {
                UserId = userId,
                Created = DateTime.Now,
                Name = workout.Name,
                Description = workout.Description,
            };

            await baseWorkoutRepository.AddAsync(_workout);
            return ServiceResult<Workout>.Ok(_workout);
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

            var workout = await baseWorkoutRepository.GetByIdAsync(workoutId) ?? throw WorkoutNotFoundByIDException(workoutId);

            if (workout.UserId != userId)
                throw UserNotHavePermissionException("delete", "workout");

            await baseWorkoutRepository.RemoveAsync(workoutId);
            await UpdateUserFirstWorkoutDate(userId);

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
    
    async Task AddExerciseSetGroups(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups)
    {
        foreach (var exerciseSetGroup in exerciseSetGroups)
        {
            var exerciseSets = exerciseSetGroup.ExerciseSets;

            exerciseSetGroup.WorkoutId = workoutId;
            exerciseSetGroup.ExerciseSets = null!;
            await exerciseSetGroupRepository.AddAsync(exerciseSetGroup);

            foreach (var exerciseSet in exerciseSets)
            {
                exerciseSet.ExerciseSetGroupId = exerciseSetGroup.Id;
                exerciseSet.ExerciseId = exerciseSetGroup.ExerciseId;
                exerciseSet.UserId = userId;
                await exerciseSetRepository.AddAsync(exerciseSet);
            }
        }
    }

    public async Task<ServiceResult> AddExerciseSetGroupsToUserWorkoutAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutId < 1)
                throw invalidWorkoutIDException;

            var workout = await baseWorkoutRepository.GetByIdAsync(workoutId) ?? throw WorkoutNotFoundByIDException(workoutId);

            if (workout.UserId != userId)
                throw UserNotHavePermissionException("add", "exercise sets to workout");

            await AddExerciseSetGroups(userId, workoutId, exerciseSetGroups);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record groups", "add"));
        }
    }

    async Task DeleteExerciseSetGroups(IEnumerable<ExerciseSetGroup> exerciseSetGroups)
    {
        await exerciseSetGroupRepository.RemoveRangeAsync(exerciseSetGroups);

        //foreach (var exerciseSetGroup in exerciseSetGroups)
        //{
        //    var exerciseSets = exerciseSetGroup.ExerciseSets;

        //    await exerciseSetGroupRepository.RemoveAsync(exerciseSetGroup.Id);

        //    //await exerciseSetRepository.RemoveRangeAsync(exerciseSetGroup.ExerciseSets);
        //    foreach (var exerciseSet in exerciseSets)
        //    {
        //        await exerciseSetRepository.RemoveAsync(exerciseSet.Id);
        //    }
        //}
    }

    public async Task<ServiceResult> UpdateUserWorkoutExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutId < 1)
                throw invalidWorkoutIDException;

            var workout = await baseWorkoutRepository.GetByIdAsync(workoutId) ?? throw WorkoutNotFoundByIDException(workoutId);

            if (workout.UserId != userId)
                throw UserNotHavePermissionException("update", "workout exercise sets");

            if(workout.ExerciseSetGroups is IEnumerable<ExerciseSetGroup> _exerciseSetGroups)
                await DeleteExerciseSetGroups(_exerciseSetGroups);

            await AddExerciseSetGroups(userId, workoutId, exerciseSetGroups);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record groups", "update"));
        }
    }

    ExerciseRecord ToExerciseRecord(ExerciseSet exerciseSet, DateTime date, long exerciseRecordGroupId)
    {
        ExerciseRecord exerciseRecord = new()
        {
            Date = date,
            ExerciseId = exerciseSet.ExerciseId,
            ExerciseRecordGroupId = exerciseRecordGroupId,
            UserId = exerciseSet.UserId,
            Reps = exerciseSet.Reps,
            Weight = exerciseSet.Weight,
            Time = exerciseSet.Time,
        };

        return exerciseRecord;
    }

    ExerciseRecordGroup ToExerciseRecordGroup(ExerciseSetGroup exerciseSetGroup, long workoutRecordId)
    {
        ExerciseRecordGroup exerciseRecordGroup = new()
        {
            ExerciseId = exerciseSetGroup.ExerciseId,
            WorkoutRecordId = workoutRecordId,
        };

        return exerciseRecordGroup;
    }

    public async Task<ServiceResult> CompleteUserWorkout(string userId, long workoutId, DateTime date, TimeSpan time)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutId < 1)
                throw invalidWorkoutIDException;

            var workout = await baseWorkoutRepository.GetByIdAsync(workoutId) ?? throw WorkoutNotFoundByIDException(workoutId);

            if (workout.UserId != userId)
                throw UserNotHavePermissionException("complete", "workout");

            if (workout.ExerciseSetGroups.CountOrDefault() == 0)
                throw new ArgumentException("Workout must have exercises to be completed.");

            WorkoutRecord workoutRecord = new()
            {
                Date = date,
                Time = time,
                WorkoutId = workoutId,
                UserId = userId
            };
            await workoutRecordRepository.AddAsync(workoutRecord);

            foreach (var exerciseSetGroup in workout.ExerciseSetGroups!)
            {
                var exerciseRecordGroup = ToExerciseRecordGroup(exerciseSetGroup, workoutRecord.Id);
                await exerciseRecordGroupRepository.AddAsync(exerciseRecordGroup);

                foreach (var exerciseSet in exerciseSetGroup.ExerciseSets)
                {
                    var exerciseRecord = ToExerciseRecord(exerciseSet, date, exerciseRecordGroup.Id);
                    await exerciseRecordRepository.AddAsync(exerciseRecord);
                }
            }

            workout.CountOfTrainings++;
            await baseWorkoutRepository.UpdateAsync(workout);
            await UpdateUserFirstWorkoutDate(userId);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record groups", "add"));
        }
    }

    async Task<ServiceResult> ChangeUserPinnedWorkout(string userId, long workoutId, bool isPinned)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutId < 1)
                throw invalidWorkoutIDException;

            var workout = await baseWorkoutRepository.GetByIdAsync(workoutId) ?? throw WorkoutNotFoundByIDException(workoutId);

            if (workout.UserId != userId)
                throw UserNotHavePermissionException("pin", "workout");

            workout.IsPinned = isPinned;
            await baseWorkoutRepository.UpdateAsync(workout);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record groups", "add"));
        }
    }

    public async Task<ServiceResult> PinUserWorkout(string userId, long workoutId)
        => await ChangeUserPinnedWorkout(userId, workoutId, true);

    public async Task<ServiceResult> UnpinUserWorkout(string userId, long workoutId)
        => await ChangeUserPinnedWorkout(userId, workoutId, false);

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

    public async Task<ServiceResult<IQueryable<Workout>>> GetUserWorkoutsAsync(string userId, long? exerciseId = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseId is long _exerciseId && _exerciseId < 1)
                    throw new InvalidIDException(nameof(Exercise));

            var userWorkouts = await baseWorkoutRepository.FindAsync(e => e.UserId == userId);

            if (exerciseId.HasValue)
                userWorkouts = userWorkouts
                    .Where(w => w.ExerciseSetGroups!.Any(s => s.ExerciseId == exerciseId))
                    .Include(w => w.ExerciseSetGroups!.Where(s => s.ExerciseId == exerciseId))
                    .ThenInclude(s => s.Exercise);

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

            var _workout = await baseWorkoutRepository.GetByIdAsync(workout.Id) ?? throw WorkoutNotFoundByIDException(workout.Id);

            if (_workout.UserId != userId)
                throw UserNotHavePermissionException("update", "workout");

            _workout.Name = workout.Name;
            _workout.Description = workout.Description;
            //_workout.CountOfTrainings = workout.CountOfTrainings;
            //_workout.SumOfWeight = workout.SumOfWeight;
            //_workout.SumOfTime = workout.SumOfTime;

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

    async Task UpdateUserFirstWorkoutDate(string userId)
    {
        var user = (await userRepository.GetUserByIdAsync(userId))!;
        var userWorkoutRecords = await userRepository.GetUserWorkoutRecordsAsync(userId);

        var firstWorkoutDate = userWorkoutRecords?.MinBy(wr => wr.Date)?.Date;
        user.StartedWorkingOut = firstWorkoutDate;
        await userRepository.UpdateUserAsync(user);
    }
}
