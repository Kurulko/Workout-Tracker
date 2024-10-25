using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerAPI.Repositories;

public class MuscleRepository : BaseWorkoutRepository<Muscle>
{
    public MuscleRepository(WorkoutDbContext db) : base(db)
    {

    }

    public override Task<IQueryable<Muscle>> GetAllAsync()
        => Task.FromResult((IQueryable<Muscle>)dbSet.Include(m => m.ChildMuscles));

    public override async Task<Muscle?> GetByIdAsync(long key)
        => await dbSet.Include(m => m.ChildMuscles).SingleOrDefaultAsync(m => m.Id == key);

    public override async Task<Muscle?> GetByNameAsync(string name)
        => await dbSet.Include(m => m.ChildMuscles).SingleOrDefaultAsync(m => m.Name == name);
}