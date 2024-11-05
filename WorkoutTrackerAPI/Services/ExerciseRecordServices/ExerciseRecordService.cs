﻿using System;
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

            exerciseRecord.CountOfTimes++;
            exerciseRecord.UserId = userId;
            exerciseRecord.Date = DateTime.Now;

            if (exerciseRecord.Reps is not null)
                exerciseRecord.SumOfReps += exerciseRecord.Reps;

            if (exerciseRecord.Time is not null)
                exerciseRecord.SumOfTime = exerciseRecord.SumOfTime + exerciseRecord.Time!;

            if (exerciseRecord.Weight is not null)
                exerciseRecord.SumOfWeight += exerciseRecord.Weight;

            if (exerciseRecord.Id == 0)
            {
                await baseRepository.AddAsync(exerciseRecord);
            }
            else
            {
                await baseRepository.UpdateAsync(exerciseRecord);
            }

            return ServiceResult<ExerciseRecord>.Ok(exerciseRecord);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToActionStr("exercise record", "add", ex));
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
            return ServiceResult.Fail(ex);
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
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(FailedToActionStr("exercise records", "get", ex));
        }
    }

    public async Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByDateAsync(string userId, long exerciseId, DateOnly date)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (date > DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("Incorrect date.");


            var userExerciseRecordByDate = (await baseRepository.FindAsync(m => DateOnly.FromDateTime(m.Date) == date && m.UserId == userId && m.ExerciseId == exerciseId)).FirstOrDefault();
            return ServiceResult<ExerciseRecord>.Ok(userExerciseRecordByDate);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToActionStr("exercise record by date", "get", ex));
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
            return ServiceResult<ExerciseRecord>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToActionStr("exercise record", "get", ex));
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

            _exerciseRecord.Date = exerciseRecord.Date;
            _exerciseRecord.CountOfTimes = exerciseRecord.CountOfTimes;
            _exerciseRecord.Weight = exerciseRecord.Weight;
            _exerciseRecord.Time = exerciseRecord.Time;
            _exerciseRecord.Reps = exerciseRecord.Reps;
            _exerciseRecord.SumOfWeight = exerciseRecord.SumOfWeight;
            _exerciseRecord.SumOfTime = exerciseRecord.SumOfTime;
            _exerciseRecord.SumOfReps = exerciseRecord.SumOfReps;
            _exerciseRecord.ExerciseId = exerciseRecord.ExerciseId;

            await baseRepository.UpdateAsync(_exerciseRecord);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record", "update", ex));
        }
    }
}
