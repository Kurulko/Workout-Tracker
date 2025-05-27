using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories;

internal class EquipmentRepository : BaseWorkoutRepository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(WorkoutDbContext db) : base(db)
    {

    }

    public async Task<Equipment?> GetEquipmentByIdWithDetailsAsync(long key, CancellationToken cancellationToken)
    {
        return await dbSet
          .Where(w => w.Id == key)
           .Include(m => m.Exercises)!.
            ThenInclude(e => e.WorkingMuscles)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Equipment?> GetEquipmentByNameWithDetailsAsync(string name, CancellationToken cancellationToken)
    {
        return await dbSet
          .Where(w => w.Name == name)
          .Include(m => m.Exercises)!.
            ThenInclude(e => e.WorkingMuscles)
          .FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<Equipment> FindByIds(IEnumerable<long> equipmentIds)
        => Find(m => equipmentIds.Contains(m.Id));

    public IQueryable<Equipment> FindInternalByIds(IEnumerable<long> equipmentIds)
        => FindByIds(equipmentIds).Where(e => e.OwnedByUserId == null);

    public IQueryable<Equipment> GetInternalEquipments()
        => Find(e => e.OwnedByUserId == null);
    public IQueryable<Equipment> GetUserEquipments(string userId)
        => Find(e => e.OwnedByUserId == userId);
    public IQueryable<Equipment> GetAllEquipments(string userId)
        => Find(e => e.OwnedByUserId == userId || e.OwnedByUserId == null);
}