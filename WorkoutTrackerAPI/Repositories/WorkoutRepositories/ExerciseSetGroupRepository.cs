using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Repositories.WorkoutRepositories;

public class ExerciseSetGroupRepository : DbModelRepository<ExerciseSetGroup>
{
    public ExerciseSetGroupRepository(WorkoutDbContext db) : base(db)
    {

    }
}
