using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Repositories;

public class ExerciseRecordRepository : BaseRepository<ExerciseRecord>
{
    public ExerciseRecordRepository(WorkoutDbContext db) : base(db)
    {

    }
}
