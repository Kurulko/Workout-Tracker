using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Interfaces.Services.Workouts;

public interface IWorkoutService : IBaseService
{
    Task<Workout?> GetUserWorkoutByIdAsync(string userId, long workoutId, bool withDetails = false);
    Task<Workout?> GetUserWorkoutByNameAsync(string userId, string name, bool withDetails = false);
    Task<IQueryable<Workout>> GetUserWorkoutsAsync(string userId, long? exerciseId = null);

    Task<Workout> AddUserWorkoutAsync(string userId, Workout model);
    Task UpdateUserWorkoutAsync(string userId, Workout model);

    Task AddExerciseSetGroupsToUserWorkoutAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups);
    Task UpdateUserWorkoutExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups);

    Task PinUserWorkout(string userId, long workoutId);
    Task UnpinUserWorkout(string userId, long workoutId);
    Task CompleteUserWorkout(string userId, long workoutId, DateTime date, TimeSpan time);

    Task DeleteUserWorkoutAsync(string userId, long workoutId);
}
