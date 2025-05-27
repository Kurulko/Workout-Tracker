using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Muscles;

internal class MuscleRepository : BaseWorkoutRepository<Muscle>, IMuscleRepository
{
    public MuscleRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Muscle> GetMuscles()
        => dbSet.Include(m => m.ParentMuscle)
        .Include(m => m.ChildMuscles);

    public override IQueryable<Muscle> GetAll()
        =>GetMuscles();

    public override async Task<Muscle?> GetByIdAsync(long key, CancellationToken cancellationToken)
    {
        return await dbSet
          .Where(w => w.Id == key)
          .Include(m => m.ParentMuscle)
          .Include(m => m.ChildMuscles)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public override async Task<Muscle?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.ParentMuscle)
          .Include(m => m.ChildMuscles)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Muscle?> GetMuscleByIdWithDetailsAsync(long key, string userId, CancellationToken cancellationToken)
    {
        var muscle = await dbSet
         .Where(w => w.Id == key)
         .Include(m => m.ParentMuscle)
         .Include(m => m.ChildMuscles)
         .Include(m => m.Exercises)!
            .ThenInclude(e => e.Equipments)
         .Include(m => m.MuscleSizes)
         .FirstOrDefaultAsync(cancellationToken);

        if (muscle != null)
        {
            muscle.MuscleSizes = muscle.MuscleSizes?
                .Where(er => er.UserId == userId)
                .ToList();
        }

        return muscle;
    }

    public async Task<Muscle?> GetMuscleByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken)
    {
        var muscle = await dbSet
         .Where(w => w.Name == name)
         .Include(m => m.ParentMuscle)
         .Include(m => m.ChildMuscles)
         .Include(m => m.Exercises)!
            .ThenInclude(e => e.Equipments)
         .Include(m => m.MuscleSizes)
         .FirstOrDefaultAsync(cancellationToken);

        if (muscle != null)
        {
            muscle.MuscleSizes = muscle.MuscleSizes?
                .Where(er => er.UserId == userId)
                .ToList();
        }

        return muscle;
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
        => Find(m => m.ChildMuscles != null && m.ChildMuscles.Count() != 0);

    public IQueryable<Muscle> GetChildMuscles()
        => Find(m => m.ParentMuscleId != null);
}