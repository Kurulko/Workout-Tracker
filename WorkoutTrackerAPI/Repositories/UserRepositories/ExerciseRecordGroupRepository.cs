using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Repositories.UserRepositories;

public class ExerciseRecordGroupRepository : DbModelRepository<ExerciseRecordGroup>
{
    public ExerciseRecordGroupRepository(WorkoutDbContext db) : base(db)
    {

    }
}
