using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Repositories;

public class MuscleRepository : BaseWorkoutRepository<Muscle>
{
    public MuscleRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Muscle> GetMuscles()
        => dbSet.Include(m => m.ParentMuscle).Include(m => m.ChildMuscles);

    public override Task<IQueryable<Muscle>> GetAllAsync()
        => Task.FromResult(GetMuscles());

    public override async Task<Muscle> AddAsync(Muscle model)
    {
        if (model.ChildMuscles is not null)
        {
            var childMuscleIds = model.ChildMuscles.Select(c => c.Id).ToList();

            var existingChildMuscles = await dbSet
                .Where(m => childMuscleIds.Contains(m.Id))
                .ToListAsync();

            model.ChildMuscles = existingChildMuscles;
        }

        return await base.AddAsync(model);
    }
}