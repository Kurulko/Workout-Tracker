using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Services.Muscles;
public interface IMuscleService : IBaseService
{
    Task<ServiceResult<Muscle>> GetMuscleByIdAsync(long muscleId, string userId, bool withDetails = false);
    Task<ServiceResult<Muscle>> GetMuscleByNameAsync(string name, string userId, bool withDetails = false);
    Task<ServiceResult<IQueryable<Muscle>>> GetMusclesAsync(long? parentMuscleId = null, bool? isMeasurable = null);
    Task<ServiceResult<IQueryable<Muscle>>> GetParentMusclesAsync();
    Task<ServiceResult<IQueryable<Muscle>>> GetChildMusclesAsync();
    Task<ServiceResult<Muscle>> AddMuscleAsync(Muscle muscle);
    Task<ServiceResult> UpdateMuscleAsync(Muscle muscle);
    Task<ServiceResult> UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs);
    Task<ServiceResult> DeleteMuscleAsync(long muscleId);
    Task<bool> MuscleExistsAsync(long muscleId);
    Task<bool> MuscleExistsByNameAsync(string name);
}
