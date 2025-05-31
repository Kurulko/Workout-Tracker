using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Models;
using System.Linq.Expressions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Persistence.Repositories.Workouts;

internal class WorkoutRecordRepository : DbModelRepository<WorkoutRecord>, IWorkoutRecordRepository
{
    public WorkoutRecordRepository(WorkoutDbContext db) : base(db)
    {

    }

    public override IQueryable<WorkoutRecord> GetAll()
        => IncludeWorkoutRecord(dbSet);

    public override async Task<WorkoutRecord?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await IncludeWorkoutRecord(dbSet.Where(w => w.Id == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override IQueryable<WorkoutRecord> Find(Expression<Func<WorkoutRecord, bool>> expression)
    {
        return IncludeWorkoutRecord(dbSet.Where(expression));
    }


    public IQueryable<WorkoutRecord> GetUserWorkoutRecords(string userId, long? workoutId, DateTimeRange? range)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        var userWorkoutRecords = Find(wr => wr.UserId == userId);

        if (range is not null)
            userWorkoutRecords = userWorkoutRecords.Where(wr => wr.Date >= range.FirstDate && wr.Date <= range.LastDate);

        if (workoutId.HasValue)
            userWorkoutRecords = userWorkoutRecords.Where(wr => wr.WorkoutId == workoutId);

        return userWorkoutRecords;
    }

    public async Task<DateTime?> GetFirstWorkoutDateAsync(string userId, CancellationToken cancellationToken)
    {
        return await GetUserWorkoutRecords(userId, null, null)
            .OrderBy(wr => wr.Date)
            .Select(wr => wr.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    static IQueryable<WorkoutRecord> IncludeWorkoutRecord(IQueryable<WorkoutRecord> query)
    {
        return query
            .Include(m => m.Workout)
            .Include(m => m.ExerciseRecordGroups)
                .ThenInclude(erg => erg.ExerciseRecords)
                .ThenInclude(er => er.Exercise)
            .AsSplitQuery();
    }
}