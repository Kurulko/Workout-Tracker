using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.WorkoutServices;

public interface IWorkoutService
{
    Task<ServiceResult<Workout>> GetUserWorkoutByIdAsync(string userId, long workoutId);
    Task<ServiceResult<Workout>> GetUserWorkoutByNameAsync(string userId, string name);
    Task<ServiceResult<IQueryable<Workout>>> GetUserWorkoutsAsync(string userId);
    Task<ServiceResult<Workout>> AddUserWorkoutAsync(string userId, Workout model);
    Task<ServiceResult> UpdateUserWorkoutAsync(string userId, Workout model);
    Task<ServiceResult> DeleteUserWorkoutAsync(string userId, long workoutId);
    Task<bool> UserWorkoutExistsAsync(string userId, long workoutId);
    Task<bool> UserWorkoutExistsByNameAsync(string userId, string name);
}
