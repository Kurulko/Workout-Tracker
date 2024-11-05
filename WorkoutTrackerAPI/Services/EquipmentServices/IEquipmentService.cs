using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Services.EquipmentServices;

public interface IEquipmentService
{
    Task<ServiceResult<Equipment>> GetInternalEquipmentByIdAsync(long equipmentId);
    Task<ServiceResult<Equipment>> GetUserEquipmentByIdAsync(string userId, long equipmentId);

    Task<ServiceResult<Equipment>> GetInternalEquipmentByNameAsync(string name);
    Task<ServiceResult<Equipment>> GetUserEquipmentByNameAsync(string userId, string name);

    Task<ServiceResult<IQueryable<Equipment>>> GetInternalEquipmentsAsync();
    Task<ServiceResult<IQueryable<Equipment>>> GetUserEquipmentsAsync(string userId);
    Task<ServiceResult<IQueryable<Equipment>>> GetAllEquipmentsAsync(string userId);

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
