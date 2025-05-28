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

    public async Task<Workout> AddUserWorkoutAsync(string userId, Workout workout, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateAddAsync(userId, workout, cancellationToken);

        Workout _workout = new()
        {
            UserId = userId,
            Created = DateTime.UtcNow,
            Name = workout.Name,
            Description = workout.Description,
        };

        return await workoutRepository.AddAsync(_workout, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "add", userId));
    }

    public async Task UpdateUserWorkoutAsync(string userId, Workout workout, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateUpdateAsync(userId, workout, cancellationToken);

        var updateAction = new Action<Workout>(w =>
        {
            w.Name = workout.Name;
            w.Description = workout.Description;
        });

        await workoutRepository.UpdatePartialAsync(workout.Id, updateAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "update", userId));
    }


    public async Task DeleteUserWorkoutAsync(string userId, long workoutId, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateDeleteAsync(userId, workoutId, cancellationToken);

        await workoutRepository.RemoveAsync(workoutId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "remove", userId));

        await UpdateUserFirstWorkoutDateAsync(userId);
    }


    public async Task<Workout?> GetUserWorkoutByIdAsync(string userId, long workoutId, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateGetByIdAsync(userId, workoutId, cancellationToken);

        return await workoutRepository.GetByIdAsync(workoutId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "get", userId));
    }

    public async Task<Workout?> GetUserWorkoutByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateGetByNameAsync(userId, name, cancellationToken);

        return await workoutRepository.GetByNameAsync(name, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "get", userId));
    }

    public async Task<Workout?> GetUserWorkoutByIdWithDetailsAsync(string userId, long workoutId, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateGetByIdAsync(userId, workoutId, cancellationToken);

        return await workoutRepository.GetWorkoutByIdWithDetailsAsync(workoutId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "get", userId));
    }

    public async Task<Workout?> GetUserWorkoutByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateGetByNameAsync(userId, name, cancellationToken);

        return await workoutRepository.GetWorkoutByNameWithDetailsAsync(name, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutEntityName, "get", userId));
    }

    public async Task<IEnumerable<Workout>> GetUserWorkoutsAsync(string userId, long? exerciseId, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateGetAllAsync(userId, exerciseId, cancellationToken);

        var userWorkouts = workoutRepository.GetUserWorkouts(userId, exerciseId);

        return await userWorkouts.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("workouts", "get", userId));
    }


    public async Task AddExerciseSetGroupsToUserWorkoutAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateAddExerciseSetGroupsAsync(userId, workoutId, exerciseSetGroups, cancellationToken);

        await AddExerciseSetGroupsAsync(userId, workoutId, exerciseSetGroups, cancellationToken);
    }

    public async Task UpdateUserWorkoutExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateUpdateExerciseSetGroupsAsync(userId, workoutId, exerciseSetGroups, cancellationToken);

        var workout = (await workoutRepository.GetByIdAsync(workoutId, cancellationToken))!;

        if (workout.ExerciseSetGroups is IEnumerable<ExerciseSetGroup> _exerciseSetGroups)
            await DeleteExerciseSetGroupsAsync(_exerciseSetGroups, userId, cancellationToken);

        await AddExerciseSetGroupsAsync(userId, workoutId, exerciseSetGroups, cancellationToken);
    }


    public async Task PinUserWorkout(string userId, long workoutId, CancellationToken cancellationToken)
        => await ChangeUserPinnedWorkout(userId, workoutId, true, cancellationToken);

    public async Task UnpinUserWorkout(string userId, long workoutId, CancellationToken cancellationToken)
        => await ChangeUserPinnedWorkout(userId, workoutId, false, cancellationToken);

    public async Task CompleteUserWorkout(string userId, long workoutId, DateTime date, TimeSpan time, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateCompleteAsync(userId, workoutId, date, time, cancellationToken);

        WorkoutRecord workoutRecord = new()
        {
            Date = date,
            Time = time,
            WorkoutId = workoutId,
            UserId = userId
        };
        await workoutRecordRepository.AddAsync(workoutRecord, cancellationToken);

        var workout = (await workoutRepository.GetByIdAsync(workoutId, cancellationToken))!;

        await AddExerciseRecordGroupsAsync(userId, workoutRecord.Id, date, workout.ExerciseSetGroups!, cancellationToken);
        await workoutRepository.IncreaseCountOfWorkoutsAsync(workoutId, cancellationToken);
        await UpdateUserFirstWorkoutDateAsync(userId);
    }


    async Task ChangeUserPinnedWorkout(string userId, long workoutId, bool isPinned, CancellationToken cancellationToken)
    {
        await workoutServiceValidator.ValidateUpdatePinnedAsync(userId, workoutId, cancellationToken);

        var workout = (await workoutRepository.GetByIdAsync(workoutId, cancellationToken))!;

        workout.IsPinned = isPinned;

        await workoutRepository.UpdateAsync(workout, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("pinned workout", "change", userId));
    }

    async Task AddExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups, CancellationToken cancellationToken)
    {
        foreach (var exerciseSetGroup in exerciseSetGroups)
        {
            var exerciseSets = exerciseSetGroup.ExerciseSets;

            exerciseSetGroup.WorkoutId = workoutId;
            exerciseSetGroup.ExerciseSets = null!;

            await exerciseSetGroupRepository.AddAsync(exerciseSetGroup, cancellationToken)
                .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise set group", "add", userId));

            foreach (var exerciseSet in exerciseSets)
            {
                exerciseSet.ExerciseSetGroupId = exerciseSetGroup.Id;
                exerciseSet.ExerciseId = exerciseSetGroup.ExerciseId;

                await exerciseSetRepository.AddAsync(exerciseSet, cancellationToken)
                    .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise set", "add", userId));
            }
        }
    }

    async Task AddExerciseRecordGroupsAsync(string userId, long workoutRecordId, DateTime date, IEnumerable<ExerciseSetGroup> exerciseSetGroups, CancellationToken cancellationToken)
    {
        foreach (var exerciseSetGroup in exerciseSetGroups)
        {
            var exerciseRecordGroup = exerciseSetGroup.ToExerciseRecordGroup(workoutRecordId);
            await exerciseRecordGroupRepository.AddAsync(exerciseRecordGroup, cancellationToken)
                .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise record group", "add", userId));

            foreach (var exerciseSet in exerciseSetGroup.ExerciseSets)
            {
                var exerciseRecord = exerciseSet.ToExerciseRecord(date, exerciseRecordGroup.Id);
                await exerciseRecordRepository.AddAsync(exerciseRecord, cancellationToken)
                    .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise record", "add", userId));
            }
        }
    }

    async Task DeleteExerciseSetGroupsAsync(IEnumerable<ExerciseSetGroup> exerciseSetGroups, string userId, CancellationToken cancellationToken)
    {
        await exerciseSetGroupRepository.RemoveRangeAsync(exerciseSetGroups, cancellationToken)
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

    async Task UpdateUserFirstWorkoutDateAsync(string userId)
    {
        var user = (await userRepository.GetUserByIdAsync(userId))!;
        
        var firstWorkoutDate = await workoutRecordRepository.GetFirstWorkoutDateAsync(userId);
        user.StartedWorkingOut = firstWorkoutDate;

        await userRepository.UpdateUserAsync(user)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("user first workout date", "update", userId));
    }
}
