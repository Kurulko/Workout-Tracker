using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Repositories;

public class ExerciseRepository : BaseWorkoutRepository<Exercise>
{
    public ExerciseRepository(WorkoutDbContext db) : base(db)
    {

    }
}