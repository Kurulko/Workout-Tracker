using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.EquipmentServices;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;

namespace WorkoutTrackerAPI.Services.EquipmentServices;

public class EquipmentService : BaseWorkoutService<Equipment>, IEquipmentService
{
    readonly UserRepository userRepository;
    public EquipmentService(EquipmentRepository baseWorkoutRepository, UserRepository userRepository) : base(baseWorkoutRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException equipmentIsNullException = new(nameof(Equipment));
    readonly InvalidIDException invalidEquipmentIDException = new(nameof(Equipment));
    readonly NotFoundException equipmentNotFoundException = new(nameof(Equipment));
    readonly ArgumentNullOrEmptyException equipmentNameIsNullOrEmptyException = new("Equipment name");

    ArgumentException InvalidEquipmentIDWhileAddingException => InvalidEntryIDWhileAddingException(nameof(Equipment), "equipment");

    public async Task<ServiceResult<Equipment>> AddEquipmentAsync(Equipment equipment)
    {
        try
        {
            if (equipment is null)
                throw equipmentIsNullException;

            if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
                throw new UnauthorizedAccessException("Equipment entry cannot be created by user.");

            if (equipment.Id != 0)
                throw InvalidEquipmentIDWhileAddingException;

            await baseWorkoutRepository.AddAsync(equipment);
            return ServiceResult<Equipment>.Ok(equipment);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("equipment", "add", ex.Message));
        }
    }

    public async Task<ServiceResult<Equipment>> AddUserEquipmentAsync(string userId, Equipment equipment)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipment is null)
                throw equipmentIsNullException;

            if (equipment.Id != 0)
                throw InvalidEquipmentIDWhileAddingException;

            equipment.OwnedByUserId = userId;
            await baseWorkoutRepository.AddAsync(equipment);

            return ServiceResult<Equipment>.Ok(equipment);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("user equipment", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteEquipmentAsync(long equipmentId)
    {
        try
        {
            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var equipment = await baseWorkoutRepository.GetByIdAsync(equipmentId) ?? throw equipmentNotFoundException;

            if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
                throw UserNotHavePermissionException("delete", "equipment");

            await baseWorkoutRepository.RemoveAsync(equipmentId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("equipment", "delete"));
        }
    }

    public async Task<ServiceResult> DeleteEquipmentFromUserAsync(string userId, long equipmentId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var equipment = await baseWorkoutRepository.GetByIdAsync(equipmentId) ?? throw equipmentNotFoundException;

            if (equipment.OwnedByUserId != userId)
                throw UserNotHavePermissionException("delete", "equipment");

            await baseWorkoutRepository.RemoveAsync(equipmentId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("user equipment", "delete"));
        }
    }

    public async Task<bool> EquipmentExistsAsync(long equipmentId)
    {
        if (equipmentId < 1)
            throw invalidEquipmentIDException;

        return await baseWorkoutRepository.ExistsAsync(equipmentId);
    }

    public async Task<bool> EquipmentExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw equipmentNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }

    public async Task<ServiceResult<Equipment>> GetEquipmentByIdAsync(long equipmentId)
    {
        try
        {
            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var equipmentById = await baseWorkoutRepository.GetByIdAsync(equipmentId);
            return ServiceResult<Equipment>.Ok(equipmentById);
        }
        catch (ArgumentException argEx)
        {
            return ServiceResult<Equipment>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("equipment", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Equipment>> GetEquipmentByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                throw equipmentNameIsNullOrEmptyException;

            var equipmentByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Equipment>.Ok(equipmentByName);
        }
        catch (ArgumentException argEx)
        {
            return ServiceResult<Equipment>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("equipment by name", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<IQueryable<Equipment>>> GetEquipmentsAsync()
    {
        try
        {
            var equipments = await baseWorkoutRepository.GetAllAsync();
            return ServiceResult<IQueryable<Equipment>>.Ok(equipments);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(FailedToActionStr("equipments", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Equipment>> GetUserEquipmentByIdAsync(string userId, long equipmentId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var userEquipmentById = await baseWorkoutRepository.GetByIdAsync(equipmentId);
            return ServiceResult<Equipment>.Ok(userEquipmentById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("user equipment", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Equipment>> GetUserEquipmentByNameAsync(string userId, string name)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw equipmentNameIsNullOrEmptyException;

            var userEquipmentByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Equipment>.Ok(userEquipmentByName);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("user equipment by name", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<IQueryable<Equipment>>> GetUserEquipmentsAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userEquipments = await baseWorkoutRepository.FindAsync(e => e.OwnedByUserId == userId);
            return ServiceResult<IQueryable<Equipment>>.Ok(userEquipments);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(FailedToActionStr("user equipments", "get", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateEquipmentAsync(Equipment equipment)
    {
        try
        {
            if (equipment is null)
                throw equipmentIsNullException;

            if (equipment.Id < 1)
                throw invalidEquipmentIDException;

            var _equipment = await baseWorkoutRepository.GetByIdAsync(equipment.Id) ?? throw equipmentNotFoundException;

            if (!string.IsNullOrEmpty(_equipment.OwnedByUserId))
                throw UserNotHavePermissionException("update", "equipment");

            await baseWorkoutRepository.UpdateAsync(equipment);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("equipment", "update", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateUserEquipmentAsync(string userId, Equipment equipment)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipment is null)
                throw equipmentIsNullException;

            if (equipment.Id < 1)
                throw invalidEquipmentIDException;

            var _equipment = await baseWorkoutRepository.GetByIdAsync(equipment.Id) ?? throw equipmentNotFoundException;


            if (_equipment.OwnedByUserId != userId)
                throw UserNotHavePermissionException("update", "equipment");

            await baseWorkoutRepository.UpdateAsync(equipment);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("user equipment", "update", ex.Message));
        }
    }

    public async Task<bool> UserEquipmentExistsAsync(string userId, long equipmentId)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (equipmentId < 1)
            throw invalidEquipmentIDException;

        return await baseWorkoutRepository.ExistsAsync(equipmentId);
    }

    public async Task<bool> UserEquipmentExistsByNameAsync(string userId, string name)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (string.IsNullOrEmpty(name))
            throw equipmentNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }
}
