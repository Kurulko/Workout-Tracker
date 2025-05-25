using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Validators.Services.Workouts;

namespace WorkoutTracker.Infrastructure.Services.Workouts;

internal class WorkoutRecordService : DbModelService<WorkoutRecordService, WorkoutRecord>, IWorkoutRecordService
{
    readonly IUserRepository userRepository;
    readonly IWorkoutRepository workoutRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly IExerciseRecordGroupRepository exerciseRecordGroupRepository;
    readonly WorkoutRecordServiceValidator workoutRecordServiceValidator;

    public WorkoutRecordService(
        IWorkoutRecordRepository baseRepository,
        IWorkoutRepository workoutRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IExerciseRecordGroupRepository exerciseRecordGroupRepository,
        IUserRepository userRepository,
        WorkoutRecordServiceValidator workoutRecordServiceValidator,
        ILogger<WorkoutRecordService> logger
    ) : base(baseRepository, logger)
    {
        this.workoutRepository = workoutRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.exerciseRecordGroupRepository = exerciseRecordGroupRepository;
        this.userRepository = userRepository;
        this.workoutRecordServiceValidator = workoutRecordServiceValidator;
    }

    const string workoutRecordEntityName = "workout record";

    public async Task<WorkoutRecord> AddWorkoutRecordToUserAsync(string userId, WorkoutRecord workoutRecord)
    {
        await workoutRecordServiceValidator.ValidateAddAsync(userId, workoutRecord);

        var workout = (await workoutRepository.GetByIdAsync(workoutRecord.WorkoutId))!;
        var exerciseRecordGroups = workoutRecord.ExerciseRecordGroups;

        workoutRecord.UserId = userId;
        workoutRecord.ExerciseRecordGroups = null!;

        await baseRepository.AddAsync(workoutRecord)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutRecordEntityName, "add", userId));

        await AddExerciseRecordGroups(userId, workoutRecord.Id, workoutRecord.Date, exerciseRecordGroups);

        workout.CountOfTrainings++;
        await workoutRepository.UpdateAsync(workout);
        await UpdateUserFirstWorkoutDate(userId);

        return workoutRecord;
    }

    public async Task UpdateUserWorkoutRecordAsync(string userId, WorkoutRecord workoutRecord)
    {
        await workoutRecordServiceValidator.ValidateUpdateAsync(userId, workoutRecord);

        var _workout = (await workoutRepository.GetByIdAsync(workoutRecord.WorkoutId))!;
        var _workoutRecord = (await baseRepository.GetByIdAsync(workoutRecord.Id))!;

        var _exerciseRecordGroups = _workoutRecord.ExerciseRecordGroups;

        _workoutRecord.Time = workoutRecord.Time;
        _workoutRecord.Date = workoutRecord.Date;
        _workoutRecord.WorkoutId = workoutRecord.WorkoutId;
        _workoutRecord.ExerciseRecordGroups = null!;

        await baseRepository.UpdateAsync(_workoutRecord)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutRecordEntityName, "update", userId));

        await UpdateUserFirstWorkoutDate(userId);

        await AddExerciseRecordGroups(userId, workoutRecord.Id, workoutRecord.Date, workoutRecord.ExerciseRecordGroups);
    }

    public async Task DeleteWorkoutRecordFromUserAsync(string userId, long workoutRecordId)
    {
        await workoutRecordServiceValidator.ValidateDeleteAsync(userId, workoutRecordId);

        var workoutRecord = (await baseRepository.GetByIdAsync(workoutRecordId))!;

        await baseRepository.RemoveAsync(workoutRecordId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutRecordEntityName, "delete", userId));

        var workout = (await workoutRepository.GetByIdAsync(workoutRecord.WorkoutId))!;

        workout.CountOfTrainings--;
        await workoutRepository.UpdateAsync(workout);
        await UpdateUserFirstWorkoutDate(userId);
    }

    public async Task<IQueryable<WorkoutRecord>> GetUserWorkoutRecordsAsync(string userId, long? workoutId = null, DateTimeRange? range = null)
    {
        await workoutRecordServiceValidator.ValidateGetAllAsync(userId, workoutId, range);

        IEnumerable<WorkoutRecord> userWorkoutRecords = (await baseRepository.FindAsync(wr => wr.UserId == userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("workout records", "get", userId)))
            .ToList();

        if (range is not null)
            userWorkoutRecords = userWorkoutRecords.Where(bw => range.IsDateInRange(bw.Date, true));

        if (workoutId.HasValue)
            userWorkoutRecords = userWorkoutRecords.Where(ms => ms.WorkoutId == workoutId);

        return userWorkoutRecords.AsQueryable();
    }

    public async Task<WorkoutRecord?> GetUserWorkoutRecordByIdAsync(string userId, long workoutRecordId)
    {
        await workoutRecordServiceValidator.ValidateGetByIdAsync(userId, workoutRecordId);

        return await baseRepository.GetByIdAsync(workoutRecordId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(workoutRecordEntityName, "get", userId));
    }


    async Task AddExerciseRecordGroups(string userId, long workoutRecordId, DateTime date, IEnumerable<ExerciseRecordGroup> exerciseRecordGroups)
    {
        foreach (var exerciseRecordGroup in exerciseRecordGroups)
        {
            var exerciseRecords = exerciseRecordGroup.ExerciseRecords;

            exerciseRecordGroup.WorkoutRecordId = workoutRecordId;
            exerciseRecordGroup.ExerciseRecords = null!;
            await exerciseRecordGroupRepository.AddAsync(exerciseRecordGroup);

            foreach (var exerciseRecord in exerciseRecords)
            {
                exerciseRecord.Date = date;
                exerciseRecord.ExerciseRecordGroupId = exerciseRecordGroup.Id;
                exerciseRecord.ExerciseId = exerciseRecordGroup.ExerciseId;
                await exerciseRecordRepository.AddAsync(exerciseRecord);
            }
        }
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