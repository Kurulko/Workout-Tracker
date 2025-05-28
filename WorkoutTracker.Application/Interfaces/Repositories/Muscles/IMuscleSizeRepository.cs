using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.Interfaces.Repositories.Muscles;

public interface IMuscleSizeRepository : IBaseRepository<MuscleSize>
{
    Task<MuscleSize?> GetMaxUserMuscleSizeAsync(string userId, long muscleId, CancellationToken cancellationToken = default);
    Task<MuscleSize?> GetMinUserMuscleSizeAsync(string userId, long muscleId, CancellationToken cancellationToken = default);

    IQueryable<MuscleSize> GetUserMuscleSizes(string userId, long? muscleId = null, DateTimeRange? range = null);

    Task<IEnumerable<MuscleSize>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId = null, DateTimeRange? range = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<MuscleSize>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId = null, DateTimeRange? range = null, CancellationToken cancellationToken = default);
}