using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Persistence.Repositories;

internal class BodyWeightRepository : DbModelRepository<BodyWeight>, IBodyWeightRepository
{
    public BodyWeightRepository(WorkoutDbContext db) : base(db)
    {

    }

    public IQueryable<BodyWeight> GetUserBodyWeights(string userId, DateTimeRange? range)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        var userBodyWeights = Find(wr => wr.UserId == userId);

        if (range is not null)
            userBodyWeights = userBodyWeights.Where(bw => bw.Date >= range.FirstDate && bw.Date <= range.LastDate);

        return userBodyWeights;
    }

    public async Task<IEnumerable<BodyWeight>> GetUserBodyWeightsInPoundsAsync(string userId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        var userBodyWeights = await GetUserBodyWeights(userId, range).ToListAsync(cancellationToken);

        foreach (var bodyWeight in userBodyWeights)
            bodyWeight.Weight = ModelWeight.GetModelWeightInPounds(bodyWeight.Weight);

        return userBodyWeights;
    }

    public async Task<IEnumerable<BodyWeight>> GetUserBodyWeightsInKilogramsAsync(string userId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        var userBodyWeights = await GetUserBodyWeights(userId, range).ToListAsync(cancellationToken);

        foreach (var bodyWeight in userBodyWeights)
            bodyWeight.Weight = ModelWeight.GetModelWeightInKilos(bodyWeight.Weight);

        return userBodyWeights;
    }

    public async Task<BodyWeight?> GetCurrentUserBodyWeightAsync(string userId, CancellationToken cancellationToken)
    {
        var userBodyWeights = await GetUserBodyWeights(userId, null).ToListAsync(cancellationToken);

        var currentBodyWeight = userBodyWeights.MaxBy(bw => bw.Date);
        return currentBodyWeight;
    }

    public async Task<BodyWeight?> GetMaxUserBodyWeightAsync(string userId, CancellationToken cancellationToken)
    {
        var userBodyWeights = await GetUserBodyWeights(userId, null).ToListAsync(cancellationToken);

        var maxBodyWeight = userBodyWeights.MaxBy(bw => bw.Weight);
        return maxBodyWeight;
    }

    public async Task<BodyWeight?> GetMinUserBodyWeightAsync(string userId, CancellationToken cancellationToken)
    {
        var userBodyWeights = await GetUserBodyWeights(userId, null).ToListAsync(cancellationToken);

        var minBodyWeight = userBodyWeights.MinBy(bw => bw.Weight);
        return minBodyWeight;
    }
}
