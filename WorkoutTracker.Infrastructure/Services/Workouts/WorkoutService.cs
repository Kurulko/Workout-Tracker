using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions.Exercises;
using Microsoft.Extensions.Logging;

namespace WorkoutTracker.Infrastructure.Services.Workouts;

internal class WorkoutService : BaseWorkoutService<WorkoutService, Workout>, IWorkoutService
{
    readonly IUserRepository userRepository;
    readonly IExerciseSetRepository exerciseSetRepository;
    readonly IExerciseSetGroupRepository exerciseSetGroupRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly IExerciseRecordGroupRepository exerciseRecordGroupRepository;
    readonly IWorkoutRecordRepository workoutRecordRepository;
    readonly IWorkoutRepository workoutRepository;
    public WorkoutService(
        IWorkoutRepository workoutRepository,
        IExerciseSetRepository exerciseSetRepository,
        IExerciseSetGroupRepository exerciseSetGroupRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IExerciseRecordGroupRepository exerciseRecordGroupRepository,
        IWorkoutRecordRepository workoutRecordRepository,
        IUserRepository userRepository,
        ILogger<WorkoutService> logger

    ) : base(workoutRepository, logger)
    {
        this.workoutRepository = workoutRepository;
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

    ArgumentException WorkoutNameMustBeUnique()
        => EntryNameMustBeUnique(nameof(Workout));

    public async Task<ServiceResult<Workout>> AddUserWorkoutAsync(string userId, Workout workout)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workout is null)
                throw workoutIsNullException;

            if (workout.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(Workout), "workout");

            var isUniqueWorkoutName = await IsUniqueWorkoutNameForUserAsync(workout.Name, userId);
            if (!isUniqueWorkoutName)
                throw WorkoutNameMustBeUnique();

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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Workout>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("workout", "add", userId));
            throw;
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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("workout", "delete", userId));
            throw;
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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise record groups", "add", userId));
            throw;
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

            if (workout.ExerciseSetGroups is IEnumerable<ExerciseSetGroup> _exerciseSetGroups)
                await DeleteExerciseSetGroups(_exerciseSetGroups);

            await AddExerciseSetGroups(userId, workoutId, exerciseSetGroups);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise record groups", "update", userId));
            throw;
        }
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
                var exerciseRecordGroup = exerciseSetGroup.ToExerciseRecordGroup(workoutRecord.Id);
                await exerciseRecordGroupRepository.AddAsync(exerciseRecordGroup);

                foreach (var exerciseSet in exerciseSetGroup.ExerciseSets)
                {
                    var exerciseRecord = exerciseSet.ToExerciseRecord(date, exerciseRecordGroup.Id);
                    await exerciseRecordRepository.AddAsync(exerciseRecord);
                }
            }

            workout.CountOfTrainings++;
            await baseWorkoutRepository.UpdateAsync(workout);
            await UpdateUserFirstWorkoutDate(userId);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("workout", "complete", userId));
            throw;
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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("pinned workout", "change", userId));
            throw;
        }
    }

    public async Task<ServiceResult> PinUserWorkout(string userId, long workoutId)
        => await ChangeUserPinnedWorkout(userId, workoutId, true);

    public async Task<ServiceResult> UnpinUserWorkout(string userId, long workoutId)
        => await ChangeUserPinnedWorkout(userId, workoutId, false);

    public async Task<ServiceResult<Workout>> GetUserWorkoutByIdAsync(string userId, long workoutId, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutId < 1)
                throw invalidWorkoutIDException;

            var userWorkoutById = withDetails ? await workoutRepository.GetWorkoutByIdWithDetailsAsync(workoutId) : await baseWorkoutRepository.GetByIdAsync(workoutId);
            return ServiceResult<Workout>.Ok(userWorkoutById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Workout>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("workout", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult<Workout>> GetUserWorkoutByNameAsync(string userId, string name, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw workoutNameIsNullOrEmptyException;

            var userWorkoutByName = withDetails ? await workoutRepository.GetWorkoutByNameWithDetailsAsync(name) : await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Workout>.Ok(userWorkoutByName);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Workout>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("workout by name", "get", userId));
            throw;
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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<Workout>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("workouts", "get", userId));
            throw;
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

            var isSameName = _workout.Name == workout.Name;
            var isUniqueWorkoutName = isSameName || await IsUniqueWorkoutNameForUserAsync(workout.Name, userId);
            if (!isUniqueWorkoutName)
                throw WorkoutNameMustBeUnique();

            _workout.Name = workout.Name;
            _workout.Description = workout.Description;

            await baseWorkoutRepository.UpdateAsync(_workout);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("workout", "update", userId));
            throw;
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

    async Task<bool> IsUniqueWorkoutNameForUserAsync(string name, string userId)
    {
        var isAnyWorkoutNames = await baseWorkoutRepository.AnyAsync(w => w.Name == name && (w.UserId == userId || w.UserId == null));
        return !isAnyWorkoutNames;
    }
}
