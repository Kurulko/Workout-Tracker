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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
    
    async Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesAsync(string userId, long? muscleId = null, DateTime? date = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (date.HasValue && date.Value.Date > DateTime.Now.Date)
                throw new ArgumentException("Incorrect date.");

            if (muscleId.HasValue && muscleId < 1)
                throw invalidMuscleSizeIDException;

            var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId);

            if (date.HasValue)
                userMuscleSizes = userMuscleSizes.Where(ms => ms.Date.Date == date.Value.Date); 

            if (muscleId.HasValue)
                userMuscleSizes = userMuscleSizes.Where(ms => ms.MuscleId == muscleId);

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

    public async Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId = null, DateTime? date = null)
    {
        var serviceResult = await GetUserMuscleSizesAsync(userId, muscleId, date);

        if (!serviceResult.Success)
            return serviceResult;

        var userMuscleSizesInInches = serviceResult.Model!.AsEnumerable().Select(m =>
        {
            m.Size = (float)Math.Round(MuscleSize.GetMuscleSizeInInches(m), 1);
            m.SizeType = SizeType.Inch;
            return m;
        }).AsQueryable();

        return ServiceResult<IQueryable<MuscleSize>>.Ok(userMuscleSizesInInches);
    }

    public async Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId = null, DateTime? date = null)
    {
        var serviceResult = await GetUserMuscleSizesAsync(userId, muscleId, date);

        if (!serviceResult.Success)
            return serviceResult;

        var userMuscleSizesInCentimeter = serviceResult.Model!.AsEnumerable().Select(m =>
        {
            m.Size = (float)Math.Round(MuscleSize.GetMuscleSizeInCentimeters(m), 1);
            m.SizeType = SizeType.Centimeter;
            return m;
        }).AsQueryable();

        return ServiceResult<IQueryable<MuscleSize>>.Ok(userMuscleSizesInCentimeter);
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

            _muscleSize.Date = muscleSize.Date;
            _muscleSize.Size = muscleSize.Size;
            _muscleSize.SizeType = muscleSize.SizeType;
            _muscleSize.MuscleId = muscleSize.MuscleId;

            await baseRepository.UpdateAsync(_muscleSize);
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
