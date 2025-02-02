﻿using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerAPI.Repositories;

public class WorkoutRepository : BaseWorkoutRepository<Workout>
{
    public WorkoutRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Workout> GetWorkouts()
        => dbSet.Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise);

    public override Task<IQueryable<Workout>> GetAllAsync()
        => Task.FromResult(GetWorkouts());

    public override async Task<Workout?> GetByIdAsync(long key)
    {
        return await dbSet
          .Where(w => w.Id == key)
          .Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise)
          .FirstOrDefaultAsync();
    }

    public override async Task<Workout?> GetByNameAsync(string name)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(w => w.ExerciseSetGroups)!
            .ThenInclude(erg => erg.ExerciseSets)
            .ThenInclude(er => er.Exercise)
          .FirstOrDefaultAsync();
    }

    public async Task<Workout?> GetWorkoutByIdWithDetailsAsync(long key)
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

         .FirstOrDefaultAsync();
    }

    public async Task<Workout?> GetWorkoutByNameWithDetailsAsync(string name)
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

         .FirstOrDefaultAsync();
    }
}