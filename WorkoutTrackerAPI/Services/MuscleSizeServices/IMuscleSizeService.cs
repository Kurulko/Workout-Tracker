using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.MuscleSizeServices;

public interface IMuscleSizeService
{
    Task<ServiceResult<MuscleSize>> GetUserMuscleSizeByIdAsync(string userId, long muscleSizeId);
    Task<ServiceResult<MuscleSize>> GetUserMuscleSizeByDateAsync(string userId, long muscleId, DateOnly date);
    Task<ServiceResult<MuscleSize>> GetMinUserMuscleSizeAsync(string userId, long muscleId);
    Task<ServiceResult<MuscleSize>> GetMaxUserMuscleSizeAsync(string userId, long muscleId);
    Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesAsync(string userId);
    Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesByMuscleIdAsync(string userId, long muscleId);
    Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInCentimetersAsync(string userId);
    Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInCentimetersByMuscleIdAsync(string userId, long muscleId);
    Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInInchesAsync(string userId);
    Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInInchesByMuscleIdAsync(string userId, long muscleId);
    Task<ServiceResult<MuscleSize>> AddMuscleSizeToUserAsync(string userId, MuscleSize muscleSize);
    Task<ServiceResult> UpdateUserMuscleSizeAsync(string userId, MuscleSize muscleSize);
    Task<ServiceResult> DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId);
}
