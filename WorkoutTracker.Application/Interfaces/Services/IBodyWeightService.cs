using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IBodyWeightService : IBaseService
{
    Task<BodyWeight?> GetUserBodyWeightByIdAsync(string userId, long bodyWeightId, CancellationToken cancellationToken = default);
    Task<BodyWeight?> GetCurrentUserBodyWeightAsync(string userId, CancellationToken cancellationToken = default);

    Task<BodyWeight?> GetMinUserBodyWeightAsync(string userId, CancellationToken cancellationToken = default);
    Task<BodyWeight?> GetMaxUserBodyWeightAsync(string userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<BodyWeight>> GetUserBodyWeightsInPoundsAsync(string userId, DateTimeRange? range = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<BodyWeight>> GetUserBodyWeightsInKilogramsAsync(string userId, DateTimeRange? range = null, CancellationToken cancellationToken = default);

    Task<BodyWeight> AddBodyWeightToUserAsync(string userId, BodyWeight bodyWeight, CancellationToken cancellationToken = default);
    Task UpdateUserBodyWeightAsync(string userId, BodyWeight bodyWeight, CancellationToken cancellationToken = default);

    Task DeleteBodyWeightFromUserAsync(string userId, long bodyWeightId, CancellationToken cancellationToken = default);
}
