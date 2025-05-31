using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Validators.Services;
using WorkoutTracker.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.Common.Models;

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

    readonly string equipmentPhotosDirectory = Path.Combine("images", "equipments");
    const int maxEquipmentImageSizeInMB = 3;

    #region Internal Equipments

    const string internalEquipmentEntityName = "internal equipment";

    public async Task<Equipment> AddInternalEquipmentAsync(Equipment equipment, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateAddInternalAsync(equipment, cancellationToken);

        return await equipmentRepository.AddAsync(equipment)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "add"));
    }

    public async Task UpdateInternalEquipmentAsync(Equipment equipment, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateUpdateInternalAsync(equipment, cancellationToken);

        await equipmentRepository.UpdatePartialAsync(equipment.Id, EquipmentUpdateAction(equipment), cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "update"));
    }

    public async Task DeleteInternalEquipmentAsync(long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateDeleteInternalAsync(equipmentId, cancellationToken);

        var equipment = (await equipmentRepository.GetByIdAsync(equipmentId))!;
        string? equipmentImage = equipment.Image;

        await equipmentRepository.RemoveAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "delete"));

        if (!string.IsNullOrEmpty(equipmentImage))
            fileService.DeleteFile(equipmentImage);
    }

    public async Task<Equipment?> GetInternalEquipmentByIdAsync(long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetInternalByIdAsync(equipmentId, cancellationToken);

        return await equipmentRepository.GetByIdAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "get"));
    }

    public async Task<Equipment?> GetInternalEquipmentByNameAsync(string name, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetInternalByNameAsync(name, cancellationToken);

        return await equipmentRepository.GetByNameAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "get"));
    }

    public async Task<Equipment?> GetInternalEquipmentByIdWithDetailsAsync(long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetInternalByIdAsync(equipmentId, cancellationToken);

        return await equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "get"));
    }

    public async Task<Equipment?> GetInternalEquipmentByNameWithDetailsAsync(string name, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetInternalByNameAsync(name, cancellationToken);

        return await equipmentRepository.GetEquipmentByNameWithDetailsAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionStr(internalEquipmentEntityName, "get"));
    }

    public async Task<IEnumerable<Equipment>> GetInternalEquipmentsAsync(CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetAllInternalAsync(cancellationToken);

        var equipments = equipmentRepository.GetInternalEquipments();

        return await equipments.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("internal equipments", "get"));
    }

    public async Task UpdateInternalEquipmentPhotoAsync(long equipmentId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateUpdateInternalPhotoAsync(equipmentId, fileUpload, cancellationToken);

        string? image = await fileService.GetImageAsync(fileUpload, equipmentPhotosDirectory, maxEquipmentImageSizeInMB, false);
        var oldImage = await equipmentRepository.GetEquipmentPhotoAsync(equipmentId, cancellationToken);

        await (!string.IsNullOrEmpty(image) ?
            equipmentRepository.UpdateEquipmentPhotoAsync(equipmentId, image!, cancellationToken) :
            equipmentRepository.DeleteEquipmentPhotoAsync(equipmentId, cancellationToken)
        ).LogExceptionsAsync(_logger, FailedToActionStr($"{internalEquipmentEntityName} photo", "update"));

        if (!string.IsNullOrEmpty(oldImage))
            fileService.DeleteFile(oldImage);
    }

    public async Task DeleteInternalEquipmentPhotoAsync(long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateDeleteInternalPhotoAsync(equipmentId, cancellationToken);

        var oldImage = await equipmentRepository.GetEquipmentPhotoAsync(equipmentId, cancellationToken);

        if (!string.IsNullOrEmpty(oldImage))
        {
            await equipmentRepository.DeleteEquipmentPhotoAsync(equipmentId, cancellationToken)
                .LogExceptionsAsync(_logger, FailedToActionStr($"{internalEquipmentEntityName} photo", "delete"));

            fileService.DeleteFile(oldImage);
        }
    }

    #endregion

    #region User Equipments

    const string userEquipmentEntityName = "user equipment";

    public async Task<Equipment> AddUserEquipmentAsync(string userId, Equipment equipment, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateAddOwnedAsync(userId, equipment, cancellationToken);

        equipment.OwnedByUserId = userId;

        return await equipmentRepository.AddAsync(equipment)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "add", userId));
    }

    public async Task UpdateUserEquipmentAsync(string userId, Equipment equipment, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateUpdateOwnedAsync(userId, equipment, cancellationToken);

        await equipmentRepository.UpdatePartialAsync(equipment.Id, EquipmentUpdateAction(equipment), cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "update", userId));
    }

    public async Task DeleteEquipmentFromUserAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateDeleteOwnedAsync(userId, equipmentId, cancellationToken);

        var equipment = (await equipmentRepository.GetByIdAsync(equipmentId))!;
        string? equipmentImage = equipment.Image;

        await equipmentRepository.RemoveAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "delete", userId));

        if (!string.IsNullOrEmpty(equipmentImage))
            fileService.DeleteFile(equipmentImage);
    }

    public async Task<Equipment?> GetUserEquipmentByIdAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetOwnedByIdAsync(userId, equipmentId, cancellationToken);

        return await equipmentRepository.GetByIdAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "get", userId));
    }

    public async Task<Equipment?> GetUserEquipmentByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetOwnedByNameAsync(userId, name, cancellationToken);

        return await equipmentRepository.GetByNameAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "get", userId));
    }

    public async Task<Equipment?> GetUserEquipmentByIdWithDetailsAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetOwnedByIdAsync(userId, equipmentId, cancellationToken);

        return await equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "get", userId));
    }

    public async Task<Equipment?> GetUserEquipmentByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetOwnedByNameAsync(userId, name, cancellationToken);

        return await equipmentRepository.GetEquipmentByNameWithDetailsAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userEquipmentEntityName, "get", userId));
    }

    public async Task<IEnumerable<Equipment>> GetUserEquipmentsAsync(string userId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetAllOwnedAsync(userId, cancellationToken);

        var equipments = equipmentRepository.GetUserEquipments(userId);

        return await equipments.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("user equipments", "get", userId));
    }

    public async Task UpdateUserEquipmentPhotoAsync(string userId, long equipmentId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateUpdateOwnedPhotoAsync(userId, equipmentId, fileUpload, cancellationToken);

        string? image = await fileService.GetImageAsync(fileUpload, equipmentPhotosDirectory, maxEquipmentImageSizeInMB, true);
        var oldImage = await equipmentRepository.GetEquipmentPhotoAsync(equipmentId, cancellationToken);

        await (!string.IsNullOrEmpty(image) ?
            equipmentRepository.UpdateEquipmentPhotoAsync(equipmentId, image!, cancellationToken) :
            equipmentRepository.DeleteEquipmentPhotoAsync(equipmentId, cancellationToken)
        ).LogExceptionsAsync(_logger, FailedToActionStr($"{userEquipmentEntityName} photo", "update"));

        if (!string.IsNullOrEmpty(oldImage))
            fileService.DeleteFile(oldImage);
    }

    public async Task DeleteUserEquipmentPhotoAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateDeleteOwnedPhotoAsync(userId, equipmentId, cancellationToken);

        var oldImage = await equipmentRepository.GetEquipmentPhotoAsync(equipmentId, cancellationToken);

        if (!string.IsNullOrEmpty(oldImage))
        {
            await equipmentRepository.DeleteEquipmentPhotoAsync(equipmentId, cancellationToken)
                .LogExceptionsAsync(_logger, FailedToActionStr($"{userEquipmentEntityName} photo", "delete"));

            fileService.DeleteFile(oldImage);
        }
    }
    #endregion

    #region All Equipments

    const string equipmentEntityName = "equipment";

    public async Task<Equipment?> GetEquipmentByIdAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetByIdAsync(userId, equipmentId, cancellationToken);

        return await equipmentRepository.GetByIdAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(equipmentEntityName, "get", userId));
    }

    public async Task<Equipment?> GetEquipmentByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetByNameAsync(userId, name, cancellationToken);

        return await equipmentRepository.GetByNameAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(equipmentEntityName, "get", userId));
    }

    public async Task<Equipment?> GetEquipmentByIdWithDetailsAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetByIdAsync(userId, equipmentId, cancellationToken);

        return await equipmentRepository.GetEquipmentByIdWithDetailsAsync(equipmentId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(equipmentEntityName, "get", userId));
    }

    public async Task<Equipment?> GetEquipmentByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetByNameAsync(userId, name, cancellationToken);

        return await equipmentRepository.GetEquipmentByNameWithDetailsAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(equipmentEntityName, "get", userId));
    }

    public async Task<IEnumerable<Equipment>> GetAllEquipmentsAsync(string userId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetAllAsync(userId, cancellationToken);

        var equipments = equipmentRepository.GetAllEquipments(userId);

        return await equipments.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("equipments", "get"));
    }

    public async Task<IEnumerable<Equipment>> GetUsedEquipmentsAsync(string userId, CancellationToken cancellationToken)
    {
        await equipmentServiceValidator.ValidateGetUsedAsync(userId, cancellationToken);

        var usedEquipments = exerciseRecordRepository.GetUserExerciseRecords(userId)
            .SelectMany(er => er.Exercise!.Equipments)
            .Distinct();

        return await usedEquipments.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("used equipments", "get"));
    }

    #endregion


    public Action<Equipment> EquipmentUpdateAction(Equipment equipment)
    {
        var updateAction = new Action<Equipment>(e =>
        {
            e.Name = equipment.Name;
            //e.Image = equipment.Image;
        });

        return updateAction;
    }
}
