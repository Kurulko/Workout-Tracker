using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;

namespace WorkoutTracker.Infrastructure.Services.Muscles;

internal class MuscleService : BaseWorkoutService<MuscleService, Muscle>, IMuscleService
{
    readonly IMuscleRepository muscleRepository;
    readonly IFileService fileService;
    public MuscleService(
        IMuscleRepository muscleRepository,
        IFileService fileService,
        ILogger<MuscleService> logger
    ) : base(muscleRepository, logger)
    {
        this.muscleRepository = muscleRepository;
        this.fileService = fileService;
    }

    readonly EntryNullException muscleIsNullException = new(nameof(Muscle));
    readonly InvalidIDException invalidMuscleIDException = new(nameof(Muscle));
    readonly ArgumentNullOrEmptyException muscleNameIsNullOrEmptyException = new("Muscle name");

    NotFoundException MuscleNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID(nameof(Muscle), id);
    NotFoundException MuscleNotFoundByNameException(string name)
        => NotFoundException.NotFoundExceptionByName(nameof(Muscle), name);

    ValidationException MuscleNameMustBeUnique()
        => EntryNameMustBeUnique(nameof(Muscle));

    public async Task<ServiceResult<Muscle>> AddMuscleAsync(Muscle muscle)
    {
        try
        {
            if (muscle is null)
                throw muscleIsNullException;

            if (muscle.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(Muscle), "muscle");

            if (await baseWorkoutRepository.ExistsByNameAsync(muscle.Name))
                throw MuscleNameMustBeUnique();

            await baseWorkoutRepository.AddAsync(muscle);
            return ServiceResult<Muscle>.Ok(muscle);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Muscle>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("muscle", "add"));
            throw;
        }
    }

    public async Task<ServiceResult> DeleteMuscleAsync(long muscleId)
    {
        try
        {
            if (muscleId < 1)
                throw invalidMuscleIDException;

            Muscle? muscle = await baseWorkoutRepository.GetByIdAsync(muscleId);

            if (muscle is null)
                throw MuscleNotFoundByIDException(muscleId);

            string? muscleImage = muscle.Image;
            await baseWorkoutRepository.RemoveAsync(muscleId);

            if (!string.IsNullOrEmpty(muscleImage))
            {
                fileService.DeleteFile(muscleImage);
            }

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("muscle", "delete"));
            throw;
        }
    }

    public async Task<ServiceResult<Muscle>> GetMuscleByIdAsync(long muscleId, string userId, bool withDetails = false)
    {

        try
        {
            if (muscleId < 1)
                throw invalidMuscleIDException;

            var muscleById = withDetails ? await muscleRepository.GetMuscleByIdWithDetailsAsync(muscleId, userId) : await baseWorkoutRepository.GetByIdAsync(muscleId);
            return ServiceResult<Muscle>.Ok(muscleById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Muscle>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("muscle", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<Muscle>> GetMuscleByNameAsync(string name, string userId, bool withDetails = false)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                throw muscleNameIsNullOrEmptyException;

            var muscleByName = withDetails ? await muscleRepository.GetMuscleByNameWithDetailsAsync(name, userId) : await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Muscle>.Ok(muscleByName);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<Muscle>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("muscle by name", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<Muscle>>> GetMusclesAsync(long? parentMuscleId = null, bool? isMeasurable = null)
    {
        try
        {
            if (parentMuscleId.HasValue && parentMuscleId < 1)
                throw invalidMuscleIDException;

            var muscles = await baseWorkoutRepository.GetAllAsync();

            if (parentMuscleId.HasValue)
                muscles = muscles.Where(m => m.ParentMuscleId == parentMuscleId);

            if (isMeasurable.HasValue)
                muscles = muscles.Where(m => m.IsMeasurable == isMeasurable);

            return ServiceResult<IQueryable<Muscle>>.Ok(muscles);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<Muscle>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("muscles", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<Muscle>>> GetParentMusclesAsync()
    {
        try
        {
            var muscles = await baseWorkoutRepository.FindAsync(m => m.ChildMuscles != null && m.ChildMuscles.Count() != 0);
            return ServiceResult<IQueryable<Muscle>>.Ok(muscles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("parent muscles", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<Muscle>>> GetChildMusclesAsync()
    {
        try
        {
            var muscles = await baseWorkoutRepository.FindAsync(m => m.ParentMuscleId != null);
            return ServiceResult<IQueryable<Muscle>>.Ok(muscles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("child muscles", "get"));
            throw;
        }
    }

    public async Task<bool> MuscleExistsAsync(long muscleId)
    {
        if (muscleId < 1)
            throw invalidMuscleIDException;

        return await baseWorkoutRepository.ExistsAsync(muscleId);
    }

    public async Task<bool> MuscleExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw muscleNameIsNullOrEmptyException;

        return await baseWorkoutRepository.ExistsByNameAsync(name);
    }

    public async Task<ServiceResult> UpdateMuscleAsync(Muscle muscle)
    {
        try
        {
            if (muscle is null)
                throw muscleIsNullException;

            if (muscle.Id < 1)
                throw invalidMuscleIDException;

            Muscle? _muscle = await baseWorkoutRepository.GetByIdAsync(muscle.Id);

            if (_muscle is null)
                return ServiceResult.Fail(MuscleNotFoundByIDException(muscle.Id));

            var isSameName = _muscle.Name != muscle.Name;
            var isUniqueMuscleName = isSameName || await baseWorkoutRepository.ExistsByNameAsync(muscle.Name);
            if (!isUniqueMuscleName)
                return ServiceResult.Fail(MuscleNameMustBeUnique());

            _muscle.Name = muscle.Name;
            _muscle.Image = muscle.Image;
            _muscle.ParentMuscleId = muscle.ParentMuscleId;

            await baseWorkoutRepository.UpdateAsync(_muscle);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("muscle", "update"));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs)
    {
        try
        {
            if (muscleId < 1)
                throw invalidMuscleIDException;

            var muscle = await baseWorkoutRepository.GetByIdAsync(muscleId) ?? throw MuscleNotFoundByIDException(muscleId);
            muscle.ChildMuscles = muscleChildIDs is null ? null : (await baseWorkoutRepository.FindAsync(m => muscleChildIDs.Contains(m.Id))).ToList();

            await baseWorkoutRepository.UpdateAsync(muscle);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("muscle children", "update"));
            throw;
        }
    }
}
