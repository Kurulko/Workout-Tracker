using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerAPI.Repositories.WorkoutRepositories;

public class EquipmentRepository : BaseWorkoutRepository<Equipment>
{
    public EquipmentRepository(WorkoutDbContext db) : base(db)
    {

    }

    public async Task<Equipment?> GetEquipmentByIdWithDetailsAsync(long key)
    {
        return await dbSet
          .Where(w => w.Id == key)
           .Include(m => m.Exercises)!.
            ThenInclude(e => e.WorkingMuscles)
          .FirstOrDefaultAsync();
    }

    public async Task<Equipment?> GetEquipmentByNameWithDetailsAsync(string name)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.Exercises)!.
            ThenInclude(e => e.WorkingMuscles)
          .FirstOrDefaultAsync();
    }
}