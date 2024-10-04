using System;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;

namespace WorkoutTrackerAPI.Services.ExerciseRecordServices;

public class ExerciseRecordService : Service<ExerciseRecord>, IExerciseRecordService
{
    readonly UserRepository userRepository;
    public ExerciseRecordService(ExerciseRecordRepository baseRepository, UserRepository userRepository) : base(baseRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException exerciseRecordIsNullException = new ("Exercise record");
    readonly InvalidIDException invalidExerciseRecordIDException = new (nameof(ExerciseRecord));
    readonly NotFoundException exerciseRecordNotFoundException = new ("Exercise record");


    public async Task<ServiceResult<ExerciseRecord>> AddExerciseRecordToUserAsync(string userId, ExerciseRecord exerciseRecord)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<ExerciseRecord>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<ExerciseRecord>.Fail(userNotFoundException);

        if (exerciseRecord is null)
            return ServiceResult<ExerciseRecord>.Fail(exerciseRecordIsNullException);

        if (exerciseRecord.Id != 0)
            return ServiceResult<ExerciseRecord>.Fail(InvalidEntryIDWhileAddingStr(nameof(ExerciseRecord), "exercise record"));

        try
        {
            exerciseRecord.UserId = userId;
            await baseRepository.AddAsync(exerciseRecord);

            return ServiceResult<ExerciseRecord>.Ok(exerciseRecord);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToAction("exercise record", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (exerciseRecordId < 1)
            return ServiceResult.Fail(invalidExerciseRecordIDException);

        ExerciseRecord? exerciseRecord = await baseRepository.GetByIdAsync(exerciseRecordId);

        if (exerciseRecord is null)
            return ServiceResult.Fail(exerciseRecordNotFoundException);

        if (exerciseRecord.UserId != userId)
            return ServiceResult.Fail(UserNotHavePermissionStr("delete", "exercise record"));

        try
        {
            await baseRepository.RemoveAsync(exerciseRecordId);
            return ServiceResult.Ok();
        }
        catch
        {
            return ServiceResult.Fail(FailedToAction("exercise record", "delete"));
        }
    }

    public async Task<ServiceResult<IQueryable<ExerciseRecord>>> GetUserExerciseRecordsAsync(string userId, long exerciseId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(userNotFoundException);

        try
        {
            var userExerciseRecords = await baseRepository.FindAsync(bw => bw.UserId == userId);
            return ServiceResult<IQueryable<ExerciseRecord>>.Ok(userExerciseRecords);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(FailedToAction("exercise records", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByDateAsync(string userId, long exerciseId, DateOnly date)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<ExerciseRecord>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<ExerciseRecord>.Fail(userNotFoundException);

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ServiceResult<ExerciseRecord>.Fail("Incorrect date.");

        try
        {
            var userExerciseRecordByDate = (await baseRepository.FindAsync(m => m.Date == date && m.UserId == userId)).FirstOrDefault();
            return ServiceResult<ExerciseRecord>.Ok(userExerciseRecordByDate);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToAction("body weight by date", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<ExerciseRecord>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<ExerciseRecord>.Fail(userNotFoundException);

        if (exerciseRecordId < 1)
            return ServiceResult<ExerciseRecord>.Fail(invalidExerciseRecordIDException);

        try
        {
            var userExerciseRecordById = await baseRepository.GetByIdAsync(exerciseRecordId);
            return ServiceResult<ExerciseRecord>.Ok(userExerciseRecordById);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToAction("body weight", "get", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateUserExerciseRecordAsync(string userId, ExerciseRecord exerciseRecord)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (exerciseRecord is null)
            return ServiceResult.Fail(exerciseRecordIsNullException);

        if (exerciseRecord.Id < 1)
            return ServiceResult.Fail(invalidExerciseRecordIDException);

        try
        {
            ExerciseRecord? _exerciseRecord = await baseRepository.GetByIdAsync(exerciseRecord.Id);

            if (_exerciseRecord is null)
                return ServiceResult.Fail(exerciseRecordNotFoundException);

            if (_exerciseRecord.UserId != userId)
                return ServiceResult.Fail(UserNotHavePermissionStr("update", "exercise record"));

            await baseRepository.UpdateAsync(exerciseRecord);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToAction("exercise record", "update", ex.Message));
        }
    }
}
