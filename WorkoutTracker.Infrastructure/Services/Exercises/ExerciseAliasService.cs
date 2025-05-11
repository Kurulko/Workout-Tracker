using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Services.Base;

namespace WorkoutTracker.Infrastructure.Services.Exercises;

internal class ExerciseAliasService : BaseWorkoutService<ExerciseAliasService, ExerciseAlias>, IExerciseAliasService
{
    readonly IExerciseRepository exerciseRepository;
    public ExerciseAliasService(
        IExerciseAliasRepository exerciseAliasRepository, 
        IExerciseRepository exerciseRepository,
        ILogger<ExerciseAliasService> logger
    ) : base(exerciseAliasRepository, logger)
    {
        this.exerciseRepository = exerciseRepository;
    }

    readonly InvalidIDException invalidExerciseIDException = new(nameof(Exercise));

    readonly EntryNullException exerciseAliasIsNullException = new("Exercise record");
    readonly InvalidIDException invalidExerciseAliasIDException = new(nameof(ExerciseAlias));
    readonly ArgumentNullOrEmptyException exerciseAliasNameIsNullOrEmptyException = new("Exercise alias name");

    NotFoundException ExerciseNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID(nameof(Exercise), id);
    NotFoundException ExerciseAliasNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID("Exercise record", id);

    ArgumentException ExerciseAliasNameMustBeUnique()
        => EntryNameMustBeUnique("Exercise alias");

    public async Task<ServiceResult<ExerciseAlias>> AddExerciseAliasToExerciseAsync(long exerciseId, ExerciseAlias exerciseAlias)
    {
        try
        {
            await CheckExerciseIdAsync(exerciseId);

            if (exerciseAlias is null)
                throw exerciseAliasIsNullException;

            if (exerciseAlias.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(ExerciseAlias), "exercise alias");

            exerciseAlias.ExerciseId = exerciseId;
            await baseWorkoutRepository.AddAsync(exerciseAlias);

            return ServiceResult<ExerciseAlias>.Ok(exerciseAlias);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<ExerciseAlias>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("exercise alias", "add"));
            throw;
        }
    }

    public async Task<ServiceResult> DeleteExerciseAliasAsync(long id)
    {
        try
        {
            if (id < 1)
                throw invalidExerciseAliasIDException;

            var _ = await baseWorkoutRepository.GetByIdAsync(id) ?? throw ExerciseAliasNotFoundByIDException(id);

            await baseWorkoutRepository.RemoveAsync(id);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("exercise alias", "delete"));
            throw;
        }
    }

    public async Task<ServiceResult<ExerciseAlias>> GetExerciseAliasByIdAsync(long id)
    {
        try
        {
            if (id < 1)
                throw invalidExerciseAliasIDException;

            var exerciseAliasById = await baseWorkoutRepository.GetByIdAsync(id);
            return ServiceResult<ExerciseAlias>.Ok(exerciseAliasById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<ExerciseAlias>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("exercise alias", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<ExerciseAlias>> GetExerciseAliasByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                throw exerciseAliasNameIsNullOrEmptyException;

            var exerciseAliasById = await baseWorkoutRepository.GetByNameAsync(name);
            return ServiceResult<ExerciseAlias>.Ok(exerciseAliasById);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<ExerciseAlias>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("exercise alias by name", "get"));
            throw;
        }
    }

    public async Task<ServiceResult<IQueryable<ExerciseAlias>>> GetExerciseAliasesByExerciseIdAsync(long exerciseId)
    {
        try
        {
            await CheckExerciseIdAsync(exerciseId);

            var exerciseAliases = await baseWorkoutRepository.FindAsync(er => er.ExerciseId == exerciseId);
            return ServiceResult<IQueryable<ExerciseAlias>>.Ok(exerciseAliases);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult<IQueryable<ExerciseAlias>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("exercise aliases", "get"));
            throw;
        }
    }

    public async Task<ServiceResult> UpdateExerciseAliasAsync(ExerciseAlias exerciseAlias)
    {
        try
        {
            if (exerciseAlias is null)
                throw exerciseAliasIsNullException;

            if (exerciseAlias.Id < 1)
                throw invalidExerciseAliasIDException;

            await CheckExerciseIdAsync(exerciseAlias.ExerciseId);

            var _exerciseAlias = await baseWorkoutRepository.GetByIdAsync(exerciseAlias.Id) ?? throw ExerciseAliasNotFoundByIDException(exerciseAlias.Id);

            var isSameName = _exerciseAlias.Name != exerciseAlias.Name;
            var isUniqueEquipmentName = isSameName || await IsUniqueExerciseAliasNameForExerciseAsync(exerciseAlias.Name, exerciseAlias.ExerciseId);
            if (!isUniqueEquipmentName)
                throw ExerciseAliasNameMustBeUnique();

            _exerciseAlias.Name = exerciseAlias.Name;
            _exerciseAlias.ExerciseId = exerciseAlias.ExerciseId;
            await baseWorkoutRepository.UpdateAsync(_exerciseAlias);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("exercise alias", "update"));
            throw;
        }
    }


    async Task CheckExerciseIdAsync(long exerciseId)
    {
        if (exerciseId < 1)
            throw invalidExerciseIDException;

        bool exerciseExists = await exerciseRepository.ExistsAsync(exerciseId);
        if (!exerciseExists)
            throw ExerciseNotFoundByIDException(exerciseId);
    }

    async Task<bool> IsUniqueExerciseAliasNameForExerciseAsync(string name, long exerciseId)
    {
        var isAnyExerciseAliasNames = await baseWorkoutRepository.AnyAsync(ea => ea.Name == name && ea.ExerciseId == exerciseId);
        return !isAnyExerciseAliasNames;
    }
}
