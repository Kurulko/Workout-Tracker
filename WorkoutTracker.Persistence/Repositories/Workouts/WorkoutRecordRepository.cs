using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Workouts;

internal class WorkoutRecordRepository : DbModelRepository<WorkoutRecord>, IWorkoutRecordRepository
{
    public WorkoutRecordRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<WorkoutRecord> GetWorkoutRecords()
        => dbSet.Include(m => m.Workout)
        .Include(m => m.ExerciseRecordGroups)
        .ThenInclude(erg => erg.ExerciseRecords)
        .ThenInclude(er => er.Exercise);

    public override Task<IQueryable<WorkoutRecord>> GetAllAsync()
        => Task.FromResult(GetWorkoutRecords());

}