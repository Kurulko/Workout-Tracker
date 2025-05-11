using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.ValueObjects;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Infrastructure.Services.Muscles;

internal class MuscleSizeService : DbModelService<MuscleSizeService, MuscleSize>, IMuscleSizeService
{
    readonly IUserRepository userRepository;
    public MuscleSizeService(
        IMuscleSizeRepository baseRepository, 
        IUserRepository userRepository,
        ILogger<MuscleSizeService> logger
    ) : base(baseRepository, logger)
    {
        this.userRepository = userRepository;
    }

    readonly EntryNullException muscleSizeIsNullException = new("Muscle size");
    readonly InvalidIDException invalidMuscleSizeIDException = new(nameof(MuscleSize));

    NotFoundException MuscleSizeNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID("Muscle size", id);

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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<MuscleSize>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("muscle size", "add", userId));
            throw;
        }
    }

    public async Task<ServiceResult> DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (muscleSizeId < 1)
                throw invalidMuscleSizeIDException;

            var muscleSize = await baseRepository.GetByIdAsync(muscleSizeId) ?? throw MuscleSizeNotFoundByIDException(muscleSizeId);

            if (muscleSize.UserId != userId)
                throw UserNotHavePermissionException("delete", "muscle size");

            await baseRepository.RemoveAsync(muscleSizeId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("muscle size", "delete", userId));
            throw;
        }
    }
    
    async Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesAsync(string userId, long? muscleId = null, DateTimeRange? range = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (range is DateTimeRange _range && _range.LastDate > DateTime.Now.Date)
                throw new ArgumentException("Incorrect date.");

            if (muscleId.HasValue && muscleId < 1)
                throw new InvalidIDException(nameof(Muscle));

            IEnumerable<MuscleSize> userMuscleSizes = (await baseRepository.FindAsync(wr => wr.UserId == userId)).ToList();

            if (range is not null)
                userMuscleSizes = userMuscleSizes.Where(ms => range.IsDateInRange(ms.Date, true));

            if (muscleId.HasValue)
                userMuscleSizes = userMuscleSizes.Where(ms => ms.MuscleId == muscleId);

            return ServiceResult<IQueryable<MuscleSize>>.Ok(userMuscleSizes.AsQueryable());
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<MuscleSize>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("muscle sizes", "delete", userId));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId = null, DateTimeRange? range = null)
    {
        var serviceResult = await GetUserMuscleSizesAsync(userId, muscleId, range);

        if (!serviceResult.Success)
            return serviceResult;

        var userMuscleSizesInInches = serviceResult.Model!.ToList().Select(m =>
        {
            m.Size = ModelSize.GetModelSizeInInches(m.Size);
            return m;
        }).AsQueryable();

        return ServiceResult<IQueryable<MuscleSize>>.Ok(userMuscleSizesInInches);
    }

    public async Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId = null, DateTimeRange? range = null)
    {
        var serviceResult = await GetUserMuscleSizesAsync(userId, muscleId, range);

        if (!serviceResult.Success)
            return serviceResult;

        var userMuscleSizesInCentimeter = serviceResult.Model!.ToList().Select(m =>
        {
            m.Size = ModelSize.GetModelSizeInCentimeters(m.Size);
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
            var userMaxMuscleSize = userMuscleSizes?.ToList().MaxBy(bw => bw.Size);
            return ServiceResult<MuscleSize>.Ok(userMaxMuscleSize);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<MuscleSize>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("max muscle size", "add", userId));
            throw;
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
            var userMinMuscleSize = userMuscleSizes?.ToList().MinBy(bw => bw.Size);
            return ServiceResult<MuscleSize>.Ok(userMinMuscleSize);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<MuscleSize>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("min muscle size", "add", userId));
            throw;
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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<MuscleSize>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("muscle size", "get", userId));
            throw;
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

            var _muscleSize = await baseRepository.GetByIdAsync(muscleSize.Id) ?? throw MuscleSizeNotFoundByIDException(muscleSize.Id);

            if (_muscleSize.UserId != userId)
                throw UserNotHavePermissionException("update", "muscle size");

            _muscleSize.Date = muscleSize.Date;
            _muscleSize.Size = muscleSize.Size;
            _muscleSize.MuscleId = muscleSize.MuscleId;

            await baseRepository.UpdateAsync(_muscleSize);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("muscle size", "update", userId));
            throw;
        }
    }
}
