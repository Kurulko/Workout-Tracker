
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Services.Muscles;
public interface IMuscleSizeService
{
    Task<ServiceResult<MuscleSize>> GetUserMuscleSizeByIdAsync(string userId, long muscleSizeId);
    Task<ServiceResult<MuscleSize>> GetMinUserMuscleSizeAsync(string userId, long muscleId);
    Task<ServiceResult<MuscleSize>> GetMaxUserMuscleSizeAsync(string userId, long muscleId);
    Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId =  null, DateTimeRange? range = null);
    Task<ServiceResult<IQueryable<MuscleSize>>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId =  null, DateTimeRange? range = null);
    Task<ServiceResult<MuscleSize>> AddMuscleSizeToUserAsync(string userId, MuscleSize muscleSize);
    Task<ServiceResult> UpdateUserMuscleSizeAsync(string userId, MuscleSize muscleSize);
    Task<ServiceResult> DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId);
}
