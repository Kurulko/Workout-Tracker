using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerAPI.Repositories;

public class MuscleRepository : BaseWorkoutRepository<Muscle>
{
    public MuscleRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Muscle> GetMuscles()
        => dbSet.Include(m => m.ParentMuscle)
        .Include(m => m.ChildMuscles)
        .Include(m => m.Exercises)
        .Include(m => m.MuscleSizes);

    public override Task<IQueryable<Muscle>> GetAllAsync()
        => Task.FromResult(GetMuscles());
}