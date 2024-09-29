using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Repositories;

public class ExerciseRecordRepository : DbModelRepository<ExerciseRecord>
{
    public ExerciseRecordRepository(WorkoutDbContext db) : base(db)
    {

    }
}
