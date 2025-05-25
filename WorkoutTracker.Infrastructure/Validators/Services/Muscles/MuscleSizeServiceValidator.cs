using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Validators.Models.Muscles;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services.Muscles;

public class MuscleSizeServiceValidator
{
    readonly MuscleValidator muscleValidator;
    readonly MuscleSizeValidator muscleSizeValidator;
    readonly UserValidator userValidator;
    readonly IMuscleSizeRepository muscleSizeRepository;

    public MuscleSizeServiceValidator(
        IMuscleSizeRepository muscleSizeRepository,
        UserValidator userValidator,
        MuscleValidator muscleValidator,
        MuscleSizeValidator muscleSizeValidator
    )
    {
        this.userValidator = userValidator;
        this.muscleValidator = muscleValidator;
        this.muscleSizeValidator = muscleSizeValidator;
        this.muscleSizeRepository = muscleSizeRepository;
    }

    public async Task ValidateAddAsync(string userId, MuscleSize muscleSize)
    {
        await userValidator.EnsureExistsAsync(userId);

        await muscleSizeValidator.ValidateForAddAsync(muscleSize);
        await muscleValidator.EnsureExistsAsync(muscleSize.MuscleId);
    }

    public async Task ValidateUpdateAsync(string userId, MuscleSize muscleSize)
    {
        await userValidator.EnsureExistsAsync(userId);
        var _muscleSize = await muscleSizeValidator.ValidateForEditAsync(muscleSize);

        if (_muscleSize.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "muscle size");

        await muscleValidator.EnsureExistsAsync(muscleSize.MuscleId);

    }

    public async Task ValidateDeleteAsync(string userId, long muscleId)
    {
        await userValidator.EnsureExistsAsync(userId);

        var muscleSize = await muscleSizeValidator.EnsureExistsAsync(muscleId);

        if (muscleSize.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("delete", "muscle size");
    }

    public async Task ValidateGetByIdAsync(string userId, long muscleSizeId)
    {
        await userValidator.EnsureExistsAsync(userId);
        ArgumentValidator.ThrowIfIdNonPositive(muscleSizeId, "Muscle Size");

        var muscleSize = await muscleSizeRepository.GetByIdAsync(muscleSizeId);

        if (muscleSize != null && muscleSize.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("get", "muscle size");

    }

    public async Task ValidateGetMinAsync(string userId, long muscleId)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNonPositive(muscleId, "Muscle");
    }

    public async Task ValidateGetMaxAsync(string userId, long muscleId)
    {
        await userValidator.EnsureExistsAsync(userId);

        ArgumentValidator.ThrowIfIdNonPositive(muscleId, "Muscle");
    }

    public async Task ValidateGetAllAsync(string userId, long? muscleId, DateTimeRange? range)
    {
        await userValidator.EnsureExistsAsync(userId);

        if (muscleId.HasValue)
            await muscleValidator.EnsureExistsAsync(muscleId.Value);

        if (range is not null)
            ArgumentValidator.ThrowIfRangeInFuture(range, nameof(range));
    }
}
