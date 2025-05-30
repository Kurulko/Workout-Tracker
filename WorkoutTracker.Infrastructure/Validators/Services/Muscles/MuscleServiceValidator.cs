using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Validators.Models;
using WorkoutTracker.Infrastructure.Validators.Models.Muscles;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services.Muscles;

public class MuscleServiceValidator
{
    readonly MuscleValidator muscleValidator;
    readonly UserValidator userValidator;
    readonly FileUploadModelValidator fileUploadModelValidator;
    public MuscleServiceValidator(
        MuscleValidator muscleValidator,
        FileUploadModelValidator fileUploadModelValidator,
        UserValidator userValidator
    )
    {
        this.userValidator = userValidator;
        this.fileUploadModelValidator = fileUploadModelValidator;
        this.muscleValidator = muscleValidator;
    }

    public async Task ValidateAddAsync(Muscle muscle, CancellationToken cancellationToken)
    {
        await muscleValidator.ValidateForAddAsync(muscle, cancellationToken);
    }

    public async Task ValidateUpdateAsync(Muscle muscle, CancellationToken cancellationToken)
    {
        await muscleValidator.ValidateForEditAsync(muscle, cancellationToken);
    }

    public async Task ValidateUpdateChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs, CancellationToken cancellationToken)
    {
        await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);

        if (muscleChildIDs is not null)
        {
            foreach (var muscleChildID in muscleChildIDs)
                await muscleValidator.EnsureExistsAsync(muscleChildID, cancellationToken);
        }
    }

    public async Task ValidateDeleteAsync(long muscleId, CancellationToken cancellationToken)
    {
        await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);
    }

    public Task ValidateGetByIdAsync(long muscleId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(muscleId, "Muscle");
        return Task.CompletedTask;
    }

    public Task ValidateGetByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(Muscle.Name));
        return Task.CompletedTask;
    }

    public async Task ValidateGetByIdWithDetailsAsync(long muscleId, string userId, CancellationToken cancellationToken)
    {
        await ValidateGetByIdAsync(muscleId, cancellationToken);
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken)
    {
        await ValidateGetByNameAsync(name, cancellationToken);
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetAllAsync(long? parentMuscleId, bool? isMeasurable, CancellationToken cancellationToken)
    {
        if (parentMuscleId.HasValue)
            await muscleValidator.EnsureExistsAsync(parentMuscleId.Value, cancellationToken);
    }

    public async Task ValidateUpdatePhotoAsync(long muscleId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);

        if(fileUpload != null)
            fileUploadModelValidator.Validate(fileUpload);
    }

    public async Task ValidateDeletePhotoAsync(long muscleId, CancellationToken cancellationToken)
    {
        await muscleValidator.EnsureExistsAsync(muscleId, cancellationToken);
    }
}

