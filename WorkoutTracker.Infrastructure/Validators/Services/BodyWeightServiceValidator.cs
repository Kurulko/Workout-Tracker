using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Validators.Models;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services;

public class BodyWeightServiceValidator
{
    readonly UserValidator userValidator;
    readonly BodyWeightValidator bodyWeightValidator;
    readonly IBodyWeightRepository bodyWeightRepository;

    public BodyWeightServiceValidator(
        UserValidator userValidator,
        BodyWeightValidator bodyWeightValidator,
        IBodyWeightRepository bodyWeightRepository
    )
    {
        this.userValidator = userValidator;
        this.bodyWeightValidator = bodyWeightValidator;
        this.bodyWeightRepository = bodyWeightRepository;
    }

    public async Task ValidateAddAsync(string userId, BodyWeight bodyWeight)
    {
        await userValidator.EnsureExistsAsync(userId);
        await bodyWeightValidator.ValidateForAddAsync(bodyWeight);
    }

    public async Task ValidateUpdateAsync(string userId, BodyWeight bodyWeight)
    {
        await userValidator.EnsureExistsAsync(userId);
        var _bodyWeight = await bodyWeightValidator.ValidateForEditAsync(bodyWeight);

        if (_bodyWeight.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "body weight");
    }

    public async Task ValidateDeleteAsync(string userId, long bodyWeightId)
    {
        var bodyWeight = await bodyWeightValidator.EnsureExistsAsync(bodyWeightId);

        if (bodyWeight.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "body weight");
    }

    public async Task ValidateGetByIdAsync(string userId, long bodyWeightId)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(bodyWeightId, "Body weight");

        var bodyWeight = await bodyWeightRepository.GetByIdAsync(bodyWeightId);

        if (bodyWeight != null && bodyWeight.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "body weight");

    }

    public async Task ValidateGetAllAsync(string userId, DateTimeRange? range)
    {
        await userValidator.EnsureExistsAsync(userId);

        if (range is not null)
            ArgumentValidator.ThrowIfRangeInFuture(range, nameof(range));
    }

    public async Task ValidateGetCurrentAsync(string userId)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetMaxAsync(string userId)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetMinAsync(string userId)
    {
        await userValidator.EnsureExistsAsync(userId);
    }
}
