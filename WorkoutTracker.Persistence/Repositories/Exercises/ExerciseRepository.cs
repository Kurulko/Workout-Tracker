using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Domain.Enums;
using System.Linq.Expressions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Persistence.Repositories.Exercises;

internal class ExerciseRepository : BaseWorkoutRepository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(WorkoutDbContext db) : base(db)
    {

    }

    public override IQueryable<Exercise> GetAll()
        => IncludeExercise(dbSet);

    public async Task UpdateExercisePhotoAsync(long key, string image, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        var photoUpdateAction = new Action<Exercise>(m => m.Image = image);
        await UpdatePartialAsync(key, photoUpdateAction, cancellationToken);
    }

    public async Task DeleteExercisePhotoAsync(long key, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        var photoDeleteAction = new Action<Exercise>(m => m.Image = null);
        await UpdatePartialAsync(key, photoDeleteAction, cancellationToken);
    }

    public async Task<string?> GetExercisePhotoAsync(long key, CancellationToken cancellationToken)
    {
        var muscle = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, key, entityName, cancellationToken);
        return muscle.Image;
    }

    public override async Task<Exercise?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await IncludeExercise(dbSet.Where(w => w.Id == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override async Task<Exercise?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(Muscle.Name));

        return await IncludeExercise(dbSet.Where(w => w.Name == name))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Exercise?> GetExerciseByIdWithDetailsAsync(long key, string userId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await IncludeExerciseDetails(dbSet.Where(w => w.Id == key), userId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Exercise?> GetExerciseByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(Exercise.Name));

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
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        var exercises = Find(e => e.CreatedByUserId == userId);
        exercises = FilterByExerciseType(exercises, exerciseType);

        return exercises;
    }

    public IQueryable<Exercise> GetAllExercises(string userId, ExerciseType? exerciseType)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

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
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        return query
            .Include(m => m.ExerciseAliases)
            .Include(m => m.WorkingMuscles)
            .Include(m => m.Equipments)
            .Include(m => m.ExerciseRecords!.Where(er => er.ExerciseRecordGroup.WorkoutRecord.UserId == userId))
            .AsSplitQuery();
    }
}