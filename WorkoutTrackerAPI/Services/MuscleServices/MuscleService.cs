using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.MuscleServices;

namespace WorkoutTrackerAPI.Services;

public class MuscleService : BaseWorkoutService<Muscle>, IMuscleService
{
    readonly UserRepository userRepository;
    public MuscleService(MuscleRepository baseWorkoutRepository, UserRepository userRepository) : base(baseWorkoutRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException muscleIsNullException = new (nameof(Muscle));
    readonly InvalidIDException invalidMuscleIDException = new (nameof(Muscle));
    readonly NotFoundException muscleNotFoundException = new (nameof(Muscle));
    readonly ArgumentNullOrEmptyException muscleNameIsNullOrEmptyException = new("Muscle name");

    public async Task<ServiceResult<Muscle>> AddMuscleAsync(Muscle muscle)
    {
        if (muscle is null)
            return ServiceResult<Muscle>.Fail(muscleIsNullException);

        if (muscle.Id != 0)
            return ServiceResult<Muscle>.Fail(InvalidEntryIDWhileAddingStr(nameof(Muscle), "muscle"));

        try
        {
            await baseWorkoutRepository.AddAsync(muscle);
            return ServiceResult<Muscle>.Ok(muscle);
        }
        catch (Exception ex)
        {
            return ServiceResult<Muscle>.Fail(FailedToAction("muscle", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteMuscleAsync(long muscleId)
    {
        if (muscleId < 1)
            return ServiceResult.Fail(invalidMuscleIDException);

        Muscle? muscle = await baseWorkoutRepository.GetByIdAsync(muscleId);

        if (muscle is null)
            return ServiceResult.Fail(new NotFoundException(nameof(Muscle)));

        try
        {
            await baseWorkoutRepository.RemoveAsync(muscleId);
            return ServiceResult.Ok();
        }
        catch
        {
            return ServiceResult.Fail(FailedToAction("delete", "delete"));
        }
    }

    public async Task<ServiceResult<Muscle>> GetMuscleByIdAsync(long muscleId)
    {
        if (muscleId < 1)
            return ServiceResult<Muscle>.Fail(invalidMuscleIDException);

        try
        {
            var muscleById = await baseWorkoutRepository.GetByIdAsync(muscleId);
            return ServiceResult<Muscle>.Ok(muscleById);
        }
        catch (Exception ex)
        {
            return ServiceResult<Muscle>.Fail(FailedToAction("muscle", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<Muscle>> GetMuscleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ServiceResult<Muscle>.Fail(muscleNameIsNullOrEmptyException);

        try
        {
            var muscleByName = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<Muscle>.Ok(muscleByName);
        }
        catch (Exception ex)
        {
            return ServiceResult<Muscle>.Fail(FailedToAction("muscle by name", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<IQueryable<Muscle>>> GetMusclesAsync()
    {
        try
        {
            var muscles = await baseWorkoutRepository.GetAllAsync();
            return ServiceResult<IQueryable<Muscle>>.Ok(muscles);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<Muscle>>.Fail(FailedToAction("muscles", "get", ex.Message));
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
                return ServiceResult.Fail(muscleNotFoundException);

            await baseWorkoutRepository.UpdateAsync(muscle);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToAction("muscle", "update", ex.Message));
        }
    }
}
