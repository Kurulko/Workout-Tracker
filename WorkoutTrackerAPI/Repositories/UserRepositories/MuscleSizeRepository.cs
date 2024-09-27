using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Repositories;

public class MuscleSizeRepository : BaseRepository<MuscleSize>
{
    public MuscleSizeRepository(WorkoutDbContext db) : base(db)
    {

    }
}