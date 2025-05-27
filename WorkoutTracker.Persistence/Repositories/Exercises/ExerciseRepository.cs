using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Extensions.Exercises;
using WorkoutTracker.Domain.Enums;

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

    public override IQueryable<Exercise> GetAll()
        => GetExercises();

    public override async Task<Exercise?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        return await dbSet
          .Where(w => w.Id == key)
          .Include(m => m.ExerciseAliases)
          .Include(m => m.WorkingMuscles)
          .Include(m => m.Equipments)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public override async Task<Exercise?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.ExerciseAliases)
          .Include(m => m.WorkingMuscles)
          .Include(m => m.Equipments)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Exercise?> GetExerciseByIdWithDetailsAsync(long key, string userId, CancellationToken cancellationToken)
    {
        var exercise = await dbSet
         .Where(w => w.Id == key)
         .Include(m => m.ExerciseAliases)
         .Include(m => m.WorkingMuscles)
         .Include(m => m.Equipments)
         .Include(m => m.ExerciseRecords)
         .FirstOrDefaultAsync(cancellationToken);

        if (exercise != null)
        {
            exercise.ExerciseRecords = exercise.ExerciseRecords?
                .Where(er => er.GetUserId() == userId)
                .ToList();
        }

        return exercise;
    }

    public async Task<Exercise?> GetExerciseByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken)
    {
        var exercise = await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.ExerciseAliases)
          .Include(m => m.WorkingMuscles)
          .Include(m => m.Equipments)
          .Include(m => m.ExerciseRecords)
          .FirstOrDefaultAsync(cancellationToken);

        if (exercise != null)
        {
            exercise.ExerciseRecords = exercise.ExerciseRecords?
                .Where(er => er.GetUserId() == userId)
                .ToList();
        }

        return exercise;
    }

    public IQueryable<Exercise> GetInternalExercises(ExerciseType? exerciseType)
    {
        var exercises = Find(e => e.CreatedByUserId == null);
        exercises = FilterByExerciseType(exercises, exerciseType);

        return exercises;
    }

    public IQueryable<Exercise> GetUserExercises(string userId, ExerciseType? exerciseType)
    {
        var exercises = Find(e => e.CreatedByUserId == userId);
        exercises = FilterByExerciseType(exercises, exerciseType);

        return exercises;
    }

    public IQueryable<Exercise> GetAllExercises(string userId, ExerciseType? exerciseType)
    {
        var exercises = Find(e => e.CreatedByUserId == userId || e.CreatedByUserId == null);
        exercises = FilterByExerciseType(exercises, exerciseType);

        return exercises;
    }

    static IQueryable<Exercise> FilterByExerciseType(IQueryable<Exercise> exercises, ExerciseType? exerciseType)
    {
        if (exerciseType.HasValue)
            exercises = exercises.Where(e => e.Type == exerciseType);

        return exercises;
    }
}