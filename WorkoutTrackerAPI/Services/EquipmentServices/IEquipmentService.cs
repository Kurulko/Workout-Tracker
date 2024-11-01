using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Services.EquipmentServices;

public interface IEquipmentService
{
    Task<ServiceResult<Equipment>> GetEquipmentByIdAsync(long equipmentId);
    Task<ServiceResult<Equipment>> GetUserEquipmentByIdAsync(string userId, long equipmentId);

    Task<ServiceResult<Equipment>> GetEquipmentByNameAsync(string name);
    Task<ServiceResult<Equipment>> GetUserEquipmentByNameAsync(string userId, string name);

    Task<ServiceResult<IQueryable<Equipment>>> GetEquipmentsAsync();
    Task<ServiceResult<IQueryable<Equipment>>> GetUserEquipmentsAsync(string userId);

    Task<ServiceResult<Equipment>> AddEquipmentAsync(Equipment model);
    Task<ServiceResult<Equipment>> AddUserEquipmentAsync(string userId, Equipment model);

    Task<ServiceResult> UpdateEquipmentAsync(Equipment model);
    Task<ServiceResult> UpdateUserEquipmentAsync(string userId, Equipment model);

    Task<ServiceResult> DeleteEquipmentAsync(long equipmentId);
    Task<ServiceResult> DeleteEquipmentFromUserAsync(string userId, long equipmentId);

    Task<bool> EquipmentExistsAsync(long equipmentId);
    Task<bool> UserEquipmentExistsAsync(string userId, long equipmentId);

    Task<bool> EquipmentExistsByNameAsync(string name);
    Task<bool> UserEquipmentExistsByNameAsync(string userId, string name);
}
