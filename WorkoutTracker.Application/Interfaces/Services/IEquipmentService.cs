using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IEquipmentService : IBaseService
{
    #region Internal Equipments

    Task<Equipment?> GetInternalEquipmentByIdAsync(long equipmentId, CancellationToken cancellationToken = default);
    Task<Equipment?> GetInternalEquipmentByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Equipment?> GetInternalEquipmentByIdWithDetailsAsync(long equipmentId, CancellationToken cancellationToken = default);
    Task<Equipment?> GetInternalEquipmentByNameWithDetailsAsync(string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<Equipment>> GetInternalEquipmentsAsync(CancellationToken cancellationToken = default);

    Task<Equipment> AddInternalEquipmentAsync(Equipment model, CancellationToken cancellationToken = default);
    Task UpdateInternalEquipmentAsync(Equipment model, CancellationToken cancellationToken = default);

    Task DeleteInternalEquipmentAsync(long equipmentId, CancellationToken cancellationToken = default);

    #endregion

    #region User Equipments

    Task<Equipment?> GetUserEquipmentByIdAsync(string userId, long equipmentId, CancellationToken cancellationToken = default);
    Task<Equipment?> GetUserEquipmentByIdWithDetailsAsync(string userId, long equipmentId, CancellationToken cancellationToken = default);

    Task<Equipment?> GetUserEquipmentByNameAsync(string userId, string name, CancellationToken cancellationToken = default);
    Task<Equipment?> GetUserEquipmentByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<Equipment>> GetUserEquipmentsAsync(string userId, CancellationToken cancellationToken = default);

    Task<Equipment> AddUserEquipmentAsync(string userId, Equipment model, CancellationToken cancellationToken = default);
    Task UpdateUserEquipmentAsync(string userId, Equipment model, CancellationToken cancellationToken = default);

    Task DeleteEquipmentFromUserAsync(string userId, long equipmentId, CancellationToken cancellationToken = default);

    #endregion

    #region All Equipments

    Task<Equipment?> GetEquipmentByIdAsync(string userId, long equipmentId, CancellationToken cancellationToken = default);
    Task<Equipment?> GetEquipmentByNameAsync(string userId, string name, CancellationToken cancellationToken = default);

    Task<Equipment?> GetEquipmentByIdWithDetailsAsync(string userId, long equipmentId, CancellationToken cancellationToken = default);
    Task<Equipment?> GetEquipmentByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<Equipment>> GetAllEquipmentsAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Equipment>> GetUsedEquipmentsAsync(string userId, CancellationToken cancellationToken = default);

    #endregion
}
