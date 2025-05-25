using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.ValueObjects;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Validators.Services.Muscles;

namespace WorkoutTracker.Infrastructure.Services.Muscles;

internal class MuscleSizeService : DbModelService<MuscleSizeService, MuscleSize>, IMuscleSizeService
{
    readonly IMuscleSizeRepository muscleSizeRepository;
    readonly MuscleSizeServiceValidator muscleSizeServiceValidator;

    public MuscleSizeService(
        IMuscleSizeRepository muscleSizeRepository,
        MuscleSizeServiceValidator muscleSizeServiceValidator,
        ILogger<MuscleSizeService> logger
    ) : base(muscleSizeRepository, logger)
    {
        this.muscleSizeRepository = muscleSizeRepository;
        this.muscleSizeServiceValidator = muscleSizeServiceValidator;
    }

    const string muscleSizeEntityName = "muscle size";

    public async Task<MuscleSize> AddMuscleSizeToUserAsync(string userId, MuscleSize muscleSize)
    {
        await muscleSizeServiceValidator.ValidateAddAsync(userId, muscleSize);

        muscleSize.UserId = userId;
        muscleSize.Date = DateTime.UtcNow;

        return await baseRepository.AddAsync(muscleSize)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "add", userId));
    }

    public async Task UpdateUserMuscleSizeAsync(string userId, MuscleSize muscleSize)
    {
        await muscleSizeServiceValidator.ValidateUpdateAsync(userId, muscleSize);

        var _muscleSize = (await muscleSizeRepository.GetByIdAsync(muscleSize.Id))!;

        _muscleSize.Date = muscleSize.Date;
        _muscleSize.Size = muscleSize.Size;
        _muscleSize.MuscleId = muscleSize.MuscleId;

        await baseRepository.UpdateAsync(_muscleSize)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "update", userId));
    }

    public async Task DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId)
    {
        await muscleSizeServiceValidator.ValidateDeleteAsync(userId, muscleSizeId);

        await baseRepository.RemoveAsync(muscleSizeId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "delete", userId));
    }

    async Task<IQueryable<MuscleSize>> GetUserMuscleSizesAsync(string userId, long? muscleId = null, DateTimeRange? range = null)
    {
        await muscleSizeServiceValidator.ValidateGetAllAsync(userId, muscleId, range);

        IEnumerable<MuscleSize> userMuscleSizes = (await baseRepository.FindAsync(wr => wr.UserId == userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("muscle sizes", "get", userId)))
            .ToList();

        if (range is not null)
            userMuscleSizes = userMuscleSizes.Where(ms => range.IsDateInRange(ms.Date, true));

        if (muscleId.HasValue)
            userMuscleSizes = userMuscleSizes.Where(ms => ms.MuscleId == muscleId);

        return userMuscleSizes.AsQueryable();
    }

    public async Task<IQueryable<MuscleSize>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId = null, DateTimeRange? range = null)
    {
        var userMuscleSizes = await GetUserMuscleSizesAsync(userId, muscleId, range);

        var userMuscleSizesInInches = userMuscleSizes.ToList().Select(m =>
        {
            m.Size = ModelSize.GetModelSizeInInches(m.Size);
            return m;
        }).AsQueryable();

        return userMuscleSizesInInches;
    }

    public async Task<IQueryable<MuscleSize>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId = null, DateTimeRange? range = null)
    {
        var userMuscleSizes = await GetUserMuscleSizesAsync(userId, muscleId, range);

        var userMuscleSizesInCentimeter = userMuscleSizes.ToList().Select(m =>
        {
            m.Size = ModelSize.GetModelSizeInCentimeters(m.Size);
            return m;
        }).AsQueryable();

        return userMuscleSizesInCentimeter;
    }

    public async Task<MuscleSize?> GetUserMuscleSizeByIdAsync(string userId, long muscleSizeId)
    {
        await muscleSizeServiceValidator.ValidateGetByIdAsync(userId, muscleSizeId);

        return await baseRepository.GetByIdAsync(muscleSizeId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "get", userId));
    }

    public async Task<MuscleSize?> GetMaxUserMuscleSizeAsync(string userId, long muscleId)
    {
        await muscleSizeServiceValidator.ValidateGetMaxAsync(userId, muscleId);

        var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId && ms.MuscleId == muscleId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "get", userId));

        var userMaxMuscleSize = userMuscleSizes?.ToList().MaxBy(bw => bw.Size);
        return userMaxMuscleSize;
    }

    public async Task<MuscleSize?> GetMinUserMuscleSizeAsync(string userId, long muscleId)
    {
        await muscleSizeServiceValidator.ValidateGetMinAsync(userId, muscleId);

        var userMuscleSizes = await baseRepository.FindAsync(ms => ms.UserId == userId && ms.MuscleId == muscleId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "get", userId));

        var userMinMuscleSize = userMuscleSizes?.ToList().MinBy(bw => bw.Size);
        return userMinMuscleSize;
    }
}
