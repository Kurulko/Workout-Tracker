using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Services.Muscles;

public interface IMuscleService : IBaseService
{
    Task<Muscle?> GetMuscleByIdAsync(long muscleId, string userId, CancellationToken cancellationToken = default);
    Task<Muscle?> GetMuscleByNameAsync(string name, string userId, CancellationToken cancellationToken = default);

    Task<Muscle?> GetMuscleByIdWithDetailsAsync(long muscleId, string userId, CancellationToken cancellationToken = default);
    Task<Muscle?> GetMuscleByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Muscle>> GetMusclesAsync(long? parentMuscleId = null, bool? isMeasurable = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Muscle>> GetParentMusclesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Muscle>> GetChildMusclesAsync(CancellationToken cancellationToken = default);

    Task<Muscle> AddMuscleAsync(Muscle muscle, CancellationToken cancellationToken = default);
    Task UpdateMuscleAsync(Muscle muscle, CancellationToken cancellationToken = default);
    Task UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs, CancellationToken cancellationToken = default);

    Task DeleteMuscleAsync(long muscleId, CancellationToken cancellationToken = default);
}
