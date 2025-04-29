using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Enums;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;

namespace WorkoutTrackerAPI.Services.ExerciseRecordServices;

public class ExerciseRecordService : DbModelService<ExerciseRecord>, IExerciseRecordService
{
    readonly UserRepository userRepository;
    public ExerciseRecordService(ExerciseRecordRepository baseRepository, UserRepository userRepository) : base(baseRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException exerciseRecordIsNullException = new("Exercise record");
    readonly InvalidIDException invalidExerciseRecordIDException = new(nameof(ExerciseRecord));
    NotFoundException ExerciseRecordNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID("Exercise record", id);

    public async Task<ServiceResult<ExerciseRecord>> AddExerciseRecordToUserAsync(string userId, ExerciseRecord exerciseRecord)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecord is null)
                throw exerciseRecordIsNullException;

            if (exerciseRecord.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(ExerciseRecord), "exercise record");

            exerciseRecord.UserId = userId;
            exerciseRecord.Date = DateTime.Now;

            await baseRepository.AddAsync(exerciseRecord);

            return ServiceResult<ExerciseRecord>.Ok(exerciseRecord);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToActionStr("exercise record", "add", ex));
        }
    }

    public async Task<ServiceResult> DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecordId < 1)
                throw invalidExerciseRecordIDException;

            ExerciseRecord? exerciseRecord = await baseRepository.GetByIdAsync(exerciseRecordId) ?? throw ExerciseRecordNotFoundByIDException(exerciseRecordId);

            if (exerciseRecord.UserId != userId)
                throw UserNotHavePermissionException("delete", "exercise record");

            await baseRepository.RemoveAsync(exerciseRecordId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record", "delete"));
        }
    }

    public async Task<ServiceResult<IQueryable<ExerciseRecord>>> GetUserExerciseRecordsAsync(string userId, long? exerciseId = null, ExerciseType? exerciseType = null, DateTimeRange? range = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (range is DateTimeRange _range && _range.LastDate > DateTime.Now.Date)
                throw new ArgumentException("Incorrect date.");

            if (exerciseId.HasValue && exerciseId < 1)
                throw new InvalidIDException(nameof(Exercise));

            IEnumerable<ExerciseRecord> userExerciseRecords = (await baseRepository.FindAsync(wr => wr.UserId == userId)).ToList();

            if (range is not null)
                userExerciseRecords = userExerciseRecords.Where(ms => range.IsDateInRange(ms.Date, true));

            if (exerciseId.HasValue)
                userExerciseRecords = userExerciseRecords.Where(ms => ms.ExerciseId == exerciseId);
            else if(exerciseType.HasValue)
                userExerciseRecords = userExerciseRecords.Where(ms => ms.Exercise!.Type == exerciseType);

            return ServiceResult<IQueryable<ExerciseRecord>>.Ok(userExerciseRecords.AsQueryable());
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<ExerciseRecord>>.Fail(FailedToActionStr("exercise records", "get", ex));
        }
    }

    public async Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId)
    {

        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecordId < 1)
                throw invalidExerciseRecordIDException;

            var userExerciseRecordById = await baseRepository.GetByIdAsync(exerciseRecordId);
            return ServiceResult<ExerciseRecord>.Ok(userExerciseRecordById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<ExerciseRecord>.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult<ExerciseRecord>.Fail(FailedToActionStr("exercise record", "get", ex));
        }
    }

    public async Task<ServiceResult> UpdateUserExerciseRecordAsync(string userId, ExerciseRecord exerciseRecord)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (exerciseRecord is null)
                throw exerciseRecordIsNullException;

            if (exerciseRecord.Id < 1)
                throw invalidExerciseRecordIDException;

            var _exerciseRecord = await baseRepository.GetByIdAsync(exerciseRecord.Id) ?? throw ExerciseRecordNotFoundByIDException(exerciseRecord.Id);

            if (_exerciseRecord.UserId != userId)
                throw UserNotHavePermissionException("update", "exercise record");

            _exerciseRecord.Date = exerciseRecord.Date;
            _exerciseRecord.Weight = exerciseRecord.Weight;
            _exerciseRecord.Time = exerciseRecord.Time;
            _exerciseRecord.Reps = exerciseRecord.Reps;
            _exerciseRecord.ExerciseId = exerciseRecord.ExerciseId;

            await baseRepository.UpdateAsync(_exerciseRecord);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("exercise record", "update", ex));
        }
    }
}
