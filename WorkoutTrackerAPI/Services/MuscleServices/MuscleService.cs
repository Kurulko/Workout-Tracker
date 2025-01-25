using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Services.FileServices;

namespace WorkoutTrackerAPI.Services;

public class MuscleService : BaseWorkoutService<Muscle>, IMuscleService
{
    readonly MuscleRepository muscleRepository;
    readonly UserRepository userRepository;
    readonly IFileService fileService;
    public MuscleService(MuscleRepository muscleRepository, UserRepository userRepository, IFileService fileService) : base(muscleRepository)
    {
        this.muscleRepository = muscleRepository;
        this.userRepository = userRepository;
        this.fileService = fileService;
    }

    readonly EntryNullException muscleIsNullException = new (nameof(Muscle));
    readonly InvalidIDException invalidMuscleIDException = new (nameof(Muscle));
    readonly ArgumentNullOrEmptyException muscleNameIsNullOrEmptyException = new("Muscle name");

    NotFoundException MuscleNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID(nameof(Muscle), id);
    NotFoundException MuscleNotFoundByNameException(string name)
        => NotFoundException.NotFoundExceptionByName(nameof(Muscle), name);

    public async Task<ServiceResult<Muscle>> AddMuscleAsync(Muscle muscle)
    {
        if (muscle is null)
            return ServiceResult<Muscle>.Fail(muscleIsNullException);

        if (muscle.Id != 0)
            return ServiceResult<Muscle>.Fail(InvalidEntryIDWhileAddingStr(nameof(Muscle), "muscle"));

        if (await baseWorkoutRepository.ExistsByNameAsync(muscle.Name))
            throw EntryNameMustBeUnique(nameof(Muscle));

        try
        {
            await baseWorkoutRepository.AddAsync(muscle);
            return ServiceResult<Muscle>.Ok(muscle);
        }
        catch (Exception ex)
        {
            return ServiceResult<Muscle>.Fail(FailedToActionStr("muscle", "add", ex));
        }
    }

    public async Task<ServiceResult> DeleteMuscleAsync(long muscleId)
    {
        if (muscleId < 1)
            return ServiceResult.Fail(invalidMuscleIDException);

        Muscle? muscle = await baseWorkoutRepository.GetByIdAsync(muscleId);

        if (muscle is null)
            return ServiceResult.Fail(MuscleNotFoundByIDException(muscleId));

        string? muscleImage = muscle.Image;

        try
        {
            await baseWorkoutRepository.RemoveAsync(muscleId);

            if (!string.IsNullOrEmpty(muscleImage))
            {
                fileService.DeleteFile(muscleImage);
            }

            return ServiceResult.Ok();
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("muscle", "delete"));
        }
    }

    public async Task<ServiceResult<Muscle>> GetMuscleByIdAsync(long muscleId, string userId, bool withDetails = false)
    {
        if (muscleId < 1)
            return ServiceResult<Muscle>.Fail(invalidMuscleIDException);

        try
        {
            var muscleById = withDetails ? await muscleRepository.GetMuscleByIdWithDetailsAsync(muscleId, userId) : await baseWorkoutRepository.GetByIdAsync(muscleId);
            return ServiceResult<Muscle>.Ok(muscleById);
        }
        catch (Exception ex)
        {
            return ServiceResult<Muscle>.Fail(FailedToActionStr("muscle", "get", ex));
        }
    }

    public async Task<ServiceResult<Muscle>> GetMuscleByNameAsync(string name, string userId, bool withDetails = false)
    {
        if (string.IsNullOrEmpty(name))
            return ServiceResult<Muscle>.Fail(muscleNameIsNullOrEmptyException);

        try
        {
            var muscleByName = withDetails ? await muscleRepository.GetMuscleByNameWithDetailsAsync(name, userId) : await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Muscle>.Ok(muscleByName);
        }
        catch (Exception ex)
        {
            return ServiceResult<Muscle>.Fail(FailedToActionStr("muscle by name", "get", ex));
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
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Muscle>>.Fail(FailedToActionStr("muscles", "get", ex));
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
            return ServiceResult<IQueryable<Muscle>>.Fail(FailedToActionStr("parent muscles", "get", ex));
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
            return ServiceResult<IQueryable<Muscle>>.Fail(FailedToActionStr("child muscles", "get", ex));
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
        if (muscle is null)
            return ServiceResult.Fail(muscleIsNullException);

        if (muscle.Id < 1)
            return ServiceResult.Fail(invalidMuscleIDException);

        try
        {
            Muscle? _muscle = await baseWorkoutRepository.GetByIdAsync(muscle.Id);

            if (_muscle is null)
                return ServiceResult.Fail(MuscleNotFoundByIDException(muscle.Id));

            _muscle.Name = muscle.Name;
            _muscle.Image = muscle.Image;
            _muscle.ParentMuscleId = muscle.ParentMuscleId;

            await baseWorkoutRepository.UpdateAsync(_muscle);
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("muscle", "update", ex));
        }
    }

    public async Task<ServiceResult> UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs)
    {
        if (muscleId < 1)
            return ServiceResult.Fail(invalidMuscleIDException);

        try
        {
            var muscle = await baseWorkoutRepository.GetByIdAsync(muscleId) ?? throw MuscleNotFoundByIDException(muscleId);
            muscle.ChildMuscles = muscleChildIDs is null ? null : (await baseWorkoutRepository.FindAsync(m => muscleChildIDs.Contains(m.Id))).ToList();

            await baseWorkoutRepository.UpdateAsync(muscle);
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("muscle", "update", ex));
        }
    }
}
