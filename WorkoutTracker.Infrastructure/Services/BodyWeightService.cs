using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.ValueObjects;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Validators.Services;

namespace WorkoutTracker.Infrastructure.Services;

internal class BodyWeightService : DbModelService<BodyWeightService, BodyWeight>, IBodyWeightService
{
    readonly BodyWeightServiceValidator bodyWeightServiceValidator;

    public BodyWeightService(
        IBodyWeightRepository baseRepository,
        BodyWeightServiceValidator bodyWeightServiceValidator,
        ILogger<BodyWeightService> logger
    ) : base(baseRepository, logger)
    {
        this.bodyWeightServiceValidator = bodyWeightServiceValidator;
    }

    public async Task<BodyWeight> AddBodyWeightToUserAsync(string userId, BodyWeight bodyWeight)
    {
        await bodyWeightServiceValidator.ValidateAddAsync(userId, bodyWeight);

        bodyWeight.UserId = userId;

        return await baseRepository.AddAsync(bodyWeight)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("body weight", "add", userId));
    }

    public async Task UpdateUserBodyWeightAsync(string userId, BodyWeight bodyWeight)
    {
        await bodyWeightServiceValidator.ValidateUpdateAsync(userId, bodyWeight);

        var _bodyWeight = (await baseRepository.GetByIdAsync(bodyWeight.Id))!;

        _bodyWeight.Weight = bodyWeight.Weight;
        _bodyWeight.Date = bodyWeight.Date;

        await baseRepository.UpdateAsync(_bodyWeight)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("body weight", "update", userId));
    }

    public async Task DeleteBodyWeightFromUserAsync(string userId, long bodyWeightId)
    {
        await bodyWeightServiceValidator.ValidateDeleteAsync(userId, bodyWeightId);

        await baseRepository.RemoveAsync(bodyWeightId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("body weight", "delete", userId));
    }

    async Task<IQueryable<BodyWeight>> GetUserBodyWeightsAsync(string userId, DateTimeRange? range = null)
    {
        await bodyWeightServiceValidator.ValidateGetAllAsync(userId, range);

        IEnumerable<BodyWeight> userBodyWeights = (await baseRepository.FindAsync(wr => wr.UserId == userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("body weights", "get", userId)))
            .ToList();

        if (range is not null)
            userBodyWeights = userBodyWeights.Where(bw => range.IsDateInRange(bw.Date, true));

        return userBodyWeights.AsQueryable();
    }

    public async Task<IQueryable<BodyWeight>> GetUserBodyWeightsInPoundsAsync(string userId, DateTimeRange? range = null)
    {
        var userBodyWeights = await GetUserBodyWeightsAsync(userId, range);

        var userBodyWeightsInPounds = userBodyWeights.ToList().Select(m =>
        {
            m.Weight = ModelWeight.GetModelWeightInPounds(m.Weight);
            return m;
        }).AsQueryable();

        return userBodyWeightsInPounds;
    }

    public async Task<IQueryable<BodyWeight>> GetUserBodyWeightsInKilogramsAsync(string userId, DateTimeRange? range = null)
    {
        var userBodyWeights = await GetUserBodyWeightsAsync(userId, range);

        var userBodyWeightsInKilograms = userBodyWeights.ToList().Select(m =>
        {
            m.Weight = ModelWeight.GetModelWeightInKilos(m.Weight);
            return m;
        }).AsQueryable();

        return userBodyWeightsInKilograms;
    }

    public async Task<BodyWeight?> GetCurrentUserBodyWeightAsync(string userId)
    {
        await bodyWeightServiceValidator.ValidateGetCurrentAsync(userId);

        var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("current body weight", "get", userId));
        return userBodyWeights?.ToList().MaxBy(bw => bw.Date);
    }

    public async Task<BodyWeight?> GetMaxUserBodyWeightAsync(string userId)
    {
        await bodyWeightServiceValidator.ValidateGetMaxAsync(userId);

        var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("max body weight", "get", userId));

        return userBodyWeights?.ToList().MaxBy(bw => bw.Weight);
    }

    public async Task<BodyWeight?> GetMinUserBodyWeightAsync(string userId)
    {
        await bodyWeightServiceValidator.ValidateGetAllAsync(userId, null);

        var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("min body weight", "get", userId));

        return userBodyWeights?.ToList().MinBy(bw => bw.Weight);
    }

    public async Task<BodyWeight?> GetUserBodyWeightByIdAsync(string userId, long bodyWeightId)
    {
        await bodyWeightServiceValidator.ValidateGetByIdAsync(userId, bodyWeightId);

        return await baseRepository.GetByIdAsync(bodyWeightId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("body weight", "get", userId));
    }
}
