using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IEquipmentService : IBaseService
{
    Task<ServiceResult<Equipment>> GetInternalEquipmentByIdAsync(long equipmentId, bool withDetails = false);
    Task<ServiceResult<Equipment>> GetUserEquipmentByIdAsync(string userId, long equipmentId, bool withDetails = false);
    Task<ServiceResult<Equipment>> GetEquipmentByIdAsync(string userId, long equipmentId, bool withDetails = false);

    Task<ServiceResult<Equipment>> GetInternalEquipmentByNameAsync(string name, bool withDetails = false);
    Task<ServiceResult<Equipment>> GetUserEquipmentByNameAsync(string userId, string name, bool withDetails = false);
    Task<ServiceResult<Equipment>> GetEquipmentByNameAsync(string userId, string name, bool withDetails = false);

    Task<ServiceResult<IQueryable<Equipment>>> GetInternalEquipmentsAsync();
    Task<ServiceResult<IQueryable<Equipment>>> GetUserEquipmentsAsync(string userId);
    Task<ServiceResult<IQueryable<Equipment>>> GetAllEquipmentsAsync(string userId);
    Task<ServiceResult<IQueryable<Equipment>>> GetUsedEquipmentsAsync(string userId);

    Task<ServiceResult<Equipment>> AddInternalEquipmentAsync(Equipment model);
    Task<ServiceResult<Equipment>> AddUserEquipmentAsync(string userId, Equipment model);

    Task<ServiceResult> UpdateInternalEquipmentAsync(Equipment model);
    Task<ServiceResult> UpdateUserEquipmentAsync(string userId, Equipment model);

    Task<ServiceResult> DeleteInternalEquipmentAsync(long equipmentId);
    Task<ServiceResult> DeleteEquipmentFromUserAsync(string userId, long equipmentId);

    Task<bool> InternalEquipmentExistsAsync(long equipmentId);
    Task<bool> UserEquipmentExistsAsync(string userId, long equipmentId);

    Task<bool> InternalEquipmentExistsByNameAsync(string name);
    Task<bool> UserEquipmentExistsByNameAsync(string userId, string name);
}
