using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Exercises.ExerciseRecords;

internal class ExerciseRecordRepository : DbModelRepository<ExerciseRecord>, IExerciseRecordRepository
{
    public ExerciseRecordRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<ExerciseRecord> GetExerciseRecords()
       => dbSet.Include(m => m.Exercise);

    public override Task<IQueryable<ExerciseRecord>> GetAllAsync()
        => Task.FromResult(GetExerciseRecords());
}
