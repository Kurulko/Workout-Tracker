using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Validators.Services;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTracker.Infrastructure.Services;

internal class BodyWeightService : DbModelService<BodyWeightService, BodyWeight>, IBodyWeightService
{
    readonly BodyWeightServiceValidator bodyWeightServiceValidator;
    readonly IBodyWeightRepository bodyWeightRepository;
    public BodyWeightService(
        IBodyWeightRepository bodyWeightRepository,
        BodyWeightServiceValidator bodyWeightServiceValidator,
        ILogger<BodyWeightService> logger
    ) : base(bodyWeightRepository, logger)
    {
        this.bodyWeightRepository = bodyWeightRepository;
        this.bodyWeightServiceValidator = bodyWeightServiceValidator;
    }

    const string bodyWeightEntityName = "body weight";

    public async Task<BodyWeight> AddBodyWeightToUserAsync(string userId, BodyWeight bodyWeight, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateAddAsync(userId, bodyWeight, cancellationToken);

        bodyWeight.UserId = userId;

        return await bodyWeightRepository.AddAsync(bodyWeight)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(bodyWeightEntityName, "add", userId));
    }

    public async Task UpdateUserBodyWeightAsync(string userId, BodyWeight bodyWeight, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateUpdateAsync(userId, bodyWeight, cancellationToken);

        var updateAction = new Action<BodyWeight>(bw =>
        {
            bw.Weight = bodyWeight.Weight;
            bw.Date = bodyWeight.Date;
        });

        await bodyWeightRepository.UpdatePartialAsync(bodyWeight.Id, updateAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(bodyWeightEntityName, "update", userId));
    }

    public async Task DeleteBodyWeightFromUserAsync(string userId, long bodyWeightId, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateDeleteAsync(userId, bodyWeightId, cancellationToken);

        await bodyWeightRepository.RemoveAsync(bodyWeightId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(bodyWeightEntityName, "delete", userId));
    }

    public async Task<IEnumerable<BodyWeight>> GetUserBodyWeightsInPoundsAsync(string userId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateGetAllAsync(userId, range, cancellationToken);

        return await bodyWeightRepository.GetUserBodyWeightsInPoundsAsync(userId, range, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("body weights in pounds", "get", userId));
    }

    public async Task<IEnumerable<BodyWeight>> GetUserBodyWeightsInKilogramsAsync(string userId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateGetAllAsync(userId, range, cancellationToken);

        return await bodyWeightRepository.GetUserBodyWeightsInKilogramsAsync(userId, range, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("body weights in kilograms", "get", userId));
    }

    public async Task<BodyWeight?> GetCurrentUserBodyWeightAsync(string userId, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateGetCurrentAsync(userId, cancellationToken);

        return await bodyWeightRepository.GetCurrentUserBodyWeightAsync(userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("current body weight", "get", userId));
    }

    public async Task<BodyWeight?> GetMaxUserBodyWeightAsync(string userId, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateGetMaxAsync(userId, cancellationToken);

        return await bodyWeightRepository.GetMaxUserBodyWeightAsync(userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("max body weight", "get", userId));
    }

    public async Task<BodyWeight?> GetMinUserBodyWeightAsync(string userId, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateGetAllAsync(userId, null, cancellationToken);

        return await bodyWeightRepository.GetMinUserBodyWeightAsync(userId, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("min body weight", "get", userId));
    }

    public async Task<BodyWeight?> GetUserBodyWeightByIdAsync(string userId, long bodyWeightId, CancellationToken cancellationToken)
    {
        await bodyWeightServiceValidator.ValidateGetByIdAsync(userId, bodyWeightId, cancellationToken);

        return await bodyWeightRepository.GetByIdAsync(bodyWeightId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(bodyWeightEntityName, "get", userId));
    }
}
