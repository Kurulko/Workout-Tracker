using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Persistence.Context;
using System.Linq.Expressions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WorkoutTracker.Persistence.Repositories.Workouts;

internal class WorkoutRepository : BaseWorkoutRepository<Workout>, IWorkoutRepository
{
    public WorkoutRepository(WorkoutDbContext db) : base(db)
    {

    }

    public override IQueryable<Workout> GetAll()
        => IncludeWorkout(dbSet);

    public override async Task<Workout?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await IncludeWorkout(dbSet.Where(w => w.Id == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override async Task<Workout?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(Workout.Name));

        return await IncludeWorkout(dbSet.Where(w => w.Name == name))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Workout?> GetWorkoutByIdWithDetailsAsync(long key, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await IncludeWorkoutDetails(dbSet.Where(w => w.Id == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Workout?> GetWorkoutByNameWithDetailsAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(Workout.Name));

        return await IncludeWorkoutDetails(dbSet.Where(w => w.Name == name))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override IQueryable<Workout> Find(Expression<Func<Workout, bool>> expression)
    {
        return IncludeWorkout(dbSet.Where(expression));
    }

    public IQueryable<Workout> GetUserWorkouts(string userId, long? exerciseId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        var userWorkouts = Find(e => e.UserId == userId);

        if (exerciseId.HasValue)
        {
            userWorkouts = userWorkouts
                .Where(w => w.ExerciseSetGroups!.Any(s => s.ExerciseId == exerciseId));
        }

        return IncludeWorkout(userWorkouts);
    }

    public async Task IncreaseCountOfWorkoutsAsync(long workoutId, CancellationToken cancellationToken)
    {
        var increaseCountOfWorkoutsAction = new Action<Workout>(w =>
        {
            w.CountOfTrainings++;
        });

        await UpdatePartialAsync(workoutId, increaseCountOfWorkoutsAction, cancellationToken);
    }

    public async Task DecreaseCountOfWorkoutsAsync(long workoutId, CancellationToken cancellationToken)
    {
        var increaseCountOfWorkoutsAction = new Action<Workout>(w =>
        {
            w.CountOfTrainings--;
        });

        await UpdatePartialAsync(workoutId, increaseCountOfWorkoutsAction, cancellationToken);
    }


    static IQueryable<Workout> IncludeWorkout(IQueryable<Workout> query)
    {
        return query
            .Include(w => w.ExerciseSetGroups)!
                .ThenInclude(erg => erg.ExerciseSets)
                .ThenInclude(er => er.Exercise)
            .AsSplitQuery();
    }

    static IQueryable<Workout> IncludeWorkoutDetails(IQueryable<Workout> query)
    {
        return query
            .Include(m => m.WorkoutRecords)!
                .ThenInclude(wr => wr.ExerciseRecordGroups)
                .ThenInclude(erg => erg.ExerciseRecords)
                .ThenInclude(er => er.Exercise)

            .Include(w => w.ExerciseSetGroups)!
                .ThenInclude(erg => erg.ExerciseSets)
                .ThenInclude(er => er.Exercise)
                    .ThenInclude(e => e!.Equipments)

            .Include(w => w.ExerciseSetGroups)!
                .ThenInclude(erg => erg.ExerciseSets)
                .ThenInclude(er => er.Exercise)
                    .ThenInclude(e => e!.WorkingMuscles)

            .AsSplitQuery();
    }
}