using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Repositories;

public class MuscleRepository : BaseWorkoutRepository<Muscle>
{
    public MuscleRepository(WorkoutDbContext db) : base(db)
    {

    }
}