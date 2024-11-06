using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerAPI.Repositories;

public class ExerciseRepository : BaseWorkoutRepository<Exercise>
{
    public ExerciseRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Exercise> GetExercises()
        => dbSet.Include(m => m.WorkingMuscles);

    public override Task<IQueryable<Exercise>> GetAllAsync()
        => Task.FromResult(GetExercises());

    public override async Task<Exercise> AddAsync(Exercise model)
    {
        if (model.WorkingMuscles is not null)
        {
            var muscleIds = model.WorkingMuscles.Select(c => c.Id).ToList();

            var existingMuscles = await db.Muscles
                .Where(m => muscleIds.Contains(m.Id))
                .ToListAsync();

            model.WorkingMuscles = existingMuscles;
        }

        return await base.AddAsync(model);
    }
}