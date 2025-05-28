using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Persistence.Context;
using System.Linq.Expressions;

namespace WorkoutTracker.Persistence.Repositories.Muscles;

internal class MuscleRepository : BaseWorkoutRepository<Muscle>, IMuscleRepository
{
    public MuscleRepository(WorkoutDbContext db) : base(db)
    {

    }

    public override IQueryable<Muscle> GetAll()
        => IncludeMuscle(dbSet);

    public override async Task<Muscle?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        return await IncludeMuscle(dbSet.Where(w => w.Id == key))
          .SingleOrDefaultAsync(cancellationToken);
    }

    public override async Task<Muscle?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await IncludeMuscle(dbSet.Where(w => w.Name == name))
          .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Muscle?> GetMuscleByIdWithDetailsAsync(long key, string userId, CancellationToken cancellationToken)
    {
        return await IncludeMuscleDetails(dbSet.Where(w => w.Id == key), userId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Muscle?> GetMuscleByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken)
    {
        return await IncludeMuscleDetails(dbSet.Where(w => w.Name == name), userId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public override IQueryable<Muscle> Find(Expression<Func<Muscle, bool>> expression)
    {
        return IncludeMuscle(dbSet.Where(expression));
    }

    public IQueryable<Muscle> FindByIds(IEnumerable<long> muscleIds)
        => Find(m => muscleIds.Contains(m.Id));

    public IQueryable<Muscle> GetMuscles(long? parentMuscleId, bool? isMeasurable)
    {
        var muscles = GetAll();

        if (parentMuscleId.HasValue)
            muscles = muscles.Where(m => m.ParentMuscleId == parentMuscleId);

        if (isMeasurable.HasValue)
            muscles = muscles.Where(m => m.IsMeasurable == isMeasurable);

        return muscles;
    }

    public IQueryable<Muscle> GetParentMuscles()
        => Find(m => m.ChildMuscles != null && m.ChildMuscles.Any());

    public IQueryable<Muscle> GetChildMuscles()
        => Find(m => m.ParentMuscleId != null);

    static IQueryable<Muscle> IncludeMuscle(IQueryable<Muscle> query)
    {
        return query
            .Include(m => m.ParentMuscle)
            .Include(m => m.ChildMuscles)
            .AsSplitQuery();
    }

    static IQueryable<Muscle> IncludeMuscleDetails(IQueryable<Muscle> query, string userId)
    {
        return query
            .Include(m => m.ParentMuscle)
            .Include(m => m.ChildMuscles)
            .Include(m => m.Exercises)!
                .ThenInclude(e => e.Equipments)
            .Include(m => m.MuscleSizes!.Where(ms => ms.UserId == userId))
            .AsSplitQuery();
    }
}