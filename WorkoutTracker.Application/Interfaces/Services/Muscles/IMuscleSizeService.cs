
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Services.Muscles;

public interface IMuscleSizeService : IBaseService
{
    Task<MuscleSize?> GetUserMuscleSizeByIdAsync(string userId, long muscleSizeId, CancellationToken cancellationToken = default);
    Task<MuscleSize?> GetMinUserMuscleSizeAsync(string userId, long muscleId, CancellationToken cancellationToken = default);
    Task<MuscleSize?> GetMaxUserMuscleSizeAsync(string userId, long muscleId, CancellationToken cancellationToken = default);

    Task<IEnumerable<MuscleSize>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId =  null, DateTimeRange? range = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<MuscleSize>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId =  null, DateTimeRange? range = null, CancellationToken cancellationToken = default);

    Task<MuscleSize> AddMuscleSizeToUserAsync(string userId, MuscleSize muscleSize, CancellationToken cancellationToken = default);
    Task UpdateUserMuscleSizeAsync(string userId, MuscleSize muscleSize, CancellationToken cancellationToken = default);

    Task DeleteMuscleSizeFromUserAsync(string userId, long muscleSizeId, CancellationToken cancellationToken = default);
}
