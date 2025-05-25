using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Validators.Services;
using WorkoutTracker.Infrastructure.Extensions;

namespace WorkoutTracker.Infrastructure.Services;

internal class EquipmentService : BaseWorkoutService<EquipmentService, Equipment>, IEquipmentService
{
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly IEquipmentRepository equipmentRepository;
    readonly IFileService fileService;
    readonly EquipmentServiceValidator equipmentServiceValidator;

    public EquipmentService(
        IEquipmentRepository equipmentRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IFileService fileService,
        ILogger<EquipmentService> logger,
        EquipmentServiceValidator equipmentServiceValidator
    ) : base(equipmentRepository, logger)
    {
        this.equipmentRepository = equipmentRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.fileService = fileService;
        this.equipmentServiceValidator = equipmentServiceValidator;
    }

    #region Internal Equipments

    const string internalEquipmentEntityName = "internal equipment";

    public async Task<Equipment> AddInternalEquipmentAsync(Equipment equipment)
    {
        await equipmentServiceValidator.ValidateAddInternalAsync(equipment);

        return await baseWorkoutRepository.AddAsync(equipment)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "add"));
    }

    public async Task UpdateInternalEquipmentAsync(Equipment equipment)
    {
        await equipmentServiceValidator.ValidateUpdateInternalAsync(equipment);

        var _equipment = (await baseWorkoutRepository.GetByIdAsync(equipment.Id))!;

        _equipment.Name = equipment.Name;
        _equipment.Image = equipment.Image;

        await baseWorkoutRepository.UpdateAsync(_equipment)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "update"));
    }

    public async Task DeleteInternalEquipmentAsync(long equipmentId)
    {
        await equipmentServiceValidator.ValidateDeleteInternalAsync(equipmentId);

        var equipment = (await baseWorkoutRepository.GetByIdAsync(equipmentId))!;
        string? equipmentImage = equipment.Image;

        await baseWorkoutRepository.RemoveAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "delete"));

        if (!string.IsNullOrEmpty(equipmentImage))
            fileService.DeleteFile(equipmentImage);
    }

    public async Task<Equipment?> GetInternalEquipmentByIdAsync(long equipmentId, bool withDetails = false)
    {
        await equipmentServiceValidator.ValidateGetInternalByIdAsync(equipmentId);

        return await (withDetails
            ? equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId)
            : baseWorkoutRepository.GetByIdAsync(equipmentId)
        ).LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "get"));
    }

    public async Task<Equipment?> GetInternalEquipmentByNameAsync(string name, bool withDetails = false)
    {
        await equipmentServiceValidator.ValidateGetInternalByNameAsync(name);

        return await (withDetails
            ? equipmentRepository.GetEquipmentByNameWithDetailsAsync(name)
            : baseWorkoutRepository.GetByNameAsync(name)
        ).LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "get"));
    }

    public async Task<IQueryable<Equipment>> GetInternalEquipmentsAsync()
    {
        await equipmentServiceValidator.ValidateGetAllInternalAsync();

        return await baseWorkoutRepository.FindAsync(e => e.OwnedByUserId == null)
            .LogExceptionsAsync(_logger, FailedToActionStr("internal equipments", "get"));
    }

    #endregion

    #region User Equipments

    const string userEquipmentEntityName = "user equipment";

    public async Task<Equipment> AddUserEquipmentAsync(string userId, Equipment equipment)
    {
        await equipmentServiceValidator.ValidateAddOwnedAsync(userId, equipment);

        equipment.OwnedByUserId = userId;

        return await baseWorkoutRepository.AddAsync(equipment)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "add", userId));
    }

    public async Task UpdateUserEquipmentAsync(string userId, Equipment equipment)
    {
        await equipmentServiceValidator.ValidateUpdateOwnedAsync(userId, equipment);

        var _equipment = (await baseWorkoutRepository.GetByIdAsync(equipment.Id))!;

        _equipment.Name = equipment.Name;
        _equipment.Image = equipment.Image;

        await baseWorkoutRepository.UpdateAsync(_equipment)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "update", userId));
    }

    public async Task DeleteEquipmentFromUserAsync(string userId, long equipmentId)
    {
        await equipmentServiceValidator.ValidateDeleteOwnedAsync(userId, equipmentId);

        var equipment = (await baseWorkoutRepository.GetByIdAsync(equipmentId))!;
        string? equipmentImage = equipment.Image;

        await baseWorkoutRepository.RemoveAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "delete", userId));

        if (!string.IsNullOrEmpty(equipmentImage))
            fileService.DeleteFile(equipmentImage);
    }

    public async Task<Equipment?> GetUserEquipmentByIdAsync(string userId, long equipmentId, bool withDetails = false)
    {
        await equipmentServiceValidator.ValidateGetOwnedByIdAsync(userId, equipmentId);

        return await (withDetails
            ? equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId)
            : baseWorkoutRepository.GetByIdAsync(equipmentId)
        ).LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "get", userId));
    }

    public async Task<Equipment?> GetUserEquipmentByNameAsync(string userId, string name, bool withDetails = false)
    {
        await equipmentServiceValidator.ValidateGetOwnedByNameAsync(userId, name);

        return await (withDetails
            ? equipmentRepository.GetEquipmentByNameWithDetailsAsync(name)
            : baseWorkoutRepository.GetByNameAsync(name)
        ).LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "get", userId));
    }

    public async Task<IQueryable<Equipment>> GetUserEquipmentsAsync(string userId)
    {
        await equipmentServiceValidator.ValidateGetAllOwnedAsync(userId);

        return await baseWorkoutRepository.FindAsync(e => e.OwnedByUserId == userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("user equipments", "get", userId));
    }

    #endregion

    #region All Equipments

    const string equipmentEntityName = "equipment";

    public async Task<Equipment?> GetEquipmentByIdAsync(string userId, long equipmentId, bool withDetails = false)
    {
        await equipmentServiceValidator.ValidateGetByIdAsync(userId, equipmentId);

        return await (withDetails
            ? equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId)
            : baseWorkoutRepository.GetByIdAsync(equipmentId)
        ).LogExceptionsAsync(_logger, FailedToActionForUserStr(equipmentEntityName, "get", userId));
    }

    public async Task<Equipment?> GetEquipmentByNameAsync(string userId, string name, bool withDetails = false)
    {
        await equipmentServiceValidator.ValidateGetByNameAsync(userId, name);

        return await (withDetails
            ? equipmentRepository.GetEquipmentByNameWithDetailsAsync(name)
            : baseWorkoutRepository.GetByNameAsync(name)
        ).LogExceptionsAsync(_logger, FailedToActionForUserStr(equipmentEntityName, "get", userId));
    }

    public async Task<IQueryable<Equipment>> GetAllEquipmentsAsync(string userId)
    {
        await equipmentServiceValidator.ValidateGetAllAsync(userId);

        return await baseWorkoutRepository.FindAsync(e => e.OwnedByUserId == userId || e.OwnedByUserId == null)
            .LogExceptionsAsync(_logger, FailedToActionStr("equipments", "get"));
    }

    public async Task<IQueryable<Equipment>> GetUsedEquipmentsAsync(string userId)
    {
        await equipmentServiceValidator.ValidateGetUsedAsync(userId);

        var usedEquipments = (await exerciseRecordRepository.GetExerciseRecordsByUserIdAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionStr("used equipments", "get")))
            .SelectMany(er => er.Exercise!.Equipments)
            .Distinct();

        return usedEquipments.AsQueryable();
    }

    #endregion
}
