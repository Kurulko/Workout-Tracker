using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Repositories;

public interface IEquipmentRepository : IBaseWorkoutRepository<Equipment>
{
    Task<Equipment?> GetEquipmentByIdWithDetailsAsync(long key, CancellationToken cancellationToken = default);
    Task<Equipment?> GetEquipmentByNameWithDetailsAsync(string name, CancellationToken cancellationToken = default);

    IQueryable<Equipment> GetInternalEquipments();
    IQueryable<Equipment> GetUserEquipments(string userId);
    IQueryable<Equipment> GetAllEquipments(string userId);

    IQueryable<Equipment> FindByIds(IEnumerable<long> equipmentIds);
    IQueryable<Equipment> FindInternalByIds(IEnumerable<long> equipmentIds);

    Task<string?> GetEquipmentPhotoAsync(long key, CancellationToken cancellationToken = default);
    Task UpdateEquipmentPhotoAsync(long key, string image, CancellationToken cancellationToken = default);
    Task DeleteEquipmentPhotoAsync(long key, CancellationToken cancellationToken = default);
}