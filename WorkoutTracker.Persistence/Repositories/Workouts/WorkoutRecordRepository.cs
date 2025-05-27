using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Models;

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

    public override IQueryable<WorkoutRecord> GetAll()
        => GetWorkoutRecords();


    public IQueryable<WorkoutRecord> GetUserWorkoutRecords(string userId, long? workoutId, DateTimeRange? range)
    {
        var userWorkoutRecords = Find(wr => wr.UserId == userId);

        if (range is not null)
            userWorkoutRecords = userWorkoutRecords.Where(wr => wr.Date >= range.FirstDate && wr.Date <= range.LastDate);

        if (workoutId.HasValue)
            userWorkoutRecords = userWorkoutRecords.Where(wr => wr.WorkoutId == workoutId);

        return userWorkoutRecords;
    }
}