using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IBodyWeightService : IBaseService
{
    Task<BodyWeight?> GetUserBodyWeightByIdAsync(string userId, long bodyWeightId);
    Task<BodyWeight?> GetCurrentUserBodyWeightAsync(string userId);

    Task<BodyWeight?> GetMinUserBodyWeightAsync(string userId);
    Task<BodyWeight?> GetMaxUserBodyWeightAsync(string userId);

    Task<IQueryable<BodyWeight>> GetUserBodyWeightsInPoundsAsync(string userId, DateTimeRange? range = null);
    Task<IQueryable<BodyWeight>> GetUserBodyWeightsInKilogramsAsync(string userId, DateTimeRange? range = null);

    Task<BodyWeight> AddBodyWeightToUserAsync(string userId, BodyWeight bodyWeight);
    Task UpdateUserBodyWeightAsync(string userId, BodyWeight bodyWeight);

    Task DeleteBodyWeightFromUserAsync(string userId, long bodyWeightId);
}
