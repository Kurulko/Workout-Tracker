using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Validators.Services.Exercises;

namespace WorkoutTracker.Infrastructure.Services.Exercises;

internal class ExerciseRecordService : DbModelService<ExerciseRecordService, ExerciseRecord>, IExerciseRecordService
{
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly ExerciseRecordServiceValidator exerciseRecordServiceValidator;

    public ExerciseRecordService(
        IExerciseRecordRepository exerciseRecordRepository,
        ExerciseRecordServiceValidator exerciseRecordServiceValidator,
        ILogger<ExerciseRecordService> logger
    ) : base(exerciseRecordRepository, logger)
    {
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.exerciseRecordServiceValidator = exerciseRecordServiceValidator;
    }

    const string exerciseRecordEntityName = "exercise record";

    public async Task<ExerciseRecord> AddExerciseRecordToExerciseRecordGroupAsync(long exerciseRecordGroupId, string userId, ExerciseRecord exerciseRecord)
    {
        await exerciseRecordServiceValidator.ValidateAddAsync(exerciseRecordGroupId, userId, exerciseRecord);

        exerciseRecord.ExerciseRecordGroupId = exerciseRecordGroupId;
        exerciseRecord.Date = DateTime.UtcNow;

        return await baseRepository.AddAsync(exerciseRecord)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(exerciseRecordEntityName, "add", userId));
    }

    public async Task UpdateUserExerciseRecordAsync(string userId, ExerciseRecord exerciseRecord)
    {
        await exerciseRecordServiceValidator.ValidateUpdateAsync(userId, exerciseRecord);

        var _exerciseRecord = (await exerciseRecordRepository.GetByIdAsync(exerciseRecord.Id))!;

        _exerciseRecord.Date = exerciseRecord.Date;
        _exerciseRecord.Weight = exerciseRecord.Weight;
        _exerciseRecord.Time = exerciseRecord.Time;
        _exerciseRecord.Reps = exerciseRecord.Reps;
        _exerciseRecord.ExerciseId = exerciseRecord.ExerciseId;
        _exerciseRecord.ExerciseRecordGroupId = exerciseRecord.ExerciseRecordGroupId;

        await baseRepository.UpdateAsync(_exerciseRecord)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(exerciseRecordEntityName, "update", userId));
    }

    public async Task DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId)
    {
        await exerciseRecordServiceValidator.ValidateDeleteAsync(userId, exerciseRecordId);

        await baseRepository.RemoveAsync(exerciseRecordId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(exerciseRecordEntityName, "delete", userId));
    }

    public async Task<IQueryable<ExerciseRecord>> GetUserExerciseRecordsAsync(string userId, long? exerciseId = null, ExerciseType? exerciseType = null, DateTimeRange? range = null)
    {
        await exerciseRecordServiceValidator.ValidateGetAllAsync(userId, exerciseId, exerciseType, range);

        IEnumerable<ExerciseRecord> userExerciseRecords = (await exerciseRecordRepository.GetExerciseRecordsByUserIdAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise records", "get", userId)))
            .ToList();

        if (range is not null)
            userExerciseRecords = userExerciseRecords.Where(ms => range.IsDateInRange(ms.Date, true));

        if (exerciseId.HasValue)
            userExerciseRecords = userExerciseRecords.Where(ms => ms.ExerciseId == exerciseId);
        else if (exerciseType.HasValue)
            userExerciseRecords = userExerciseRecords.Where(ms => ms.Exercise!.Type == exerciseType);


        return userExerciseRecords.AsQueryable();
    }

    public async Task<ExerciseRecord?> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId)
    {
        await exerciseRecordServiceValidator.ValidateGetAsync(userId, exerciseRecordId);

        return await baseRepository.GetByIdAsync(exerciseRecordId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(exerciseRecordEntityName, "get", userId));
    }
}
