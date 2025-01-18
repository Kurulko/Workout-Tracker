using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Repositories;

public class ExerciseRecordRepository : DbModelRepository<ExerciseRecord>
{
    public ExerciseRecordRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<ExerciseRecord> GetExerciseRecords()
       => dbSet.Include(m => m.Exercise);

    public override Task<IQueryable<ExerciseRecord>> GetAllAsync()
        => Task.FromResult(GetExerciseRecords());
}
