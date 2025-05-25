
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Services.Muscles;

public interface IMuscleSizeService : IBaseService
{
    Task<MuscleSize?> GetUserMuscleSizeByIdAsync(string userId, long muscleSizeId);
    Task<MuscleSize?> GetMinUserMuscleSizeAsync(string userId, long muscleId);
    Task<MuscleSize?> GetMaxUserMuscleSizeAsync(string userId, long muscleId);

    Task<IQueryable<MuscleSize>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId =  null, DateTimeRange? range = null);
    Task<IQueryable<MuscleSize>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId =  null, DateTimeRange? range = null);

    Task<MuscleSize> AddMuscleSizeToUserAsync(string userId, MuscleSize muscleSize);
    Task UpdateUserMuscleSizeAsync(string userId, MuscleSize muscleSize);

    Task DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId);
}
