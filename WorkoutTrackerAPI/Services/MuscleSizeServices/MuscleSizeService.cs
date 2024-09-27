using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.MuscleSizeServices;

namespace WorkoutTrackerAPI.Services;

public class MuscleSizeService : Service<MuscleSize>, IMuscleSizeService
{
    readonly UserRepository userRepository;
    public MuscleSizeService(MuscleSizeRepository baseRepository, UserRepository userRepository) : base(baseRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException muscleSizeIsNullException = new ("Muscle size");
    readonly InvalidIDException invalidMuscleSizeIDException = new (nameof(MuscleSize));
    readonly NotFoundException muscleSizeNotFoundException = new ("Muscle size");


    public async Task<ServiceResult<MuscleSize>> AddMuscleSizeToUserAsync(string userId, MuscleSize muscleSize)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<MuscleSize>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<MuscleSize>.Fail(userNotFoundException);

        if (muscleSize is null)
            return ServiceResult<MuscleSize>.Fail(muscleSizeIsNullException);

        if (muscleSize.Id != 0)
            return ServiceResult<MuscleSize>.Fail(InvalidEntryIDWhileAddingStr(nameof(MuscleSize), "muscle size"));

        try
        {
            muscleSize.UserId = userId;
            await baseRepository.AddAsync(muscleSize);

            return ServiceResult<MuscleSize>.Ok(muscleSize);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToAction("muscle size", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (muscleSizeId < 1)
            return ServiceResult.Fail(invalidMuscleSizeIDException);

        MuscleSize? muscleSize = await baseRepository.GetByIdAsync(muscleSizeId);

        if (muscleSize is null)
            return ServiceResult.Fail(muscleSizeNotFoundException);

        if (muscleSize.UserId != userId)
            return ServiceResult.Fail(UserNotHavePermissionStr("delete", "muscle size"));

        try
        {
            await baseRepository.RemoveAsync(muscleSizeId);
            return ServiceResult.Ok();
        }
        catch
        {
            return ServiceResult.Fail(FailedToAction("muscle size", "delete"));
        }
    }

    public async Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesAsync(string userId, long muscleId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<IQueryable<MuscleSize>>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<IQueryable<MuscleSize>>.Fail(userNotFoundException);

        if (muscleId < 1)
            return ServiceResult<IQueryable<MuscleSize>>.Fail(invalidMuscleSizeIDException);

        try
        {
            var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId && ms.MuscleId == muscleId);
            return ServiceResult<IQueryable<MuscleSize>>.Ok(userMuscleSizes);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<MuscleSize>>.Fail(FailedToAction("muscle sizes", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<MuscleSize>> GetMaxUserMuscleSizeAsync(string userId, long muscleId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<MuscleSize>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<MuscleSize>.Fail(userNotFoundException);

        if (muscleId < 1)
            return ServiceResult<MuscleSize>.Fail(invalidMuscleSizeIDException);

        try
        {
            var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId && ms.MuscleId == muscleId);
            var userMaxMuscleSize = userMuscleSizes?.MaxBy(bw => MuscleSize.GetMuscleSizeInCentimeters(bw));
            return ServiceResult<MuscleSize>.Ok(userMaxMuscleSize);
        }   
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToAction("max muscle size", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<MuscleSize>> GetMinUserMuscleSizeAsync(string userId, long muscleId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<MuscleSize>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<MuscleSize>.Fail(userNotFoundException);

        if (muscleId < 1)
            return ServiceResult<MuscleSize>.Fail(invalidMuscleSizeIDException);

        try
        {
            var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId && ms.MuscleId == muscleId);
            var userMinMuscleSize = userMuscleSizes?.MinBy(bw => MuscleSize.GetMuscleSizeInCentimeters(bw));
            return ServiceResult<MuscleSize>.Ok(userMinMuscleSize);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToAction("min muscle size", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<MuscleSize>> GetUserMuscleSizeByDateAsync(string userId, long muscleId, DateOnly date)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<MuscleSize>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<MuscleSize>.Fail(userNotFoundException);

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ServiceResult<MuscleSize>.Fail("Incorrect date.");

        try
        {
            var userMuscleSizeByDate = (await baseRepository.FindAsync(bw => bw.Date == date))?.First();
            return ServiceResult<MuscleSize>.Ok(userMuscleSizeByDate);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToAction("muscle size by date", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<MuscleSize>> GetUserMuscleSizeByIdAsync(string userId, long muscleSizeId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<MuscleSize>.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<MuscleSize>.Fail(userNotFoundException);

        if (muscleSizeId < 1)
            return ServiceResult<MuscleSize>.Fail(invalidMuscleSizeIDException);

        try
        {
            var userMuscleSizeById = await baseRepository.GetByIdAsync(muscleSizeId);
            return ServiceResult<MuscleSize>.Ok(userMuscleSizeById);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToAction("muscle size", "get", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateUserMuscleSizeAsync(string userId, MuscleSize muscleSize)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

        if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (muscleSize is null)
            return ServiceResult.Fail(muscleSizeIsNullException);

        if (muscleSize.Id < 1)
            return ServiceResult.Fail(invalidMuscleSizeIDException);

        try
        {
            MuscleSize? _muscleSize = await baseRepository.GetByIdAsync(muscleSize.Id);

            if (_muscleSize is null)
                return ServiceResult.Fail(muscleSizeNotFoundException);

            if (_muscleSize.UserId != userId)
                return ServiceResult.Fail(UserNotHavePermissionStr("update", "body weight"));

            await baseRepository.UpdateAsync(muscleSize);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToAction("muscle size", "update", ex.Message));
        }
    }
}
