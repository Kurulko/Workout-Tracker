using System;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;

namespace WorkoutTrackerAPI.Services.ExerciseRecordServices;

public class ExerciseRecordService : DbModelService<ExerciseRecord>, IExerciseRecordService
{
    readonly UserRepository userRepository;
    public ExerciseRecordService(ExerciseRecordRepository baseRepository, UserRepository userRepository) : base(baseRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException exerciseRecordIsNullException = new ("Exercise record");
    readonly InvalidIDException invalidExerciseRecordIDException = new (nameof(ExerciseRecord));
    readonly NotFoundException exerciseRecordNotFoundException = new ("Exercise record");

    public async Task<ServiceResult<ExerciseRecord>> AddExerciseRecordToUserAsync(string userId, ExerciseRecord exerciseRecord)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecord is null)
                throw exerciseRecordIsNullException;

            if (exerciseRecord.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(ExerciseRecord), "exercise record");

            exerciseRecord.UserId = userId;
            await baseRepository.AddAsync(exerciseRecord);

            return ServiceResult<ExerciseRecord>.Ok(exerciseRecord);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToActionStr("exercise record", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecordId < 1)
                throw invalidExerciseRecordIDException;

            ExerciseRecord? exerciseRecord = await baseRepository.GetByIdAsync(exerciseRecordId) ?? throw exerciseRecordNotFoundException;

            if (exerciseRecord.UserId != userId)
                throw UserNotHavePermissionException("delete", "exercise record");

            await baseRepository.RemoveAsync(exerciseRecordId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record", "delete"));
        }
    }

    public async Task<ServiceResult<IQueryable<ExerciseRecord>>> GetUserExerciseRecordsAsync(string userId, long exerciseId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userExerciseRecords = await baseRepository.FindAsync(m => m.UserId == userId && m.ExerciseId == exerciseId);
            return ServiceResult<IQueryable<ExerciseRecord>>.Ok(userExerciseRecords);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(FailedToActionStr("exercise records", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByDateAsync(string userId, long exerciseId, DateOnly date)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (date > DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("Incorrect date.");


            var userExerciseRecordByDate = (await baseRepository.FindAsync(m => m.Date == date && m.UserId == userId && m.ExerciseId == exerciseId)).FirstOrDefault();
            return ServiceResult<ExerciseRecord>.Ok(userExerciseRecordByDate);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToActionStr("exercise record by date", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId)
    {

        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecordId < 1)
                throw invalidExerciseRecordIDException;

            var userExerciseRecordById = await baseRepository.GetByIdAsync(exerciseRecordId);
            return ServiceResult<ExerciseRecord>.Ok(userExerciseRecordById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToActionStr("exercise record", "get", ex.Message));
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

            var _exerciseRecord = await baseRepository.GetByIdAsync(exerciseRecord.Id) ?? throw exerciseRecordNotFoundException;

            if (_exerciseRecord.UserId != userId)
                throw UserNotHavePermissionException("update", "exercise record");

            await baseRepository.UpdateAsync(exerciseRecord);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record", "update", ex.Message));
        }
    }
}
