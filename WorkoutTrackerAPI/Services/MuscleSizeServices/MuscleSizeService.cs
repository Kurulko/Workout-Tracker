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

public class MuscleSizeService : DbModelService<MuscleSize>, IMuscleSizeService
{
    readonly UserRepository userRepository;
    public MuscleSizeService(MuscleSizeRepository baseRepository, UserRepository userRepository) : base(baseRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException muscleSizeIsNullException = new("Muscle size");
    readonly InvalidIDException invalidMuscleSizeIDException = new(nameof(MuscleSize));
    readonly NotFoundException muscleSizeNotFoundException = new("Muscle size");


    public async Task<ServiceResult<MuscleSize>> AddMuscleSizeToUserAsync(string userId, MuscleSize muscleSize)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (muscleSize is null)
                throw muscleSizeIsNullException;

            if (muscleSize.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(MuscleSize), "muscle size");

            muscleSize.UserId = userId;
            await baseRepository.AddAsync(muscleSize);

            return ServiceResult<MuscleSize>.Ok(muscleSize);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<MuscleSize>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToActionStr("muscle size", "add", ex));
        }
    }

    public async Task<ServiceResult> DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (muscleSizeId < 1)
                throw invalidMuscleSizeIDException;

            var muscleSize = await baseRepository.GetByIdAsync(muscleSizeId) ?? throw muscleSizeNotFoundException;

            if (muscleSize.UserId != userId)
                throw UserNotHavePermissionException("delete", "muscle size");

            await baseRepository.RemoveAsync(muscleSizeId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("muscle size", "delete"));
        }
    }

    public async Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesAsync(string userId, long muscleId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (muscleId < 1)
                throw invalidMuscleSizeIDException;

            var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId && ms.MuscleId == muscleId);
            return ServiceResult<IQueryable<MuscleSize>>.Ok(userMuscleSizes);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<MuscleSize>>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<MuscleSize>>.Fail(FailedToActionStr("muscle sizes", "get", ex));
        }
    }

    public async Task<ServiceResult<MuscleSize>> GetMaxUserMuscleSizeAsync(string userId, long muscleId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (muscleId < 1)
                throw invalidMuscleSizeIDException;

            var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId && ms.MuscleId == muscleId);
            var userMaxMuscleSize = userMuscleSizes?.ToList().MaxBy(bw => MuscleSize.GetMuscleSizeInCentimeters(bw));
            return ServiceResult<MuscleSize>.Ok(userMaxMuscleSize);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<MuscleSize>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToActionStr("max muscle size", "get", ex));
        }
    }

    public async Task<ServiceResult<MuscleSize>> GetMinUserMuscleSizeAsync(string userId, long muscleId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (muscleId < 1)
                throw invalidMuscleSizeIDException;

            var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId && ms.MuscleId == muscleId);
            var userMinMuscleSize = userMuscleSizes?.ToList().MinBy(bw => MuscleSize.GetMuscleSizeInCentimeters(bw));
            return ServiceResult<MuscleSize>.Ok(userMinMuscleSize);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<MuscleSize>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToActionStr("min muscle size", "get", ex));
        }
    }

    public async Task<ServiceResult<MuscleSize>> GetUserMuscleSizeByDateAsync(string userId, long muscleId, DateOnly date)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (date > DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("Incorrect date.");

            var userMuscleSizeByDate = (await baseRepository.FindAsync(m => DateOnly.FromDateTime(m.Date) == date && m.UserId == userId && m.MuscleId == muscleId)).FirstOrDefault();
            return ServiceResult<MuscleSize>.Ok(userMuscleSizeByDate);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<MuscleSize>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToActionStr("muscle size by date", "get", ex));
        }
    }

    public async Task<ServiceResult<MuscleSize>> GetUserMuscleSizeByIdAsync(string userId, long muscleSizeId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (muscleSizeId < 1)
                throw invalidMuscleSizeIDException;

            var userMuscleSizeById = await baseRepository.GetByIdAsync(muscleSizeId);
            return ServiceResult<MuscleSize>.Ok(userMuscleSizeById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<MuscleSize>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<MuscleSize>.Fail(FailedToActionStr("muscle size", "get", ex));
        }
    }

    public async Task<ServiceResult> UpdateUserMuscleSizeAsync(string userId, MuscleSize muscleSize)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (muscleSize is null)
                throw muscleSizeIsNullException;

            if (muscleSize.Id < 1)
                throw invalidMuscleSizeIDException;

            var _muscleSize = await baseRepository.GetByIdAsync(muscleSize.Id) ?? throw muscleSizeNotFoundException;

            if (_muscleSize.UserId != userId)
                throw UserNotHavePermissionException("update", "muscle size");

            await baseRepository.UpdateAsync(muscleSize);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("muscle size", "update", ex));
        }
    }
}
