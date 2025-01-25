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
        => dbSet.Include(m => m.WorkingMuscles)
        .Include(m => m.Equipments);

    public override Task<IQueryable<Exercise>> GetAllAsync()
        => Task.FromResult(GetExercises());

    public override async Task<Exercise?> GetByIdAsync(long key)
    {
        return await dbSet
          .Where(w => w.Id == key)
          .Include(m => m.WorkingMuscles)
          .Include(m => m.Equipments)
          .FirstOrDefaultAsync();
    }

    public override async Task<Exercise?> GetByNameAsync(string name)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.WorkingMuscles)
          .Include(m => m.Equipments)
          .FirstOrDefaultAsync();
    }

    public async Task<Exercise?> GetExerciseByIdWithDetailsAsync(long key, string userId)
    {
        var exercise = await dbSet
         .Where(w => w.Id == key)
         .Include(m => m.WorkingMuscles)
         .Include(m => m.Equipments)
         .Include(m => m.ExerciseRecords)
         .FirstOrDefaultAsync();

        if (exercise != null)
        {
            exercise.ExerciseRecords = exercise.ExerciseRecords?
                .Where(er => er.UserId == userId)
                .ToList();
        }

        return exercise;
    }

    public async Task<Exercise?> GetExerciseByNameWithDetailsAsync(string name, string userId)
    {
        var exercise = await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.WorkingMuscles)
          .Include(m => m.Equipments)
          .Include(m => m.ExerciseRecords)
          .FirstOrDefaultAsync();

        if (exercise != null)
        {
            exercise.ExerciseRecords = exercise.ExerciseRecords?
                .Where(er => er.UserId == userId)
                .ToList();
        }

        return exercise;
    }
}