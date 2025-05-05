using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Exercises;

internal class ExerciseRepository : BaseWorkoutRepository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Exercise> GetExercises()
        => dbSet.Include(m => m.ExerciseAliases)
        .Include(m => m.WorkingMuscles)
        .Include(m => m.Equipments);

    public override Task<IQueryable<Exercise>> GetAllAsync()
        => Task.FromResult(GetExercises());

    public override async Task<Exercise?> GetByIdAsync(long key)
    {
        return await dbSet
          .Where(w => w.Id == key)
          .Include(m => m.ExerciseAliases)
          .Include(m => m.WorkingMuscles)
          .Include(m => m.Equipments)
          .FirstOrDefaultAsync();
    }

    public override async Task<Exercise?> GetByNameAsync(string name)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.ExerciseAliases)
          .Include(m => m.WorkingMuscles)
          .Include(m => m.Equipments)
          .FirstOrDefaultAsync();
    }

    public async Task<Exercise?> GetExerciseByIdWithDetailsAsync(long key, string userId)
    {
        var exercise = await dbSet
         .Where(w => w.Id == key)
         .Include(m => m.ExerciseAliases)
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
          .Include(m => m.ExerciseAliases)
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