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

    readonly EntryNullException EquipmentIsNullException = new(nameof(Equipment));
    readonly InvalidIDException InvalidEquipmentIDException = new(nameof(Equipment));
    readonly NotFoundException EquipmentNotFoundException = new(nameof(Equipment));
    readonly ArgumentNullOrEmptyException EquipmentNameIsNullOrEmptyException = new("Equipment name");

    ArgumentException InvalidEquipmentIDWhileAddingException => InvalidEntryIDWhileAddingException(nameof(Equipment), "equipment");

    public async Task<ServiceResult<Equipment>> AddInternalEquipmentAsync(Equipment equipment)
    {
        try
        {
            if (equipment is null)
                throw EquipmentIsNullException;

            if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
                throw new UnauthorizedAccessException("Equipment entry cannot be created by user.");

            if (equipment.Id != 0)
                throw InvalidEquipmentIDWhileAddingException;

            await baseWorkoutRepository.AddAsync(equipment);
            return ServiceResult<Equipment>.Ok(equipment);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult<Equipment>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("internal equipment", "add", ex));
        }
    }

    public async Task<ServiceResult<Equipment>> AddUserEquipmentAsync(string userId, Equipment equipment)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipment is null)
                throw EquipmentIsNullException;

            if (equipment.Id != 0)
                throw InvalidEquipmentIDWhileAddingException;

            equipment.OwnedByUserId = userId;
            await baseWorkoutRepository.AddAsync(equipment);

            return ServiceResult<Equipment>.Ok(equipment);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("user equipment", "add", ex));
        }
    }

    public async Task<ServiceResult> DeleteInternalEquipmentAsync(long equipmentId)
    {
        try
        {
            if (equipmentId < 1)
                throw InvalidEquipmentIDException;

            var equipment = await baseWorkoutRepository.GetByIdAsync(equipmentId) ?? throw EquipmentNotFoundException;

            if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
                throw UserNotHavePermissionException("delete", "internal equipment");

            await baseWorkoutRepository.RemoveAsync(equipmentId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("internal equipment", "delete"));
        }
    }

    public async Task<ServiceResult> DeleteEquipmentFromUserAsync(string userId, long equipmentId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipmentId < 1)
                throw InvalidEquipmentIDException;

            var equipment = await baseWorkoutRepository.GetByIdAsync(equipmentId) ?? throw EquipmentNotFoundException;

            if (equipment.OwnedByUserId != userId)
                throw UserNotHavePermissionException("delete", "equipment");

            await baseWorkoutRepository.RemoveAsync(equipmentId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("user equipment", "delete"));
        }
    }

    public async Task<bool> InternalEquipmentExistsAsync(long equipmentId)
    {
        if (equipmentId < 1)
            throw InvalidEquipmentIDException;

        var equipment = await baseWorkoutRepository.GetByIdAsync(equipmentId);

        if (equipment is null)
            return false;

        if (equipment.OwnedByUserId != null)
            throw UserNotHavePermissionException("get", "internal equipment");

        return true;
    }

    public async Task<bool> InternalEquipmentExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw EquipmentNameIsNullOrEmptyException;

        var equipment = await baseWorkoutRepository.GetByNameAsync(name);

        if (equipment is null)
            return false;

        if (equipment.OwnedByUserId != null)
            throw UserNotHavePermissionException("get", "internal equipment  by name");

        return true;
    }

    public async Task<ServiceResult<Equipment>> GetInternalEquipmentByIdAsync(long equipmentId)
    {
        try
        {
            if (equipmentId < 1)
                throw InvalidEquipmentIDException;

            var equipmentById = await baseWorkoutRepository.GetByIdAsync(equipmentId);

            if (equipmentById != null && equipmentById.OwnedByUserId != null)
                throw UserNotHavePermissionException("get", "internal equipment");

            return ServiceResult<Equipment>.Ok(equipmentById);
        }
        catch (ArgumentException argEx)
        {
            return ServiceResult<Equipment>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("internal equipment", "get", ex));
        }
    }

    public async Task<ServiceResult<Equipment>> GetInternalEquipmentByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                throw EquipmentNameIsNullOrEmptyException;

            var equipmentByName = await baseWorkoutRepository.GetByNameAsync(name);

            if (equipmentByName != null && equipmentByName.OwnedByUserId != null)
                throw UserNotHavePermissionException("get", "internal equipment by name");

            return ServiceResult<Equipment>.Ok(equipmentByName);
        }
        catch (ArgumentException argEx)
        {
            return ServiceResult<Equipment>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("internal equipment by name", "get", ex));
        }
    }

    public async Task<ServiceResult<IQueryable<Equipment>>> GetInternalEquipmentsAsync()
    {
        try
        {
            var equipments = await baseWorkoutRepository.FindAsync(e => e.OwnedByUserId == null);
            return ServiceResult<IQueryable<Equipment>>.Ok(equipments);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(FailedToActionStr("internal equipments", "get", ex));
        }
    }

    public async Task<ServiceResult<Equipment>> GetUserEquipmentByIdAsync(string userId, long equipmentId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipmentId < 1)
                throw InvalidEquipmentIDException;

            var userEquipmentById = await baseWorkoutRepository.GetByIdAsync(equipmentId);
            return ServiceResult<Equipment>.Ok(userEquipmentById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("user equipment", "get", ex));
        }
    }

    public async Task<ServiceResult<Equipment>> GetUserEquipmentByNameAsync(string userId, string name)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw EquipmentNameIsNullOrEmptyException;

            var userEquipmentByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Equipment>.Ok(userEquipmentByName);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("user equipment by name", "get", ex));
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
            return ServiceResult<IQueryable<Equipment>>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(FailedToActionStr("user equipments", "get", ex));
        }
    }

    public async Task<ServiceResult<IQueryable<Equipment>>> GetAllEquipmentsAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var equipments = await baseWorkoutRepository.FindAsync(e => e.OwnedByUserId == userId || e.OwnedByUserId == null);
            return ServiceResult<IQueryable<Equipment>>.Ok(equipments);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(FailedToActionStr("equipments", "get", ex));
        }
    }

    public async Task<ServiceResult> UpdateInternalEquipmentAsync(Equipment equipment)
    {
        try
        {
            if (equipment is null)
                throw EquipmentIsNullException;

            if (equipment.Id < 1)
                throw InvalidEquipmentIDException;

            var _equipment = await baseWorkoutRepository.GetByIdAsync(equipment.Id) ?? throw EquipmentNotFoundException;

            if (!string.IsNullOrEmpty(_equipment.OwnedByUserId))
                throw UserNotHavePermissionException("update", "internal equipment");

            _equipment.Name = equipment.Name;
            _equipment.Image = equipment.Image;

            await baseWorkoutRepository.UpdateAsync(_equipment);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("equipment", "update", ex));
        }
    }

    public async Task<ServiceResult> UpdateUserEquipmentAsync(string userId, Equipment equipment)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipment is null)
                throw EquipmentIsNullException;

            if (equipment.Id < 1)
                throw InvalidEquipmentIDException;

            var _equipment = await baseWorkoutRepository.GetByIdAsync(equipment.Id) ?? throw EquipmentNotFoundException;

            if (_equipment.OwnedByUserId != userId)
                throw UserNotHavePermissionException("update", "equipment");

            _equipment.Name = equipment.Name;
            _equipment.Image = equipment.Image;

            await baseWorkoutRepository.UpdateAsync(_equipment);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("user equipment", "update", ex));
        }
    }

    public async Task<bool> UserEquipmentExistsAsync(string userId, long equipmentId)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (equipmentId < 1)
            throw InvalidEquipmentIDException;

        return await baseWorkoutRepository.ExistsAsync(equipmentId);
    }

    public async Task<bool> UserEquipmentExistsByNameAsync(string userId, string name)
    {
        await CheckUserIdAsync(userRepository, userId);

        if (string.IsNullOrEmpty(name))
            throw EquipmentNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }
}
