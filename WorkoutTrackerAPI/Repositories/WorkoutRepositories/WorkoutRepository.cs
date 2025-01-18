using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Repositories;

public class WorkoutRepository : BaseWorkoutRepository<Workout>
{
    public WorkoutRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Workout> GetWorkouts()
        => dbSet.Include(m => m.WorkoutRecords)!
        .ThenInclude(wr => wr.ExerciseRecordGroups)
        .ThenInclude(erg => erg.ExerciseRecords)
        .ThenInclude(er => er.Exercise)
        .ThenInclude(er => er!.Equipments)

        .Include(m => m.WorkoutRecords)!
        .ThenInclude(wr => wr.ExerciseRecordGroups)
        .ThenInclude(erg => erg.ExerciseRecords)
        .ThenInclude(er => er.Exercise)
        .ThenInclude(er => er!.WorkingMuscles)

        .Include(w => w.ExerciseSetGroups)!
        .ThenInclude(erg => erg.ExerciseSets)
        .ThenInclude(er => er.Exercise)
        .ThenInclude(er => er!.Equipments)

        .Include(w => w.ExerciseSetGroups)!
        .ThenInclude(erg => erg.ExerciseSets)
        .ThenInclude(er => er.Exercise)
        .ThenInclude(er => er!.WorkingMuscles);

    public override Task<IQueryable<Workout>> GetAllAsync()
        => Task.FromResult(GetWorkouts());
}