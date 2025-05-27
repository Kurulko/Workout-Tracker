using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Validators.Services.Muscles;
using Microsoft.EntityFrameworkCore;

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

    public async Task<MuscleSize> AddMuscleSizeToUserAsync(string userId, MuscleSize muscleSize, CancellationToken cancellationToken)
    {
        await muscleSizeServiceValidator.ValidateAddAsync(userId, muscleSize, cancellationToken);

        muscleSize.UserId = userId;
        muscleSize.Date = DateTime.UtcNow;

        return await muscleSizeRepository.AddAsync(muscleSize)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "add", userId));
    }

    public async Task UpdateUserMuscleSizeAsync(string userId, MuscleSize muscleSize, CancellationToken cancellationToken)
    {
        await muscleSizeServiceValidator.ValidateUpdateAsync(userId, muscleSize, cancellationToken);

        var updateAction = new Action<MuscleSize>(ms =>
        {
            ms.Date = muscleSize.Date;
            ms.Size = muscleSize.Size;
            ms.MuscleId = muscleSize.MuscleId;
        });

        await muscleSizeRepository.UpdatePartialAsync(muscleSize.Id, updateAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "update", userId));
    }

    public async Task DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId, CancellationToken cancellationToken)
    {
        await muscleSizeServiceValidator.ValidateDeleteAsync(userId, muscleSizeId, cancellationToken);

        await muscleSizeRepository.RemoveAsync(muscleSizeId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "delete", userId));
    }

    public async Task<IEnumerable<MuscleSize>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        await muscleSizeServiceValidator.ValidateGetAllAsync(userId, muscleId, range, cancellationToken);

        return await muscleSizeRepository.GetUserMuscleSizesInInchesAsync(userId, muscleId, range, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("muscle sizes in inches", "get", userId));
    }

    public async Task<IEnumerable<MuscleSize>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        await muscleSizeServiceValidator.ValidateGetAllAsync(userId, muscleId, range, cancellationToken);

        return await muscleSizeRepository.GetUserMuscleSizesInCentimetersAsync(userId, muscleId, range, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("muscle sizes in centimeters", "get", userId));
    }

    public async Task<MuscleSize?> GetUserMuscleSizeByIdAsync(string userId, long muscleSizeId, CancellationToken cancellationToken)
    {
        await muscleSizeServiceValidator.ValidateGetByIdAsync(userId, muscleSizeId, cancellationToken);

        return await muscleSizeRepository.GetByIdAsync(muscleSizeId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(muscleSizeEntityName, "get", userId));
    }

    public async Task<MuscleSize?> GetMaxUserMuscleSizeAsync(string userId, long muscleId, CancellationToken cancellationToken)
    {
        await muscleSizeServiceValidator.ValidateGetMaxAsync(userId, muscleId, cancellationToken);

        return await muscleSizeRepository.GetMaxUserMuscleSizeAsync(userId, muscleId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("max muscle size", "get", userId));
    }

    public async Task<MuscleSize?> GetMinUserMuscleSizeAsync(string userId, long muscleId, CancellationToken cancellationToken)
    {
        await muscleSizeServiceValidator.ValidateGetMinAsync(userId, muscleId, cancellationToken);

        return await muscleSizeRepository.GetMinUserMuscleSizeAsync(userId, muscleId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("min muscle size", "get", userId));
    }
}
