using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories;

internal class EquipmentRepository : BaseWorkoutRepository<Equipment>, IEquipmentRepository
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