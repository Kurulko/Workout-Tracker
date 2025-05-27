using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Interfaces.Services.Workouts;

public interface IWorkoutService : IBaseService
{
    Task<Workout?> GetUserWorkoutByIdAsync(string userId, long workoutId, CancellationToken cancellationToken = default);
    Task<Workout?> GetUserWorkoutByNameAsync(string userId, string name, CancellationToken cancellationToken = default);
    
    Task<Workout?> GetUserWorkoutByIdWithDetailsAsync(string userId, long workoutId, CancellationToken cancellationToken = default);
    Task<Workout?> GetUserWorkoutByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Workout>> GetUserWorkoutsAsync(string userId, long? exerciseId = null, CancellationToken cancellationToken = default);

    Task<Workout> AddUserWorkoutAsync(string userId, Workout model, CancellationToken cancellationToken = default);
    Task UpdateUserWorkoutAsync(string userId, Workout model, CancellationToken cancellationToken = default);

    Task AddExerciseSetGroupsToUserWorkoutAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups, CancellationToken cancellationToken = default);
    Task UpdateUserWorkoutExerciseSetGroupsAsync(string userId, long workoutId, IEnumerable<ExerciseSetGroup> exerciseSetGroups, CancellationToken cancellationToken = default);

    Task PinUserWorkout(string userId, long workoutId, CancellationToken cancellationToken = default);
    Task UnpinUserWorkout(string userId, long workoutId, CancellationToken cancellationToken = default);
    Task CompleteUserWorkout(string userId, long workoutId, DateTime date, TimeSpan time, CancellationToken cancellationToken = default);

    Task DeleteUserWorkoutAsync(string userId, long workoutId, CancellationToken cancellationToken = default);
}
