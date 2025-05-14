using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Extensions.Exercises;

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


    public Task<IQueryable<ExerciseRecord>> GetExerciseRecordsByUserIdAsync(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        return FindAsync(er => er.ExerciseRecordGroup.WorkoutRecord.UserId == userId);
    }

    public async Task<string?> GetUserIdByExerciseRecordIdAsync(long exerciseRecordId)
    {
        var exerciseRecord = await GetByIdAsync(exerciseRecordId);

        if (exerciseRecord is null)
            return null;

        db.Entry(exerciseRecord).Reference(g => g.ExerciseRecordGroup).Load();
        db.Entry(exerciseRecord.ExerciseRecordGroup!).Reference(w => w.WorkoutRecord).Load();

        return exerciseRecord.GetUserId();
    }
}
