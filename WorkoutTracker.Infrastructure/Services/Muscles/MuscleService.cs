using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
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

    public async Task<Muscle> AddMuscleAsync(Muscle muscle)
    {
        await muscleServiceValidator.ValidateAddAsync(muscle);

        return await baseWorkoutRepository.AddAsync(muscle)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "add"));
    }

    public async Task DeleteMuscleAsync(long muscleId)
    {
        await muscleServiceValidator.ValidateDeleteAsync(muscleId);

        var muscle = await baseWorkoutRepository.GetByIdAsync(muscleId);
        string? muscleImage = muscle?.Image;

        await baseWorkoutRepository.RemoveAsync(muscleId)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "delete"));

        if (!string.IsNullOrEmpty(muscleImage))
        {
            fileService.DeleteFile(muscleImage);
        }
    }

    public async Task<Muscle?> GetMuscleByIdAsync(long muscleId, string userId, bool withDetails = false)
    {
        await muscleServiceValidator.ValidateGetByIdAsync(muscleId, userId, withDetails);

        var muscle = await (withDetails ?
            muscleRepository.GetMuscleByIdWithDetailsAsync(muscleId, userId) :
            baseWorkoutRepository.GetByIdAsync(muscleId)
        )
        .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "get"));

        return muscle;
    }

    public async Task<Muscle?> GetMuscleByNameAsync(string name, string userId, bool withDetails = false)
    {
        await muscleServiceValidator.ValidateGetByNameAsync(name, userId, withDetails);

        var muscle = await (withDetails ?
            muscleRepository.GetMuscleByNameWithDetailsAsync(name, userId) :
            baseWorkoutRepository.GetByNameAsync(name)
        )
        .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "get"));

        return muscle;
    }

    public async Task<IQueryable<Muscle>> GetMusclesAsync(long? parentMuscleId = null, bool? isMeasurable = null)
    {
        await muscleServiceValidator.ValidateGetAllAsync(parentMuscleId, isMeasurable);

        var muscles = await baseWorkoutRepository.GetAllAsync()
            .LogExceptionsAsync(_logger, FailedToActionStr("muscles", "get"));

        if (parentMuscleId.HasValue)
            muscles = muscles.Where(m => m.ParentMuscleId == parentMuscleId);

        if (isMeasurable.HasValue)
            muscles = muscles.Where(m => m.IsMeasurable == isMeasurable);

        return muscles;
    }

    public async Task<IQueryable<Muscle>> GetParentMusclesAsync()
    {
        return await baseWorkoutRepository.FindAsync(m => m.ChildMuscles != null && m.ChildMuscles.Count() != 0)
            .LogExceptionsAsync(_logger, FailedToActionStr("parent muscles", "get"));
    }

    public async Task<IQueryable<Muscle>> GetChildMusclesAsync()
    {
        return await baseWorkoutRepository.FindAsync(m => m.ParentMuscleId != null)
            .LogExceptionsAsync(_logger, FailedToActionStr("child muscles", "get"));
    }

    public async Task UpdateMuscleAsync(Muscle muscle)
    {
        await muscleServiceValidator.ValidateUpdateAsync(muscle);

        var _muscle = (await baseWorkoutRepository.GetByIdAsync(muscle.Id))!;

        _muscle.Name = muscle.Name;
        _muscle.Image = muscle.Image;
        _muscle.ParentMuscleId = muscle.ParentMuscleId;
        _muscle.IsMeasurable = muscle.IsMeasurable;

        await baseWorkoutRepository.UpdateAsync(_muscle)
            .LogExceptionsAsync(_logger, FailedToActionStr(muscleEntityName, "update"));
    }

    public async Task UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs)
    {
        await muscleServiceValidator.ValidateUpdateChildrenAsync(muscleId, muscleChildIDs);

        var muscle = (await baseWorkoutRepository.GetByIdAsync(muscleId))!;

        muscle.ChildMuscles = muscleChildIDs is null ? null : (await baseWorkoutRepository.FindAsync(m => muscleChildIDs.Contains(m.Id))).ToList();

        await baseWorkoutRepository.UpdateAsync(muscle)
            .LogExceptionsAsync(_logger, FailedToActionStr("muscle children", "update"));
    }
}
