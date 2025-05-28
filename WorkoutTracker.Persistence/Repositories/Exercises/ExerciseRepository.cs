using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Domain.Enums;
using System.Linq.Expressions;

namespace WorkoutTracker.Persistence.Repositories.Exercises;

internal class ExerciseRepository : BaseWorkoutRepository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(WorkoutDbContext db) : base(db)
    {

    }

    public override IQueryable<Exercise> GetAll()
        => IncludeExercise(dbSet);

    public override async Task<Exercise?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        return await IncludeExercise(dbSet.Where(w => w.Id == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override async Task<Exercise?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await IncludeExercise(dbSet.Where(w => w.Name == name))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Exercise?> GetExerciseByIdWithDetailsAsync(long key, string userId, CancellationToken cancellationToken)
    {
        return await IncludeExerciseDetails(dbSet.Where(w => w.Id == key), userId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Exercise?> GetExerciseByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken)
    {
        return await IncludeExerciseDetails(dbSet.Where(w => w.Name == name), userId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override IQueryable<Exercise> Find(Expression<Func<Exercise, bool>> expression)
    {
        return IncludeExercise(dbSet.Where(expression));
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

    static IQueryable<Exercise> IncludeExercise(IQueryable<Exercise> query)
    {
        return query
            .Include(m => m.ExerciseAliases)
            .Include(m => m.WorkingMuscles)
            .Include(m => m.Equipments)
            .AsSplitQuery();
    }

    static IQueryable<Exercise> IncludeExerciseDetails(IQueryable<Exercise> query, string userId)
    {
        return query
            .Include(m => m.ExerciseAliases)
            .Include(m => m.WorkingMuscles)
            .Include(m => m.Equipments)
            .Include(m => m.ExerciseRecords!.Where(er => er.ExerciseRecordGroup.WorkoutRecord.UserId == userId))

            .AsSplitQuery();
    }
}