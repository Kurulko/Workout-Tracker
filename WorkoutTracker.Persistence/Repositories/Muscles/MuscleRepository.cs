using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Muscles;

internal class MuscleRepository : BaseWorkoutRepository<Muscle>, IMuscleRepository
{
    public MuscleRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Muscle> GetMuscles()
        => dbSet.Include(m => m.ParentMuscle)
        .Include(m => m.ChildMuscles);

    public override Task<IQueryable<Muscle>> GetAllAsync()
        => Task.FromResult(GetMuscles());

    public override async Task<Muscle?> GetByIdAsync(long key)
    {
        return await dbSet
          .Where(w => w.Id == key)
          .Include(m => m.ParentMuscle)
          .Include(m => m.ChildMuscles)
          .FirstOrDefaultAsync();
    }

    public override async Task<Muscle?> GetByNameAsync(string name)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.ParentMuscle)
          .Include(m => m.ChildMuscles)
          .FirstOrDefaultAsync();
    }

    public async Task<Muscle?> GetMuscleByIdWithDetailsAsync(long key, string userId)
    {
        var muscle = await dbSet
         .Where(w => w.Id == key)
         .Include(m => m.ParentMuscle)
         .Include(m => m.ChildMuscles)
         .Include(m => m.Exercises)!
            .ThenInclude(e => e.Equipments)
         .Include(m => m.MuscleSizes)
         .FirstOrDefaultAsync();

        if (muscle != null)
        {
            muscle.MuscleSizes = muscle.MuscleSizes?
                .Where(er => er.UserId == userId)
                .ToList();
        }

        return muscle;
    }

    public async Task<Muscle?> GetMuscleByNameWithDetailsAsync(string name, string userId)
    {
        var muscle = await dbSet
         .Where(w => w.Name == name)
         .Include(m => m.ParentMuscle)
         .Include(m => m.ChildMuscles)
         .Include(m => m.Exercises)!
            .ThenInclude(e => e.Equipments)
         .Include(m => m.MuscleSizes)
         .FirstOrDefaultAsync();

        if (muscle != null)
        {
            muscle.MuscleSizes = muscle.MuscleSizes?
                .Where(er => er.UserId == userId)
                .ToList();
        }

        return muscle;
    }
}