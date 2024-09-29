using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Repositories;

public class BodyWeightRepository : DbModelRepository<BodyWeight>
{
    public BodyWeightRepository(WorkoutDbContext db) : base(db)
    {

    }
}
