using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.EquipmentServices;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;
using WorkoutTrackerAPI.Services.FileServices;

namespace WorkoutTrackerAPI.Services.EquipmentServices;

public class EquipmentService : BaseWorkoutService<Equipment>, IEquipmentService
{
    readonly UserRepository userRepository;
    readonly ExerciseRecordRepository exerciseRecordRepository;
    readonly EquipmentRepository equipmentRepository;
    readonly IFileService fileService;
    public EquipmentService(EquipmentRepository equipmentRepository, UserRepository userRepository, ExerciseRecordRepository exerciseRecordRepository, IFileService fileService) : base(equipmentRepository)
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

    ArgumentException InvalidEquipmentIDWhileAddingException => InvalidEntryIDWhileAddingException(nameof(Equipment), "equipment");

    ArgumentException EquipmentNameMustBeUnique()
        => EntryNameMustBeUnique(nameof(Equipment));

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
        catch (ArgumentException argEx)
        {
            return ServiceResult<Equipment>.Fail(argEx.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("internal equipment", "get", ex));
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
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("user equipment", "get", ex));
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
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("user equipment by name", "get", ex));
        }
    }


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
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("equipment", "get", ex));
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
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<Equipment>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<Equipment>.Fail(FailedToActionStr("equipment by name", "get", ex));
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

    
    async Task<bool> IsUniqueEquipmentNameForUserAsync(string name, string userId)
    {
        var isAnyEquipmentNames = await baseWorkoutRepository.AnyAsync(w => w.Name == name && (w.OwnedByUserId == userId || w.OwnedByUserId == null));
        return !isAnyEquipmentNames;
    }
}
