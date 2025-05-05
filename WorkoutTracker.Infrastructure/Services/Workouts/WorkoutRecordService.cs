using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions;

namespace WorkoutTracker.Infrastructure.Services.Workouts;

internal class WorkoutRecordService : DbModelService<WorkoutRecord>, IWorkoutRecordService
{
    readonly IUserRepository userRepository;
    readonly IWorkoutRepository workoutRepository;
    readonly IExerciseRecordRepository exerciseRecordRepository;
    readonly IExerciseRecordGroupRepository exerciseRecordGroupRepository;
    public WorkoutRecordService(
        IWorkoutRecordRepository baseRepository,
        IWorkoutRepository workoutRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IExerciseRecordGroupRepository exerciseRecordGroupRepository,
        IUserRepository userRepository
    ) : base(baseRepository)
    {
        this.workoutRepository = workoutRepository;
        this.exerciseRecordRepository = exerciseRecordRepository;
        this.exerciseRecordGroupRepository = exerciseRecordGroupRepository;
        this.userRepository = userRepository;
    }

    readonly EntryNullException workoutRecordIsNullException = new("Workout record");
    readonly InvalidIDException invalidWorkoutRecordIDException = new(nameof(WorkoutRecord));

    NotFoundException WorkoutNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID(nameof(Workout), id);
    NotFoundException WorkoutRecordNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID("Workout record", id);


    public async Task<ServiceResult<WorkoutRecord>> AddWorkoutRecordToUserAsync(string userId, WorkoutRecord workoutRecord)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutRecord is null)
                throw workoutRecordIsNullException;

            if (workoutRecord.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(WorkoutRecord), "workout record");

            Workout? workout = await workoutRepository.GetByIdAsync(workoutRecord.WorkoutId) ?? throw WorkoutNotFoundByIDException(workoutRecord.WorkoutId);

            var exerciseRecordGroups = workoutRecord.ExerciseRecordGroups;

            workoutRecord.UserId = userId;
            workoutRecord.ExerciseRecordGroups = null!;
            await baseRepository.AddAsync(workoutRecord);

            await AddExerciseRecordGroups(userId, workoutRecord.Id, workoutRecord.Date, exerciseRecordGroups);

            workout.CountOfTrainings++;
            await workoutRepository.UpdateAsync(workout);
            await UpdateUserFirstWorkoutDate(userId);

            return ServiceResult<WorkoutRecord>.Ok(workoutRecord);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<WorkoutRecord>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<WorkoutRecord>.Fail(FailedToActionStr("workout record", "add", ex));
        }
    }

    public async Task<ServiceResult> DeleteWorkoutRecordFromUserAsync(string userId, long workoutRecordId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutRecordId < 1)
                throw invalidWorkoutRecordIDException;

            WorkoutRecord? workoutRecord = await baseRepository.GetByIdAsync(workoutRecordId) ?? throw WorkoutRecordNotFoundByIDException(workoutRecordId);

            if (workoutRecord.UserId != userId)
                throw UserNotHavePermissionException("delete", "workout record");

            await baseRepository.RemoveAsync(workoutRecordId);

            Workout workout = (await workoutRepository.GetByIdAsync(workoutRecord.WorkoutId))!;

            workout.CountOfTrainings--;
            await workoutRepository.UpdateAsync(workout);
            await UpdateUserFirstWorkoutDate(userId);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("workout record", "delete"));
        }
    }

    public async Task<ServiceResult<IQueryable<WorkoutRecord>>> GetUserWorkoutRecordsAsync(string userId, long? workoutId = null, DateTimeRange? range = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (range is DateTimeRange _range && _range.LastDate > DateTime.Now.Date)
                throw new ArgumentException("Incorrect date.");

            if (workoutId.HasValue && workoutId < 1)
                throw new InvalidIDException(nameof(Workout));

            IEnumerable<WorkoutRecord> userWorkoutRecords = (await baseRepository.FindAsync(wr => wr.UserId == userId)).ToList();

            if (range is not null)
                userWorkoutRecords = userWorkoutRecords.Where(bw => range.IsDateInRange(bw.Date, true));

            if (workoutId.HasValue)
                userWorkoutRecords = userWorkoutRecords.Where(ms => ms.WorkoutId == workoutId);

            return ServiceResult<IQueryable<WorkoutRecord>>.Ok(userWorkoutRecords.AsQueryable());
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<WorkoutRecord>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<WorkoutRecord>>.Fail(FailedToActionStr("workout records", "get", ex));
        }
    }

    public async Task<ServiceResult<WorkoutRecord>> GetUserWorkoutRecordByIdAsync(string userId, long workoutRecordId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutRecordId < 1)
                throw invalidWorkoutRecordIDException;

            var userWorkoutRecordById = await baseRepository.GetByIdAsync(workoutRecordId);
            return ServiceResult<WorkoutRecord>.Ok(userWorkoutRecordById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<WorkoutRecord>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<WorkoutRecord>.Fail(FailedToActionStr("workout record", "get", ex));
        }
    }

    public async Task<ServiceResult> UpdateUserWorkoutRecordAsync(string userId, WorkoutRecord workoutRecord)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (workoutRecord is null)
                throw workoutRecordIsNullException;

            if (workoutRecord.Id < 1)
                throw invalidWorkoutRecordIDException;

            var _workout = await workoutRepository.GetByIdAsync(workoutRecord.WorkoutId) ?? throw WorkoutNotFoundByIDException(workoutRecord.WorkoutId);
            var _workoutRecord = await baseRepository.GetByIdAsync(workoutRecord.Id) ?? throw WorkoutRecordNotFoundByIDException(workoutRecord.Id);

            if (_workoutRecord.UserId != userId)
                throw UserNotHavePermissionException("update", "workout record");

            var _exerciseRecordGroups = _workoutRecord.ExerciseRecordGroups;

            _workoutRecord.Time = workoutRecord.Time;
            _workoutRecord.Date = workoutRecord.Date;
            _workoutRecord.WorkoutId = workoutRecord.WorkoutId;
            _workoutRecord.ExerciseRecordGroups = null!;
            await baseRepository.UpdateAsync(_workoutRecord);
            await UpdateUserFirstWorkoutDate(userId);

            await AddExerciseRecordGroups(userId, workoutRecord.Id, workoutRecord.Date, workoutRecord.ExerciseRecordGroups);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("workout record", "update", ex));
        }
    }

    async Task AddExerciseRecordGroups(string userId, long workoutRecordId, DateTime date, IEnumerable<ExerciseRecordGroup> exerciseRecordGroups)
    {
        foreach (var exerciseRecordGroup in exerciseRecordGroups)
        {
            var exerciseRecords = exerciseRecordGroup.ExerciseRecords;

            exerciseRecordGroup.WorkoutRecordId = workoutRecordId;
            exerciseRecordGroup.ExerciseRecords = null!;
            await exerciseRecordGroupRepository.AddAsync(exerciseRecordGroup);

            foreach (var exerciseRecord in exerciseRecords)
            {
                exerciseRecord.Date = date;
                exerciseRecord.ExerciseRecordGroupId = exerciseRecordGroup.Id;
                exerciseRecord.ExerciseId = exerciseRecordGroup.ExerciseId;
                exerciseRecord.UserId = userId;
                await exerciseRecordRepository.AddAsync(exerciseRecord);
            }
        }
    }

    async Task UpdateUserFirstWorkoutDate(string userId)
    {
        var user = (await userRepository.GetUserByIdAsync(userId))!;
        var userWorkoutRecords = await userRepository.GetUserWorkoutRecordsAsync(userId);

        var firstWorkoutDate = userWorkoutRecords?.MinBy(wr => wr.Date)?.Date;
        user.StartedWorkingOut = firstWorkoutDate;
        await userRepository.UpdateUserAsync(user);
    }
}