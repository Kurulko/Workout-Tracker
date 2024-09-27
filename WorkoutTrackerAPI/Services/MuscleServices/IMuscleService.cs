using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.MuscleServices;

public interface IMuscleService
{
    Task<ServiceResult<Muscle>> GetMuscleByIdAsync(long muscleId);
    Task<ServiceResult<Muscle>> GetMuscleByNameAsync(string name);
    Task<ServiceResult<IQueryable<Muscle>>> GetMusclesAsync();
    Task<ServiceResult<Muscle>> AddMuscleAsync(Muscle muscle);
    Task<ServiceResult> UpdateMuscleAsync(Muscle muscle);
    Task<ServiceResult> DeleteMuscleAsync(long muscleId);
    Task<bool> MuscleExistsAsync(long muscleId);
    Task<bool> MuscleExistsByNameAsync(string name);
}
