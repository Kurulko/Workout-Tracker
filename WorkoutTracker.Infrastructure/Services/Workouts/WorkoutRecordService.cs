using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Validators.Services.Workouts;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTracker.Infrastructure.Services.Workouts;

internal class WorkoutRecordService : DbModelService<WorkoutRecordService, WorkoutRecord>, IWorkoutRecordService
{
    readonly IUserRepository userRepository;
    readonly IWorkoutRecordRepository workoutRecordRepository;
    readonly IWorkoutRepository workoutRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly IExerciseRecordGroupRepository exerciseRecordGroupRepository;
    readonly WorkoutRecordServiceValidator workoutRecordServiceValidator;

    public WorkoutRecordService(
        IWorkoutRecordRepository workoutRecordRepository,
        IWorkoutRepository workoutRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IExerciseRecordGroupRepository exerciseRecordGroupRepository,
        IUserRepository userRepository,
        WorkoutRecordServiceValidator workoutRecordServiceValidator,
        ILogger<WorkoutRecordService> logger
    ) : base(workoutRecordRepository, logger)
    {
        this.workoutRepository = workoutRepository;
        this.workoutRecordRepository = workoutRecordRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.exerciseRecordGroupRepository = exerciseRecordGroupRepository;
        this.userRepository = userRepository;
        this.workoutRecordServiceValidator = workoutRecordServiceValidator;
    }

    const string workoutRecordEntityName = "workout record";

    public async Task<WorkoutRecord> AddWorkoutRecordToUserAsync(string userId, WorkoutRecord workoutRecord, CancellationToken cancellationToken)
    {
        await workoutRecordServiceValidator.ValidateAddAsync(userId, workoutRecord, cancellationToken);

        var exerciseRecordGroups = workoutRecord.ExerciseRecordGroups;

        workoutRecord.UserId = userId;
        workoutRecord.ExerciseRecordGroups = null!;

        await workoutRecordRepository.AddAsync(workoutRecord, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutRecordEntityName, "add", userId));

        await AddExerciseRecordGroupsAsync(userId, workoutRecord.Id, workoutRecord.Date, exerciseRecordGroups, cancellationToken);
        await workoutRepository.IncreaseCountOfWorkoutsAsync(workoutRecord.WorkoutId, cancellationToken);
        await UpdateUserFirstWorkoutDateAsync(userId);

        return workoutRecord;
    }

    public async Task UpdateUserWorkoutRecordAsync(string userId, WorkoutRecord workoutRecord, CancellationToken cancellationToken)
    {
        await workoutRecordServiceValidator.ValidateUpdateAsync(userId, workoutRecord, cancellationToken);

        var updateAction = new Action<WorkoutRecord>(wr =>
        {
            wr.Time = workoutRecord.Time;
            wr.Date = workoutRecord.Date;
            wr.WorkoutId = workoutRecord.WorkoutId;
            wr.ExerciseRecordGroups = null!;
        });

        await workoutRecordRepository.UpdatePartialAsync(workoutRecord.Id, updateAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutRecordEntityName, "update", userId));

        await UpdateUserFirstWorkoutDateAsync(userId);
        await AddExerciseRecordGroupsAsync(userId, workoutRecord.Id, workoutRecord.Date, workoutRecord.ExerciseRecordGroups, cancellationToken);
    }

    public async Task DeleteWorkoutRecordFromUserAsync(string userId, long workoutRecordId, CancellationToken cancellationToken)
    {
        await workoutRecordServiceValidator.ValidateDeleteAsync(userId, workoutRecordId, cancellationToken);

        var workoutRecord = (await workoutRecordRepository.GetByIdAsync(workoutRecordId))!;

        await workoutRecordRepository.RemoveAsync(workoutRecordId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutRecordEntityName, "delete", userId));

        await workoutRepository.DecreaseCountOfWorkoutsAsync(workoutRecord.WorkoutId, cancellationToken);
        await UpdateUserFirstWorkoutDateAsync(userId);
    }

    public async Task<IEnumerable<WorkoutRecord>> GetUserWorkoutRecordsAsync(string userId, long? workoutId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        await workoutRecordServiceValidator.ValidateGetAllAsync(userId, workoutId, range, cancellationToken);

        var userWorkoutRecords = workoutRecordRepository.GetUserWorkoutRecords(userId, workoutId, range);

        return await userWorkoutRecords.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("workout records", "get", userId));
    }

    public async Task<WorkoutRecord?> GetUserWorkoutRecordByIdAsync(string userId, long workoutRecordId, CancellationToken cancellationToken)
    {
        await workoutRecordServiceValidator.ValidateGetByIdAsync(userId, workoutRecordId, cancellationToken);

        return await workoutRecordRepository.GetByIdAsync(workoutRecordId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutRecordEntityName, "get", userId));
    }


    async Task AddExerciseRecordGroupsAsync(string userId, long workoutRecordId, DateTime date, IEnumerable<ExerciseRecordGroup> exerciseRecordGroups, CancellationToken cancellationToken)
    {
        foreach (var exerciseRecordGroup in exerciseRecordGroups)
        {
            var exerciseRecords = exerciseRecordGroup.ExerciseRecords;

            exerciseRecordGroup.WorkoutRecordId = workoutRecordId;
            exerciseRecordGroup.ExerciseRecords = null!;
            await exerciseRecordGroupRepository.AddAsync(exerciseRecordGroup, cancellationToken)
                .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise record group", "add", userId));

            foreach (var exerciseRecord in exerciseRecords)
            {
                exerciseRecord.Date = date;
                exerciseRecord.ExerciseRecordGroupId = exerciseRecordGroup.Id;
                exerciseRecord.ExerciseId = exerciseRecordGroup.ExerciseId;
                await exerciseRecordRepository.AddAsync(exerciseRecord, cancellationToken)
                    .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise record", "add", userId));
            }
        }
    }

    async Task UpdateUserFirstWorkoutDateAsync(string userId)
    {
        var user = (await userRepository.GetUserByIdAsync(userId))!;
        var userWorkoutRecords = workoutRecordRepository.GetUserWorkoutRecords(userId, null, null);

        var firstWorkoutDate = userWorkoutRecords?.MinBy(wr => wr.Date)?.Date;
        user.StartedWorkingOut = firstWorkoutDate;

        await userRepository.UpdateUserAsync(user)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("user first workout date", "update", userId));
    }
}