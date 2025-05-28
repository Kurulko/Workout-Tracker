using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.ValueObjects;
using WorkoutTracker.Domain.Entities.Workouts;
using System.Linq.Expressions;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;

namespace WorkoutTracker.Persistence.Repositories.Muscles;

internal class MuscleSizeRepository : DbModelRepository<MuscleSize>, IMuscleSizeRepository
{
    public MuscleSizeRepository(WorkoutDbContext db) : base(db)
    {

    }

    public override IQueryable<MuscleSize> GetAll()
        => IncludeMuscleSize(dbSet);

    public override async Task<MuscleSize?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        return await IncludeMuscleSize(dbSet.Where(w => w.Id == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override IQueryable<MuscleSize> Find(Expression<Func<MuscleSize, bool>> expression)
    {
        return IncludeMuscleSize(dbSet.Where(expression));
    }

    public IQueryable<MuscleSize> GetUserMuscleSizes(string userId, long? muscleId, DateTimeRange? range)
    {
        var userMuscleSizes = Find(wr => wr.UserId == userId);

        if (range is not null)
            userMuscleSizes = userMuscleSizes.Where(ms => ms.Date >= range.FirstDate && ms.Date <= range.LastDate);

        if (muscleId.HasValue)
            userMuscleSizes = userMuscleSizes.Where(ms => ms.MuscleId == muscleId);

        return userMuscleSizes;
    }

    public async Task<IEnumerable<MuscleSize>> GetUserMuscleSizesInInchesAsync(string userId, long? muscleId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        var userMuscleSizes = await GetUserMuscleSizes(userId, muscleId, range).ToListAsync(cancellationToken);

        foreach (var userMuscleSize in userMuscleSizes)
            userMuscleSize.Size = ModelSize.GetModelSizeInInches(userMuscleSize.Size);

        return userMuscleSizes;
    }

    public async Task<IEnumerable<MuscleSize>> GetUserMuscleSizesInCentimetersAsync(string userId, long? muscleId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        var userMuscleSizes = await GetUserMuscleSizes(userId, muscleId, range).ToListAsync(cancellationToken);

        foreach (var userMuscleSize in userMuscleSizes)
            userMuscleSize.Size = ModelSize.GetModelSizeInCentimeters(userMuscleSize.Size);

        return userMuscleSizes;
    }

    public async Task<MuscleSize?> GetMaxUserMuscleSizeAsync(string userId, long muscleId, CancellationToken cancellationToken)
    {
        var userMuscleSizes = await GetUserMuscleSizes(userId, muscleId, null).ToListAsync(cancellationToken);

        var userMaxMuscleSize = userMuscleSizes.MaxBy(bw => bw.Size);
        return userMaxMuscleSize;
    }

    public async Task<MuscleSize?> GetMinUserMuscleSizeAsync(string userId, long muscleId, CancellationToken cancellationToken)
    {
        var userMuscleSizes = await GetUserMuscleSizes(userId, muscleId, null).ToListAsync(cancellationToken);

        var userMinMuscleSize = userMuscleSizes.MinBy(bw => bw.Size);
        return userMinMuscleSize;
    }

    static IQueryable<MuscleSize> IncludeMuscleSize(IQueryable<MuscleSize> query)
    {
        return query.Include(m => m.Muscle);
    }
}