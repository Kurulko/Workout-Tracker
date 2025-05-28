using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Extensions.Exercises;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Domain.Entities.Workouts;
using System.Linq.Expressions;

namespace WorkoutTracker.Persistence.Repositories.Exercises.ExerciseRecords;

internal class ExerciseRecordRepository : DbModelRepository<ExerciseRecord>, IExerciseRecordRepository
{
    public ExerciseRecordRepository(WorkoutDbContext db) : base(db)
    {

    }

    public override IQueryable<ExerciseRecord> GetAll()
        => IncludeExerciseRecord(dbSet);

    public override async Task<ExerciseRecord?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        return await IncludeExerciseRecord(dbSet.Where(w => w.Id == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override IQueryable<ExerciseRecord> Find(Expression<Func<ExerciseRecord, bool>> expression)
    {
        return IncludeExerciseRecord(dbSet.Where(expression));
    }

    public IQueryable<ExerciseRecord> GetUserExerciseRecords(string userId, long? exerciseId, ExerciseType? exerciseType, DateTimeRange? range)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        var exerciseRecords = Find(er => er.ExerciseRecordGroup.WorkoutRecord.UserId == userId);

        if (range is not null)
            exerciseRecords = exerciseRecords.Where(ms => ms.Date >= range.FirstDate && ms.Date <= range.LastDate);

        if (exerciseId.HasValue)
            exerciseRecords = exerciseRecords.Where(ms => ms.ExerciseId == exerciseId);
        else if (exerciseType.HasValue)
            exerciseRecords = exerciseRecords.Where(ms => ms.Exercise!.Type == exerciseType);

        return exerciseRecords;
    }

    public async Task<string?> GetUserIdByExerciseRecordIdAsync(long exerciseRecordId, CancellationToken cancellationToken = default)
    {
        var exerciseRecord = await GetByIdAsync(exerciseRecordId, cancellationToken);

        if (exerciseRecord is null)
            return null;

        db.Entry(exerciseRecord).Reference(g => g.ExerciseRecordGroup).Load();
        db.Entry(exerciseRecord.ExerciseRecordGroup!).Reference(w => w.WorkoutRecord).Load();

        return exerciseRecord.GetUserId();
    }

    static IQueryable<ExerciseRecord> IncludeExerciseRecord(IQueryable<ExerciseRecord> query)
    {
        return query.Include(m => m.Exercise);
    }
}
