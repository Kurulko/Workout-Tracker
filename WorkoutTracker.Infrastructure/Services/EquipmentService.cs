using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Application.Common.Results;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Infrastructure.Services;

internal class EquipmentService : BaseWorkoutService<EquipmentService, Equipment>, IEquipmentService
{
    readonly IUserRepository userRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly IEquipmentRepository equipmentRepository;
    readonly IFileService fileService;
    public EquipmentService(
        IEquipmentRepository equipmentRepository, 
        IUserRepository userRepository, 
        IExerciseRecordRepository exerciseRecordRepository, 
        IFileService fileService,
        ILogger<EquipmentService> logger
    ) : base(equipmentRepository, logger)
    {
        this.equipmentRepository = equipmentRepository;
        this.userRepository = userRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.fileService = fileService;
    }

    readonly EntryNullException equipmentIsNullException = new(nameof(Equipment));
    readonly InvalidIDException invalidEquipmentIDException = new(nameof(Equipment));
    readonly ArgumentNullOrEmptyException equipmentNameIsNullOrEmptyException = new("Equipment name");

    NotFoundException EquipmentNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID(nameof(Equipment), id);

    ValidationException InvalidEquipmentIDWhileAddingException => InvalidEntryIDWhileAddingException(nameof(Equipment), "equipment");

    ValidationException EquipmentNameMustBeUnique()
        => EntryNameMustBeUnique(nameof(Equipment));


    #region Internal Equipments

    public async Task<ServiceResult<Equipment>> AddInternalEquipmentAsync(Equipment equipment)
    {
        try
        {
            if (equipment is null)
                throw equipmentIsNullException;

            if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
                throw EntryNameMustBeUnique(nameof(Equipment));

            if (equipment.Id != 0)
                throw InvalidEquipmentIDWhileAddingException;

            if (await baseWorkoutRepository.ExistsByNameAsync(equipment.Name))
                throw EquipmentNameMustBeUnique();

            await baseWorkoutRepository.AddAsync(equipment);
            return ServiceResult<Equipment>.Ok(equipment);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal equipment", "add"));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateInternalEquipmentAsync(Equipment equipment)
    {
        try
        {
            if (equipment is null)
                throw equipmentIsNullException;

            if (equipment.Id < 1)
                throw invalidEquipmentIDException;

            var _equipment = await baseWorkoutRepository.GetByIdAsync(equipment.Id) ?? throw EquipmentNotFoundByIDException(equipment.Id);

            if (!string.IsNullOrEmpty(_equipment.OwnedByUserId))
                throw UserNotHavePermissionException("update", "internal equipment");

            var isSameName = _equipment.Name != equipment.Name;
            var isUniqueEquipmentName = isSameName || await baseWorkoutRepository.ExistsByNameAsync(equipment.Name);
            if (!isUniqueEquipmentName)
                throw EquipmentNameMustBeUnique();

            _equipment.Name = equipment.Name;
            _equipment.Image = equipment.Image;

            await baseWorkoutRepository.UpdateAsync(_equipment);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal equipment", "update"));
            throw;
        }
    }


    public async Task<ServiceResult> DeleteInternalEquipmentAsync(long equipmentId)
    {
        try
        {
            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var equipment = await baseWorkoutRepository.GetByIdAsync(equipmentId) ?? throw EquipmentNotFoundByIDException(equipmentId);

            if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
                throw UserNotHavePermissionException("delete", "internal equipment");

            string? equipmentImage = equipment.Image;
            await baseWorkoutRepository.RemoveAsync(equipmentId);

            if (!string.IsNullOrEmpty(equipmentImage))
            {
                fileService.DeleteFile(equipmentImage);
            }

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal equipment", "delete"));
            throw;
        }
    }

    
    public async Task<ServiceResult<Equipment>> GetInternalEquipmentByIdAsync(long equipmentId, bool withDetails = false)
    {
        try
        {
            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var equipmentById = withDetails ? await equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId) : await baseWorkoutRepository.GetByIdAsync(equipmentId);

            if (equipmentById != null && equipmentById.OwnedByUserId != null)
                throw UserNotHavePermissionException("get", "internal equipment");

            return ServiceResult<Equipment>.Ok(equipmentById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal equipment", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<Equipment>> GetInternalEquipmentByNameAsync(string name, bool withDetails = false)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                throw equipmentNameIsNullOrEmptyException;

            var equipmentByName = withDetails ? await equipmentRepository.GetEquipmentByNameWithDetailsAsync(name) : await baseWorkoutRepository.GetByNameAsync(name);

            if (equipmentByName != null && equipmentByName.OwnedByUserId != null)
                throw UserNotHavePermissionException("get", "internal equipment by name");

            return ServiceResult<Equipment>.Ok(equipmentByName);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("internal equipment by name", "get"));
            throw;
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
            _logger.LogError(ex, FailedToActionStr("internal equipments", "get"));
            throw;
        }
    }


    public async Task<bool> InternalEquipmentExistsAsync(long equipmentId)
    {
        if (equipmentId < 1)
            throw invalidEquipmentIDException;

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
            throw equipmentNameIsNullOrEmptyException;

        var equipment = await baseWorkoutRepository.GetByNameAsync(name);

        if (equipment is null)
            return false;

        if (equipment.OwnedByUserId != null)
            throw UserNotHavePermissionException("get", "internal equipment  by name");

        return true;
    }

    #endregion

    #region User Equipments

    public async Task<ServiceResult<Equipment>> AddUserEquipmentAsync(string userId, Equipment equipment)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipment is null)
                throw equipmentIsNullException;

            if (equipment.Id != 0)
                throw InvalidEquipmentIDWhileAddingException;

            var isUniqueEquipmentName = await IsUniqueEquipmentNameForUserAsync(equipment.Name, userId);
            if (!isUniqueEquipmentName)
                throw EquipmentNameMustBeUnique();

            equipment.OwnedByUserId = userId;
            await baseWorkoutRepository.AddAsync(equipment);

            return ServiceResult<Equipment>.Ok(equipment);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user equipment", "add", userId));
            throw;
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

            var _equipment = await baseWorkoutRepository.GetByIdAsync(equipment.Id) ?? throw EquipmentNotFoundByIDException(equipment.Id);

            if (_equipment.OwnedByUserId != userId)
                throw UserNotHavePermissionException("update", "equipment");

            var isSameName = _equipment.Name != equipment.Name;
            var isUniqueEquipmentName = isSameName || await IsUniqueEquipmentNameForUserAsync(equipment.Name, userId);
            if (!isUniqueEquipmentName)
                throw EquipmentNameMustBeUnique();

            _equipment.Name = equipment.Name;
            _equipment.Image = equipment.Image;

            await baseWorkoutRepository.UpdateAsync(_equipment);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user equipment", "update", userId));
            throw;
        }
    }


    public async Task<ServiceResult> DeleteEquipmentFromUserAsync(string userId, long equipmentId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var equipment = await baseWorkoutRepository.GetByIdAsync(equipmentId) ?? throw EquipmentNotFoundByIDException(equipmentId);

            if (equipment.OwnedByUserId != userId)
                throw UserNotHavePermissionException("delete", "equipment");

            string? equipmentImage = equipment.Image;
            await baseWorkoutRepository.RemoveAsync(equipmentId);

            if (!string.IsNullOrEmpty(equipmentImage))
            {
                fileService.DeleteFile(equipmentImage);
            }

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user equipment", "delete", userId));
            throw;
        }
    }


    public async Task<ServiceResult<Equipment>> GetUserEquipmentByIdAsync(string userId, long equipmentId, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var userEquipmentById = withDetails ? await equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId) : await baseWorkoutRepository.GetByIdAsync(equipmentId);

            if (userEquipmentById != null && userEquipmentById.OwnedByUserId != userId)
                throw UserNotHavePermissionException("get", "user equipment");

            return ServiceResult<Equipment>.Ok(userEquipmentById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user equipment", "get", userId));
            throw;
        }
    }

    public async Task<ServiceResult<Equipment>> GetUserEquipmentByNameAsync(string userId, string name, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw equipmentNameIsNullOrEmptyException;

            var userEquipmentByName = withDetails ? await equipmentRepository.GetEquipmentByNameWithDetailsAsync(name) : await baseWorkoutRepository.GetByNameAsync(name);

            if (userEquipmentByName != null && userEquipmentByName.OwnedByUserId != userId)
                throw UserNotHavePermissionException("get", "user equipment by name");

            return ServiceResult<Equipment>.Ok(userEquipmentByName);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user equipment by name", "get", userId));
            throw;
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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionForUserStr("user equipments", "get", userId));
            throw;
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

    #endregion

    #region All Equipments

    public async Task<ServiceResult<Equipment>> GetEquipmentByIdAsync(string userId, long equipmentId, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (equipmentId < 1)
                throw invalidEquipmentIDException;

            var equipmentById = withDetails ? await equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId) : await baseWorkoutRepository.GetByIdAsync(equipmentId);

            if (equipmentById != null && (equipmentById.OwnedByUserId != userId && equipmentById.OwnedByUserId != null))
                throw UserNotHavePermissionException("get", "equipment");

            return ServiceResult<Equipment>.Ok(equipmentById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("equipment", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<Equipment>> GetEquipmentByNameAsync(string userId, string name, bool withDetails = false)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (string.IsNullOrEmpty(name))
                throw equipmentNameIsNullOrEmptyException;

            var equipmentByName = withDetails ? await equipmentRepository.GetEquipmentByNameWithDetailsAsync(name) : await baseWorkoutRepository.GetByNameAsync(name);

            if (equipmentByName != null && (equipmentByName.OwnedByUserId != userId && equipmentByName.OwnedByUserId != null))
                throw UserNotHavePermissionException("get", "equipment by name");

            return ServiceResult<Equipment>.Ok(equipmentByName);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Equipment>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("equipment by name", "get"));
            throw;
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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("equipments", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<Equipment>>> GetUsedEquipmentsAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var usedEquipments = (await exerciseRecordRepository.FindAsync(er => er.UserId == userId))
                .SelectMany(er => er.Exercise!.Equipments)
                .Distinct();

            return ServiceResult<IQueryable<Equipment>>.Ok(usedEquipments);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<Equipment>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("used equipments", "get"));
            throw;
        }
    }

    #endregion

    async Task<bool> IsUniqueEquipmentNameForUserAsync(string name, string userId)
    {
        var isAnyEquipmentNames = await baseWorkoutRepository.AnyAsync(w => w.Name == name && (w.OwnedByUserId == userId || w.OwnedByUserId == null));
        return !isAnyEquipmentNames;
    }
}
