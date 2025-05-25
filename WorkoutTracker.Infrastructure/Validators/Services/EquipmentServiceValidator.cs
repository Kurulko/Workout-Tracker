using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Validators.Models;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services;

public class EquipmentServiceValidator
{
    readonly UserValidator userValidator;
    readonly IEquipmentRepository equipmentRepository;
    readonly EquipmentValidator equipmentValidator;

    public EquipmentServiceValidator(
        UserValidator userValidator,
        IEquipmentRepository equipmentRepository,
        EquipmentValidator equipmentValidator
    )
    {
        this.userValidator = userValidator;
        this.equipmentRepository = equipmentRepository;
        this.equipmentValidator = equipmentValidator;
    }

    #region Internal Equipment

    public async Task ValidateAddInternalAsync(Equipment equipment)
    {
        await equipmentValidator.ValidateForAddAsync(equipment);

        if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("add", "internal equipment");
    }

    public async Task ValidateUpdateInternalAsync(Equipment equipment)
    {
        var _equipment = await equipmentValidator.ValidateForEditAsync(equipment);

        if (!string.IsNullOrEmpty(_equipment.OwnedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("update", "internal equipment");
    }

    public async Task ValidateDeleteInternalAsync(long equipmentId)
    {
        var equipment = await equipmentValidator.EnsureExistsAsync(equipmentId);

        if (!string.IsNullOrEmpty(equipment.OwnedByUserId))
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "internal equipment");
    }

    public async Task ValidateGetInternalByIdAsync(long equipmentId)
    {
        ArgumentValidator.ThrowIfIdNonPositive(equipmentId, "Equipment");

        var equipment = await equipmentRepository.GetByIdAsync(equipmentId);

        if (equipment != null && equipment.OwnedByUserId != null)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "internal equipment");
    }

    public async Task ValidateGetInternalByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Equipment");

        var equipment = await equipmentRepository.GetByNameAsync(name);

        if (equipment != null && equipment.OwnedByUserId != null)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "internal equipment");
    }

    public Task ValidateGetAllInternalAsync()
    {
        return Task.CompletedTask;
    }

    #endregion

    #region User Equipment

    public async Task ValidateAddOwnedAsync(string userId, Equipment equipment)
    {
        await userValidator.EnsureExistsAsync(userId);
        await equipmentValidator.ValidateForAddAsync(equipment);
    }

    public async Task ValidateUpdateOwnedAsync(string userId, Equipment equipment)
    {
        await userValidator.EnsureExistsAsync(userId);
        await equipmentValidator.ValidateForEditAsync(equipment);

        var _equipment = (await equipmentRepository.GetByIdAsync(equipment.Id))!;

        if (_equipment.OwnedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user equipment");
    }

    public async Task ValidateDeleteOwnedAsync(string userId, long equipmentId)
    {
        await userValidator.EnsureExistsAsync(userId);

        var equipment = await equipmentValidator.EnsureExistsAsync(equipmentId);

        if (equipment.OwnedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "user equipment");
    }

    public async Task ValidateGetOwnedByIdAsync(string userId, long equipmentId)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(equipmentId, "Equipment");

        var equipment = await equipmentRepository.GetByIdAsync(equipmentId);

        if (equipment != null && equipment.OwnedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "user equipment");
    }

    public async Task ValidateGetOwnedByNameAsync(string userId, string name)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Equipment");

        var equipment = await equipmentRepository.GetByNameAsync(name);

        if (equipment != null && equipment.OwnedByUserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "user equipment");
    }

    public async Task ValidateGetAllOwnedAsync(string userId)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    #endregion

    #region All Equipment

    public async Task ValidateGetByIdAsync(string userId, long equipmentId)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(equipmentId, "Equipment");

        var equipment = await equipmentRepository.GetByIdAsync(equipmentId);

        if (equipment != null && (equipment.OwnedByUserId != userId && equipment.OwnedByUserId != null))
            throw UnauthorizedException.HaveNoPermissionToAction("get", "equipment");
    }

    public async Task ValidateGetByNameAsync(string userId, string name)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNullOrEmpty(name, "Equipment");

        var equipment = await equipmentRepository.GetByNameAsync(name);

        if (equipment != null && (equipment.OwnedByUserId != userId && equipment.OwnedByUserId != null))
            throw UnauthorizedException.HaveNoPermissionToAction("get", "equipment");
    }

    public async Task ValidateGetAllAsync(string userId)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUsedAsync(string userId)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    #endregion
}
