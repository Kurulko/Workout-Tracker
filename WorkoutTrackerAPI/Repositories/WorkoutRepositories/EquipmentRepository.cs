using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Repositories.WorkoutRepositories;

public class EquipmentRepository : BaseWorkoutRepository<Equipment>
{
    public EquipmentRepository(WorkoutDbContext db) : base(db)
    {

    }
}