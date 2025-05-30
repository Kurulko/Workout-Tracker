using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Persistence.Repositories;

internal class EquipmentRepository : BaseWorkoutRepository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(WorkoutDbContext db) : base(db)
    {

    }

    public async Task UpdateEquipmentPhotoAsync(long key, string image, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        var photoUpdateAction = new Action<Equipment>(m => m.Image = image);
        await UpdatePartialAsync(key, photoUpdateAction, cancellationToken);
    }

    public async Task DeleteEquipmentPhotoAsync(long key, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        var photoDeleteAction = new Action<Equipment>(m => m.Image = null);
        await UpdatePartialAsync(key, photoDeleteAction, cancellationToken);
    }

    public async Task<string?> GetEquipmentPhotoAsync(long key, CancellationToken cancellationToken)
    {
        var muscle = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, key, entityName, cancellationToken);
        return muscle.Image;
    }

    public async Task<Equipment?> GetEquipmentByIdWithDetailsAsync(long key, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(key, entityName);

        return await IncludeEquipmentDetails(dbSet.Where(w => w.Id == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Equipment?> GetEquipmentByNameWithDetailsAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(Equipment.Name));

        return await IncludeEquipmentDetails(dbSet.Where(w => w.Name == name))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public IQueryable<Equipment> FindByIds(IEnumerable<long> equipmentIds)
        => Find(m => equipmentIds.Contains(m.Id));

    public IQueryable<Equipment> FindInternalByIds(IEnumerable<long> equipmentIds)
        => FindByIds(equipmentIds).Where(e => e.OwnedByUserId == null);

    public IQueryable<Equipment> GetInternalEquipments()
        => Find(e => e.OwnedByUserId == null);

    public IQueryable<Equipment> GetUserEquipments(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        return Find(e => e.OwnedByUserId == userId);
    }
    
    public IQueryable<Equipment> GetAllEquipments(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        return Find(e => e.OwnedByUserId == userId || e.OwnedByUserId == null);
    }

    static IQueryable<Equipment> IncludeEquipmentDetails(IQueryable<Equipment> query)
    {
        return query
           .Include(m => m.Exercises)!
                .ThenInclude(e => e.WorkingMuscles)
            .AsSplitQuery();
    }
}