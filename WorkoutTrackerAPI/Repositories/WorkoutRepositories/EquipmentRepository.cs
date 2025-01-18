using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerAPI.Repositories.WorkoutRepositories;

public class EquipmentRepository : BaseWorkoutRepository<Equipment>
{
    public EquipmentRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Equipment> GetEquipments()
       => dbSet.Include(m => m.Exercises)!.ThenInclude(e => e.WorkingMuscles);

    public override Task<IQueryable<Equipment>> GetAllAsync()
        => Task.FromResult(GetEquipments());
}