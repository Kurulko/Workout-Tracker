using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Infrastructure.Validators.Services.Muscles;

namespace WorkoutTracker.Infrastructure.Services.Muscles;

internal class MuscleService : BaseWorkoutService<MuscleService, Muscle>, IMuscleService
{
    readonly IMuscleRepository muscleRepository;
    readonly IFileService fileService;
    readonly MuscleServiceValidator muscleServiceValidator;

    public MuscleService(
        IMuscleRepository muscleRepository,
        IFileService fileService,
        MuscleServiceValidator muscleServiceValidator,
        ILogger<MuscleService> logger
    ) : base(muscleRepository, logger)
    {
        this.muscleRepository = muscleRepository;
        this.fileService = fileService;
        this.muscleServiceValidator = muscleServiceValidator;
    }

    const string muscleEntityName = "muscle";

    readonly string musclePhotosDirectory = Path.Combine("images", "muscles");
    const int maxMuscleImageSizeInMB = 3;

    public async Task<Muscle> AddMuscleAsync(Muscle muscle, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateAddAsync(muscle, cancellationToken);

        return await muscleRepository.AddAsync(muscle)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "add"));
    }

    public async Task DeleteMuscleAsync(long muscleId, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateDeleteAsync(muscleId, cancellationToken);

        var muscle = await muscleRepository.GetByIdAsync(muscleId);
        string? muscleImage = muscle?.Image;

        await muscleRepository.RemoveAsync(muscleId)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "delete"));

        if (!string.IsNullOrEmpty(muscleImage))
            fileService.DeleteFile(muscleImage);
    }

    public async Task<Muscle?> GetMuscleByIdAsync(long muscleId, string userId, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateGetByIdAsync(muscleId, cancellationToken);

        var muscle = await muscleRepository.GetByIdAsync(muscleId)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "get"));

        return muscle;
    }

    public async Task<Muscle?> GetMuscleByNameAsync(string name, string userId, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateGetByNameAsync(name, cancellationToken);

        var muscle = await muscleRepository.GetByNameAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "get"));

        return muscle;
    }

    public async Task<Muscle?> GetMuscleByIdWithDetailsAsync(long muscleId, string userId, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateGetByIdWithDetailsAsync(muscleId, userId, cancellationToken);

        var muscle = await muscleRepository.GetMuscleByIdWithDetailsAsync(muscleId, userId)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "get"));

        return muscle;
    }

    public async Task<Muscle?> GetMuscleByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateGetByNameWithDetailsAsync(name, userId, cancellationToken);

        var muscle = await muscleRepository.GetMuscleByNameWithDetailsAsync(name, userId)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "get"));

        return muscle;
    }

    public async Task<IEnumerable<Muscle>> GetMusclesAsync(long? parentMuscleId, bool? isMeasurable, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateGetAllAsync(parentMuscleId, isMeasurable, cancellationToken);

        var muscles = muscleRepository.GetMuscles(parentMuscleId, isMeasurable);

        return await muscles.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("muscles", "get"));
    }

    public async Task<IEnumerable<Muscle>> GetParentMusclesAsync(CancellationToken cancellationToken)
    {
        var parentMuscles = muscleRepository.GetParentMuscles();

        return await parentMuscles.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("parent muscles", "get"));
    }

    public async Task<IEnumerable<Muscle>> GetChildMusclesAsync(CancellationToken cancellationToken)
    {
        var childMuscles = muscleRepository.GetChildMuscles();

        return await childMuscles.ToListAsync(cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("child muscles", "get"));
    }

    public async Task UpdateMuscleAsync(Muscle muscle, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateUpdateAsync(muscle, cancellationToken);

        var updateAction = new Action<Muscle>(m =>
        {
            m.Name = muscle.Name;
            //m.Image = muscle.Image;
            m.ParentMuscleId = muscle.ParentMuscleId;
            m.IsMeasurable = muscle.IsMeasurable;
        });

        await muscleRepository.UpdatePartialAsync(muscle.Id, updateAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "update"));
    }

    public async Task UpdateMusclePhotoAsync(long muscleId, FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateUpdatePhotoAsync(muscleId, fileUpload, cancellationToken);

        string? image = await fileService.GetImageAsync(fileUpload, musclePhotosDirectory, maxMuscleImageSizeInMB, false);
        var oldImage = await muscleRepository.GetMusclePhotoAsync(muscleId, cancellationToken);

        await (!string.IsNullOrEmpty(image) ?
            muscleRepository.UpdateMusclePhotoAsync(muscleId, image!, cancellationToken) :
            muscleRepository.DeleteMusclePhotoAsync(muscleId, cancellationToken)
        ).LogExceptionsAsync(_logger, FailedToActionStr("muscle photo", "update"));

        if (!string.IsNullOrEmpty(oldImage))
            fileService.DeleteFile(oldImage);
    }

    public async Task DeleteMusclePhotoAsync(long muscleId, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateDeletePhotoAsync(muscleId, cancellationToken);

        var oldImage = await muscleRepository.GetMusclePhotoAsync(muscleId, cancellationToken);

        if (!string.IsNullOrEmpty(oldImage))
        {
            await muscleRepository.DeleteMusclePhotoAsync(muscleId, cancellationToken)
                .LogExceptionsAsync(_logger, FailedToActionStr("muscle photo", "delete"));
            
            fileService.DeleteFile(oldImage);
        }
    }

    public async Task UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs, CancellationToken cancellationToken)
    {
        await muscleServiceValidator.ValidateUpdateChildrenAsync(muscleId, muscleChildIDs, cancellationToken);

        var updateChildMusclesAction = new Action<Muscle>(m =>
        {
            m.ChildMuscles = muscleChildIDs is null ? null : [.. muscleRepository.FindByIds(muscleChildIDs)];
        });

        await muscleRepository.UpdatePartialAsync(muscleId, updateChildMusclesAction, cancellationToken)
            .LogExceptionsAsync(_logger, FailedToActionStr("muscle children", "update"));
    }
}
