using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Extensions.Exercises;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Infrastructure.Services.Exercises;

internal class ExerciseRecordService : DbModelService<ExerciseRecordService, ExerciseRecord>, IExerciseRecordService
{
    readonly IUserRepository userRepository;
    readonly IExerciseRecordGroupRepository exerciseRecordGroupRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;

    public ExerciseRecordService(
        IExerciseRecordRepository exerciseRecordRepository, 
        IUserRepository userRepository,
        IExerciseRecordGroupRepository exerciseRecordGroupRepository,
        ILogger<ExerciseRecordService> logger
    ) : base(exerciseRecordRepository, logger)
    {
        this.userRepository = userRepository;
        this.exerciseRecordGroupRepository = exerciseRecordGroupRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
    }

    readonly EntryNullException exerciseRecordIsNullException = new("Exercise record");
    readonly InvalidIDException invalidExerciseRecordIDException = new(nameof(ExerciseRecord));
    NotFoundException ExerciseRecordNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID("Exercise record", id);

    public async Task<ServiceResult<ExerciseRecord>> AddExerciseRecordToExerciseRecordGroupAsync(long exerciseRecordGroupId, string userId, ExerciseRecord exerciseRecord)
    {
        try
        {
            if (exerciseRecord is null)
                throw exerciseRecordIsNullException;

            if (exerciseRecord.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(ExerciseRecord), "exercise record");

            var _exerciseRecordGroup = await exerciseRecordGroupRepository.GetByIdAsync(exerciseRecordGroupId) ?? throw NotFoundException.NotFoundExceptionByID("Exercise record group", exerciseRecordGroupId);

            if (_exerciseRecordGroup.GetUserId() != userId)
                throw UserNotHavePermissionException("get", "exercise record group");

            exerciseRecord.ExerciseRecordGroupId = exerciseRecordGroupId;
            exerciseRecord.Date = DateTime.Now;

            await baseRepository.AddAsync(exerciseRecord);

            return ServiceResult<ExerciseRecord>.Ok(exerciseRecord);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise record", "add", userId));
            throw;
        }
    }

    public async Task<ServiceResult> DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecordId < 1)
                throw invalidExerciseRecordIDException;

            var _userId = await exerciseRecordRepository.GetUserIdByExerciseRecordIdAsync(exerciseRecordId) ?? throw ExerciseRecordNotFoundByIDException(exerciseRecordId);

            if (_userId != userId)
                throw UserNotHavePermissionException("delete", "exercise record");

            await baseRepository.RemoveAsync(exerciseRecordId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise record", "delete", userId));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<ExerciseRecord>>> GetUserExerciseRecordsAsync(string userId, long? exerciseId = null, ExerciseType? exerciseType = null, DateTimeRange? range = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (range is DateTimeRange _range && _range.LastDate > DateTime.Now.Date)
                throw new ArgumentException("Incorrect date.");

            if (exerciseId.HasValue && exerciseId < 1)
                throw new InvalidIDException(nameof(Exercise));

            IEnumerable<ExerciseRecord> userExerciseRecords = (await exerciseRecordRepository.GetExerciseRecordsByUserIdAsync(userId)).ToList();

            if (range is not null)
                userExerciseRecords = userExerciseRecords.Where(ms => range.IsDateInRange(ms.Date, true));

            if (exerciseId.HasValue)
                userExerciseRecords = userExerciseRecords.Where(ms => ms.ExerciseId == exerciseId);
            else if(exerciseType.HasValue)
                userExerciseRecords = userExerciseRecords.Where(ms => ms.Exercise!.Type == exerciseType);

            return ServiceResult<IQueryable<ExerciseRecord>>.Ok(userExerciseRecords.AsQueryable());
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise records", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecordId < 1)
                throw invalidExerciseRecordIDException;

            var _userId = await exerciseRecordRepository.GetUserIdByExerciseRecordIdAsync(exerciseRecordId);

            if (_userId != null && _userId != userId)
                throw UserNotHavePermissionException("get", "exercise record");

            var userExerciseRecordById = await baseRepository.GetByIdAsync(exerciseRecordId);
            return ServiceResult<ExerciseRecord>.Ok(userExerciseRecordById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise record", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateUserExerciseRecordAsync(string userId, ExerciseRecord exerciseRecord)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecord is null)
                throw exerciseRecordIsNullException;

            if (exerciseRecord.Id < 1)
                throw invalidExerciseRecordIDException;

            var _exerciseRecord = await baseRepository.GetByIdAsync(exerciseRecord.Id) ?? throw ExerciseRecordNotFoundByIDException(exerciseRecord.Id);

            if (_exerciseRecord.GetUserId() != userId)
                throw UserNotHavePermissionException("update", "exercise record");

            _exerciseRecord.Date = exerciseRecord.Date;
            _exerciseRecord.Weight = exerciseRecord.Weight;
            _exerciseRecord.Time = exerciseRecord.Time;
            _exerciseRecord.Reps = exerciseRecord.Reps;
            _exerciseRecord.ExerciseId = exerciseRecord.ExerciseId;

            await baseRepository.UpdateAsync(_exerciseRecord);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("exercise record", "update", userId));
            throw;
        }
    }
}
