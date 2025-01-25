using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Services.WorkoutServices;

public interface IWorkoutService
{
    Task<ServiceResult<Workout>> GetUserWorkoutByIdAsync(string userId, long workoutId, bool withDetails = false);
    Task<ServiceResult<Workout>> GetUserWorkoutByNameAsync(string userId, string name, bool withDetails = false);
    Task<ServiceResult<IQueryable<Workout>>> GetUserWorkoutsAsync(string userId, long? exerciseId = null);

    Task<ServiceResult<Workout>> AddUserWorkoutAsync(string userId, Workout model);
    Task<ServiceResult> UpdateUserWorkoutAsync(string userId, Workout model);

    Task<ServiceResult> AddExerciseSetGroupsToUserWorkoutAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups);
    Task<ServiceResult> UpdateUserWorkoutExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups);

    Task<ServiceResult> PinUserWorkout(string userId, long workoutId);
    Task<ServiceResult> UnpinUserWorkout(string userId, long workoutId);

    Task<ServiceResult> CompleteUserWorkout(string userId, long workoutId, DateTime date, TimeSpan time);
    Task<ServiceResult> DeleteUserWorkoutAsync(string userId, long workoutId);
    Task<bool> UserWorkoutExistsAsync(string userId, long workoutId);
    Task<bool> UserWorkoutExistsByNameAsync(string userId, string name);
}
