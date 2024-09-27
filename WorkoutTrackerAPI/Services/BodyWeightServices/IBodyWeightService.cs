using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Repositories;

namespace WorkoutTrackerAPI.Services.BodyWeightServices;

public interface IBodyWeightService
{
    Task<ServiceResult<BodyWeight>> GetUserBodyWeightByIdAsync(string userId, long bodyWeightId);
    Task<ServiceResult<BodyWeight>> GetUserBodyWeightByDateAsync(string userId, DateOnly date);
    Task<ServiceResult<BodyWeight>> GetMinUserBodyWeightAsync(string userId);
    Task<ServiceResult<BodyWeight>> GetMaxUserBodyWeightAsync(string userId);
    Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsAsync(string userId);
    Task<ServiceResult<BodyWeight>> AddBodyWeightToUserAsync(string userId, BodyWeight bodyWeight);
    Task<ServiceResult> UpdateUserBodyWeightAsync(string userId, BodyWeight bodyWeight);
    Task<ServiceResult> DeleteBodyWeightFromUserAsync(string userId, long bodyWeightId);
}
