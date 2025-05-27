using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Repositories;

public interface IBodyWeightRepository : IBaseRepository<BodyWeight>
{
    IQueryable<BodyWeight> GetUserBodyWeights(string userId, DateTimeRange? range);

    Task<IEnumerable<BodyWeight>> GetUserBodyWeightsInPoundsAsync(string userId, DateTimeRange? range, CancellationToken cancellationToken = default);
    Task<IEnumerable<BodyWeight>> GetUserBodyWeightsInKilogramsAsync(string userId, DateTimeRange? range, CancellationToken cancellationToken = default);

    Task<BodyWeight?> GetCurrentUserBodyWeightAsync(string userId, CancellationToken cancellationToken = default);
    Task<BodyWeight?> GetMaxUserBodyWeightAsync(string userId, CancellationToken cancellationToken = default);
    Task<BodyWeight?> GetMinUserBodyWeightAsync(string userId, CancellationToken cancellationToken = default);
}
