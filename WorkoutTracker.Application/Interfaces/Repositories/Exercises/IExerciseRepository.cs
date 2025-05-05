using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Exercises;

public interface IExerciseRepository : IBaseWorkoutRepository<Exercise>
{
    Task<Exercise?> GetExerciseByIdWithDetailsAsync(long key, string userId);
    Task<Exercise?> GetExerciseByNameWithDetailsAsync(string name, string userId);
}