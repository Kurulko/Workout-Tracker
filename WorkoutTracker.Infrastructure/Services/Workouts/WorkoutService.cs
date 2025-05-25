using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions.Exercises;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Validators.Services.Workouts;
using WorkoutTracker.Infrastructure.Extensions;

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
    readonly WorkoutServiceValidator workoutServiceValidator;

    public WorkoutService(
        IWorkoutRepository workoutRepository,
        IExerciseSetRepository exerciseSetRepository,
        IExerciseSetGroupRepository exerciseSetGroupRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IExerciseRecordGroupRepository exerciseRecordGroupRepository,
        IWorkoutRecordRepository workoutRecordRepository,
        IUserRepository userRepository,
        WorkoutServiceValidator workoutServiceValidator,
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
        this.workoutServiceValidator = workoutServiceValidator;
    }

    const string workoutEntityName = "workout";

    public async Task<Workout> AddUserWorkoutAsync(string userId, Workout workout)
    {
        await workoutServiceValidator.ValidateAddAsync(userId, workout);

        Workout _workout = new()
        {
            UserId = userId,
            Created = DateTime.UtcNow,
            Name = workout.Name,
            Description = workout.Description,
        };

        return await baseWorkoutRepository.AddAsync(_workout)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "add", userId));
    }

    public async Task UpdateUserWorkoutAsync(string userId, Workout workout)
    {
        await workoutServiceValidator.ValidateUpdateAsync(userId, workout);

        var _workout = (await baseWorkoutRepository.GetByIdAsync(workout.Id))!;

        _workout.Name = workout.Name;
        _workout.Description = workout.Description;

        await baseWorkoutRepository.UpdateAsync(_workout)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "update", userId));
    }


    public async Task DeleteUserWorkoutAsync(string userId, long workoutId)
    {
        await workoutServiceValidator.ValidateDeleteAsync(userId, workoutId);

        await baseWorkoutRepository.RemoveAsync(workoutId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "remove", userId));

        await UpdateUserFirstWorkoutDate(userId);
    }


    public async Task<Workout?> GetUserWorkoutByIdAsync(string userId, long workoutId, bool withDetails = false)
    {
        await workoutServiceValidator.ValidateGetByIdAsync(userId, workoutId);

        return await (withDetails
            ? workoutRepository.GetWorkoutByIdWithDetailsAsync(workoutId)
            : baseWorkoutRepository.GetByIdAsync(workoutId)
        ).LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "get", userId));
    }

    public async Task<Workout?> GetUserWorkoutByNameAsync(string userId, string name, bool withDetails = false)
    {
        await workoutServiceValidator.ValidateGetByNameAsync(userId, name);

        return await (withDetails
            ? workoutRepository.GetWorkoutByNameWithDetailsAsync(name)
            : baseWorkoutRepository.GetByNameAsync(name)
        ).LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "get", userId));

    }

    public async Task<IQueryable<Workout>> GetUserWorkoutsAsync(string userId, long? exerciseId = null)
    {
        await workoutServiceValidator.ValidateGetAllAsync(userId, exerciseId);

        var userWorkouts = await baseWorkoutRepository.FindAsync(e => e.UserId == userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("workouts", "get", userId));

        if (exerciseId.HasValue)
        {
            userWorkouts = userWorkouts
                .Where(w => w.ExerciseSetGroups!.Any(s => s.ExerciseId == exerciseId))
                .Include(w => w.ExerciseSetGroups!.Where(s => s.ExerciseId == exerciseId))
                .ThenInclude(s => s.Exercise);
        }

        return userWorkouts;
    }


    public async Task AddExerciseSetGroupsToUserWorkoutAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups)
    {
        await workoutServiceValidator.ValidateAddExerciseSetGroupsAsync(userId, workoutId, exerciseSetGroups);

        await AddExerciseSetGroups(userId, workoutId, exerciseSetGroups);
    }

    public async Task UpdateUserWorkoutExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups)
    {
        await workoutServiceValidator.ValidateUpdateExerciseSetGroupsAsync(userId, workoutId, exerciseSetGroups);

        var workout = (await baseWorkoutRepository.GetByIdAsync(workoutId))!;

        if (workout.ExerciseSetGroups is IEnumerable<ExerciseSetGroup> _exerciseSetGroups)
            await DeleteExerciseSetGroups(_exerciseSetGroups, userId);

        await AddExerciseSetGroups(userId, workoutId, exerciseSetGroups);
    }


    public async Task PinUserWorkout(string userId, long workoutId)
        => await ChangeUserPinnedWorkout(userId, workoutId, true);

    public async Task UnpinUserWorkout(string userId, long workoutId)
        => await ChangeUserPinnedWorkout(userId, workoutId, false);

    public async Task CompleteUserWorkout(string userId, long workoutId, DateTime date, TimeSpan time)
    {
        await workoutServiceValidator.ValidateCompleteAsync(userId, workoutId, date, time);

        var workout = (await baseWorkoutRepository.GetByIdAsync(workoutId))!;

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
    }


    async Task ChangeUserPinnedWorkout(string userId, long workoutId, bool isPinned)
    {
        await workoutServiceValidator.ValidateUpdatePinnedAsync(userId, workoutId);

        var workout = (await baseWorkoutRepository.GetByIdAsync(workoutId))!;

        workout.IsPinned = isPinned;

        await baseWorkoutRepository.UpdateAsync(workout)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("pinned workout", "change", userId));
    }

    async Task AddExerciseSetGroups(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups)
    {
        foreach (var exerciseSetGroup in exerciseSetGroups)
        {
            var exerciseSets = exerciseSetGroup.ExerciseSets;

            exerciseSetGroup.WorkoutId = workoutId;
            exerciseSetGroup.ExerciseSets = null!;

            await exerciseSetGroupRepository.AddAsync(exerciseSetGroup)
                .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise set group", "add", userId));

            foreach (var exerciseSet in exerciseSets)
            {
                exerciseSet.ExerciseSetGroupId = exerciseSetGroup.Id;
                exerciseSet.ExerciseId = exerciseSetGroup.ExerciseId;

                await exerciseSetRepository.AddAsync(exerciseSet)
                    .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise set", "add", userId));
            }
        }
    }

    async Task DeleteExerciseSetGroups(IEnumerable<ExerciseSetGroup> exerciseSetGroups, string userId)
    {
        await exerciseSetGroupRepository.RemoveRangeAsync(exerciseSetGroups)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise set groups", "remove", userId));

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

    async Task UpdateUserFirstWorkoutDate(string userId)
    {
        var user = (await userRepository.GetUserByIdAsync(userId))!;
        var userWorkoutRecords = await userRepository.GetUserWorkoutRecordsAsync(userId);

        var firstWorkoutDate = userWorkoutRecords?.MinBy(wr => wr.Date)?.Date;
        user.StartedWorkingOut = firstWorkoutDate;

        await userRepository.UpdateUserAsync(user)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("user first workout date", "update", userId));
    }
}
