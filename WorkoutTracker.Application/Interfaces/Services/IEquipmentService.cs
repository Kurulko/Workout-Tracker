using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IEquipmentService : IBaseService
{
    Task<Equipment?> GetInternalEquipmentByIdAsync(long equipmentId, bool withDetails = false);
    Task<Equipment?> GetUserEquipmentByIdAsync(string userId, long equipmentId, bool withDetails = false);
    Task<Equipment?> GetEquipmentByIdAsync(string userId, long equipmentId, bool withDetails = false);

    Task<Equipment?> GetInternalEquipmentByNameAsync(string name, bool withDetails = false);
    Task<Equipment?> GetUserEquipmentByNameAsync(string userId, string name, bool withDetails = false);
    Task<Equipment?> GetEquipmentByNameAsync(string userId, string name, bool withDetails = false);

    Task<IQueryable<Equipment>> GetInternalEquipmentsAsync();
    Task<IQueryable<Equipment>> GetUserEquipmentsAsync(string userId);
    Task<IQueryable<Equipment>> GetAllEquipmentsAsync(string userId);
    Task<IQueryable<Equipment>> GetUsedEquipmentsAsync(string userId);

    Task<Equipment> AddInternalEquipmentAsync(Equipment model);
    Task<Equipment> AddUserEquipmentAsync(string userId, Equipment model);

    Task UpdateInternalEquipmentAsync(Equipment model);
    Task UpdateUserEquipmentAsync(string userId, Equipment model);

    Task DeleteInternalEquipmentAsync(long equipmentId);
    Task DeleteEquipmentFromUserAsync(string userId, long equipmentId);
}
