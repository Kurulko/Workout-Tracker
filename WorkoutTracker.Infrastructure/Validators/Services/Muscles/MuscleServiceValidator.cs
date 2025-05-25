using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Validators.Models.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Muscles;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services.Muscles;

public class MuscleServiceValidator
{
    readonly MuscleValidator muscleValidator;
    readonly UserValidator userValidator;

    public MuscleServiceValidator(
        MuscleValidator muscleValidator,
        UserValidator userValidator
    )
    {
        this.userValidator = userValidator;
        this.muscleValidator = muscleValidator;
    }

    public async Task ValidateAddAsync(Muscle muscle)
    {
        await muscleValidator.ValidateForAddAsync(muscle);
    }

    public async Task ValidateUpdateAsync(Muscle muscle)
    {
        await muscleValidator.ValidateForEditAsync(muscle);
    }

    public async Task ValidateUpdateChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs)
    {
        await muscleValidator.EnsureExistsAsync(muscleId);

        if (muscleChildIDs is not null)
        {
            foreach (var muscleChildID in muscleChildIDs)
                await muscleValidator.EnsureExistsAsync(muscleChildID);
        }
    }

    public async Task ValidateDeleteAsync(long muscleId)
    {
        await muscleValidator.EnsureExistsAsync(muscleId);
    }

    public async Task ValidateGetByIdAsync(long muscleId, string userId, bool withDetails)
    {
        ArgumentValidator.ThrowIfIdNonPositive(muscleId, "Muscle");

        if (withDetails)
            await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetByNameAsync(string name, string userId, bool withDetails)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(Muscle.Name));

        if (withDetails)
            await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetAllAsync(long? parentMuscleId, bool? isMeasurable)
    {
        if (parentMuscleId.HasValue)
            await muscleValidator.EnsureExistsAsync(parentMuscleId.Value);

    }
}

