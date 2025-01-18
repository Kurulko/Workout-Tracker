using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Repositories.UserRepositories;

public class WorkoutRecordRepository : DbModelRepository<WorkoutRecord>
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