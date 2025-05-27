using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Workouts;

internal class WorkoutRepository : BaseWorkoutRepository<Workout>, IWorkoutRepository
{
    public WorkoutRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Workout> GetWorkouts()
        => dbSet.Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise);

    public override IQueryable<Workout> GetAll()
        => GetWorkouts();

    public override async Task<Workout?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        return await dbSet
          .Where(w => w.Id == key)
          .Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public override async Task<Workout?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Workout?> GetWorkoutByIdWithDetailsAsync(long key, CancellationToken cancellationToken)
    {
       return await dbSet
         .Where(w => w.Id == key)
         .Include(m => m.WorkoutRecords)!
            .ThenInclude(wr => wr.ExerciseRecordGroups)
            .ThenInclude(erg => erg.ExerciseRecords)
            .ThenInclude(er => er.Exercise)

          .Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise)
            .ThenInclude(er => er!.Equipments)

          .Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise)
            .ThenInclude(er => er!.WorkingMuscles)

         .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Workout?> GetWorkoutByNameWithDetailsAsync(string name, CancellationToken cancellationToken)
    {
       return await dbSet
         .Where(w => w.Name == name)
         .Include(m => m.WorkoutRecords)!
            .ThenInclude(wr => wr.ExerciseRecordGroups)
            .ThenInclude(erg => erg.ExerciseRecords)
            .ThenInclude(er => er.Exercise)

          .Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise)
            .ThenInclude(er => er!.Equipments)

          .Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise)
            .ThenInclude(er => er!.WorkingMuscles)

         .FirstOrDefaultAsync(cancellationToken);
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

    public IQueryable<Workout> GetUserWorkouts(string userId, long? exerciseId)
    {
        var userWorkouts = Find(e => e.UserId == userId);

        if (exerciseId.HasValue)
        {
            userWorkouts = userWorkouts
                .Where(w => w.ExerciseSetGroups!.Any(s => s.ExerciseId == exerciseId))
                .Include(w => w.ExerciseSetGroups!.Where(s => s.ExerciseId == exerciseId))
                .ThenInclude(s => s.Exercise);
        }

        return userWorkouts;
    }
}