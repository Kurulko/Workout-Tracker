using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Infrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Validators.Services.Exercises;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ExerciseRecord> AddExerciseRecordToExerciseRecordGroupAsync(long exerciseRecordGroupId, string userId, ExerciseRecord exerciseRecord, CancellationToken cancellationToken)
    {
        await exerciseRecordServiceValidator.ValidateAddAsync(exerciseRecordGroupId, userId, exerciseRecord, cancellationToken);

        exerciseRecord.ExerciseRecordGroupId = exerciseRecordGroupId;
        exerciseRecord.Date = DateTime.UtcNow;

        return await baseRepository.AddAsync(exerciseRecord, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(exerciseRecordEntityName, "add", userId));
    }

    public async Task UpdateUserExerciseRecordAsync(string userId, ExerciseRecord exerciseRecord, CancellationToken cancellationToken)
    {
        await exerciseRecordServiceValidator.ValidateUpdateAsync(userId, exerciseRecord, cancellationToken);

        var updateAction = new Action<ExerciseRecord>(er =>
        {
            er.Date = exerciseRecord.Date;
            er.Weight = exerciseRecord.Weight;
            er.Time = exerciseRecord.Time;
            er.Reps = exerciseRecord.Reps;
            er.ExerciseId = exerciseRecord.ExerciseId;
            er.ExerciseRecordGroupId = exerciseRecord.ExerciseRecordGroupId;
        });

        await baseRepository.UpdatePartialAsync(exerciseRecord.Id, updateAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(exerciseRecordEntityName, "update", userId));
    }

    public async Task DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId, CancellationToken cancellationToken)
    {
        await exerciseRecordServiceValidator.ValidateDeleteAsync(userId, exerciseRecordId, cancellationToken);

        await baseRepository.RemoveAsync(exerciseRecordId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(exerciseRecordEntityName, "delete", userId));
    }

    public async Task<IEnumerable<ExerciseRecord>> GetUserExerciseRecordsAsync(string userId, long? exerciseId, ExerciseType? exerciseType, DateTimeRange? range, CancellationToken cancellationToken)
    {
        await exerciseRecordServiceValidator.ValidateGetAllAsync(userId, exerciseId, exerciseType, range, cancellationToken);

        var userExerciseRecords = exerciseRecordRepository.GetUserExerciseRecords(userId, exerciseId, exerciseType, range);

        return await userExerciseRecords.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("exercise records", "get", userId));
    }

    public async Task<ExerciseRecord?> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId, CancellationToken cancellationToken)
    {
        await exerciseRecordServiceValidator.ValidateGetAsync(userId, exerciseRecordId, cancellationToken);

        return await baseRepository.GetByIdAsync(exerciseRecordId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(exerciseRecordEntityName, "get", userId));
    }
}
