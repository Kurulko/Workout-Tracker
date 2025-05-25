using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Services.Muscles;

public interface IMuscleService : IBaseService
{
    Task<Muscle?> GetMuscleByIdAsync(long muscleId, string userId, bool withDetails = false);
    Task<Muscle?> GetMuscleByNameAsync(string name, string userId, bool withDetails = false);

    Task<IQueryable<Muscle>> GetMusclesAsync(long? parentMuscleId = null, bool? isMeasurable = null);
    Task<IQueryable<Muscle>> GetParentMusclesAsync();
    Task<IQueryable<Muscle>> GetChildMusclesAsync();

    Task<Muscle> AddMuscleAsync(Muscle muscle);
    Task UpdateMuscleAsync(Muscle muscle);
    Task UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs);

    Task DeleteMuscleAsync(long muscleId);
}
