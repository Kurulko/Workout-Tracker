using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Repositories;

public class WorkoutRepository : BaseWorkoutRepository<Workout>
{
    public WorkoutRepository(WorkoutDbContext db) : base(db)
    {

    }
}