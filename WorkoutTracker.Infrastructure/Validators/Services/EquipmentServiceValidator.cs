using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Validators.Models;
using WorkoutTracker.Infrastructure.Validators.Models.Muscles;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services;

public class EquipmentServiceValidator
{
    readonly UserValidator userValidator;
    readonly IEquipmentRepository equipmentRepository;
    readonly EquipmentValidator equipmentValidator;
    readonly FileUploadModelValidator fileUploadModelValidator;
    public EquipmentServiceValidator(
        UserValidator userValidator,
        IEquipmentRepository equipmentRepository,
        EquipmentValidator equipmentValidator,
        FileUploadModelValidator fileUploadModelValidator
    )
    {
        this.userValidator = userValidator;
        this.equipmentRepository = equipmentRepository;
        this.equipmentValidator = equipmentValidator;
        this.fileUploadModelValidator = fileUploadModelValidator;
    }

    #region Internal Equipment

    public async Task ValidateAddInternalAsync(Equipment equipment, CancellationToken cancellationToken)
    {
        await equipmentValidator.ValidateForAddAsync(equipment, cancellationToken);

        if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("add", "internal equipment");
    }

    public async Task ValidateUpdateInternalAsync(Equipment equipment, CancellationToken cancellationToken)
    {
        var _equipment = await equipmentValidator.ValidateForEditAsync(equipment, cancellationToken);

        if (!string.IsNullOrEmpty(_equipment.OwnedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal equipment");
    }

    public async Task ValidateDeleteInternalAsync(long equipmentId, CancellationToken cancellationToken)
    {
        var equipment = await equipmentValidator.EnsureExistsAsync(equipmentId, cancellationToken);

        if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "internal equipment");
    }

    public async Task ValidateGetInternalByIdAsync(long equipmentId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(equipmentId, "Equipment");

        var equipment = await equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);

        if (equipment != null && equipment.OwnedByUserId != null)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "internal equipment");
    }

    public async Task ValidateGetInternalByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Equipment");

        var equipment = await equipmentRepository.GetByNameAsync(name, cancellationToken);

        if (equipment != null && equipment.OwnedByUserId != null)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "internal equipment");
    }

    public Task ValidateGetAllInternalAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task ValidateUpdateInternalPhotoAsync(long equipmentId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await equipmentValidator.EnsureExistsAsync(equipmentId, cancellationToken);

        if (fileUpload != null)
            fileUploadModelValidator.Validate(fileUpload);
    }

    public async Task ValidateDeleteInternalPhotoAsync(long equipmentId, CancellationToken cancellationToken)
    {
        await equipmentValidator.EnsureExistsAsync(equipmentId, cancellationToken);
    }

    #endregion

    #region User Equipment

    public async Task ValidateAddOwnedAsync(string userId, Equipment equipment, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await equipmentValidator.ValidateForAddAsync(equipment, cancellationToken);
    }

    public async Task ValidateUpdateOwnedAsync(string userId, Equipment equipment, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await equipmentValidator.ValidateForEditAsync(equipment, cancellationToken);

        var _equipment = (await equipmentRepository.GetByIdAsync(equipment.Id, cancellationToken))!;

        if (_equipment.OwnedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user equipment");
    }

    public async Task ValidateDeleteOwnedAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        var equipment = await equipmentValidator.EnsureExistsAsync(equipmentId, cancellationToken);

        if (equipment.OwnedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "user equipment");
    }

    public async Task ValidateGetOwnedByIdAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(equipmentId, "Equipment");

        var equipment = await equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);

        if (equipment != null && equipment.OwnedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "user equipment");
    }

    public async Task ValidateGetOwnedByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Equipment");

        var equipment = await equipmentRepository.GetByNameAsync(name, cancellationToken);

        if (equipment != null && equipment.OwnedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "user equipment");
    }

    public async Task ValidateGetAllOwnedAsync(string userId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateUpdateOwnedPhotoAsync(string userId, long equipmentId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await equipmentValidator.EnsureExistsAsync(equipmentId, cancellationToken);

        if (fileUpload != null)
            fileUploadModelValidator.Validate(fileUpload);
    }

    public async Task ValidateDeleteOwnedPhotoAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        await equipmentValidator.EnsureExistsAsync(equipmentId, cancellationToken);
    }

    #endregion

    #region All Equipment

    public async Task ValidateGetByIdAsync(string userId, long equipmentId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(equipmentId, "Equipment");

        var equipment = await equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);

        if (equipment != null && (equipment.OwnedByUserId != userId && equipment.OwnedByUserId != null))
            throw UnauthorizedException.HaveNoPermissionToAction("get", "equipment");
    }

    public async Task ValidateGetByNameAsync(string userId, string name, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Equipment");

        var equipment = await equipmentRepository.GetByNameAsync(name, cancellationToken);

        if (equipment != null && (equipment.OwnedByUserId != userId && equipment.OwnedByUserId != null))
            throw UnauthorizedException.HaveNoPermissionToAction("get", "equipment");
    }

    public async Task ValidateGetAllAsync(string userId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUsedAsync(string userId, CancellationToken cancellationToken)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    #endregion
}
