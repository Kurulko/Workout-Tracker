using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Extensions.Enums;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Progresses;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Models;

namespace WorkoutTracker.Infrastructure.Services.Progresses;

internal class WorkoutProgressService : IWorkoutProgressService
{
    readonly IBodyWeightService bodyWeightService;
    readonly IWorkoutRecordService workoutRecordService;

    readonly IBaseInfoProgressService baseInfoProgressService;
    readonly ITotalCompletedProgressService totalCompletedProgressService;
    readonly IWorkoutWeightLiftedProgressService workoutWeightLiftedProgressService;
    readonly IStrikeDurationProgressService strikeDurationProgressService;
    readonly IWorkoutDurationProgressService workoutDurationProgressService;
    readonly IBodyWeightProgressService bodyWeightProgressService;

    public WorkoutProgressService(
        IBodyWeightService bodyWeightService,
        IWorkoutRecordService workoutRecordService,
        IBaseInfoProgressService baseInfoProgressService,
        ITotalCompletedProgressService totalCompletedProgressService,
        IWorkoutWeightLiftedProgressService workoutWeightLiftedProgressService,
        IStrikeDurationProgressService strikeDurationProgressService,
        IWorkoutDurationProgressService workoutDurationProgressService,
        IBodyWeightProgressService bodyWeightProgressService
    )
    {
        this.bodyWeightService = bodyWeightService;
        this.workoutRecordService = workoutRecordService;

        this.baseInfoProgressService = baseInfoProgressService;
        this.totalCompletedProgressService = totalCompletedProgressService;
        this.workoutWeightLiftedProgressService = workoutWeightLiftedProgressService;
        this.strikeDurationProgressService = strikeDurationProgressService;
        this.workoutDurationProgressService = workoutDurationProgressService;
        this.bodyWeightProgressService = bodyWeightProgressService;
    }

    public async Task<CurrentUserProgress> CalculateCurrentUserProgressAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(userId));

        var workoutRecords = (await workoutRecordService.GetUserWorkoutRecordsAsync(userId, cancellationToken: cancellationToken))?.ToList();

        if (workoutRecords is null || !workoutRecords.Any())
            return new CurrentUserProgress();

        CurrentUserProgress currentUserProgress = new();

        currentUserProgress.CurrentWorkoutStrikeDays = GetCurrentWorkoutStrikeInDays(workoutRecords);

        currentUserProgress.CurrentBodyWeight = (await bodyWeightService.GetCurrentUserBodyWeightAsync(userId, cancellationToken))!.Weight;

        var firstWorkoutDate = workoutRecords.MinBy(wr => wr.Date)!.Date;
        currentUserProgress.FirstWorkoutDate = firstWorkoutDate;
        currentUserProgress.LastWorkoutDate = workoutRecords.MaxBy(wr => wr.Date)!.Date;

        var countOfWorkoutDays = (int)(DateTime.Today - firstWorkoutDate).TotalDays + 1;
        currentUserProgress.CountOfWorkoutDays = countOfWorkoutDays;

        currentUserProgress.WorkoutStatus = WorkoutStatusExtensions.GetWorkoutStatusByDays(countOfWorkoutDays);

        return currentUserProgress;
    }

    int GetCurrentWorkoutStrikeInDays(IEnumerable<WorkoutRecord> workoutRecords)
    {
        var range = GetTotalDateRangeTillToday(workoutRecords);
        var dates = workoutRecords.Select(wr => wr.Date).Distinct();
        var allStrikes = WorkoutStrikeModel.GetAllStrikes(dates, range);

        var lastStrike = allStrikes.Last();
        return lastStrike.IsWorkoutStrike ? lastStrike.Range.CountOfDays : 0;
    }

    public async Task<WorkoutProgress> CalculateWorkoutProgressAsync(string userId, DateTimeRange? range, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(userId));

        var workoutRecordsForMonth = (await workoutRecordService.GetUserWorkoutRecordsAsync(userId, range: range, cancellationToken: cancellationToken))!.ToList();
        var bodyWeightsForMonth = (await bodyWeightService.GetUserBodyWeightsInKilogramsAsync(userId, range: range, cancellationToken: cancellationToken))!.ToList();

        return CalculateWorkoutProgress(workoutRecordsForMonth, bodyWeightsForMonth, range);
    }

    public async Task<WorkoutProgress> CalculateWorkoutProgressForMonthAsync(string userId, YearMonth yearMonth, CancellationToken cancellationToken)
    {
        var rangeForMonth = DateTimeRange.GetRangeRangeByMonth(yearMonth);
        return await CalculateWorkoutProgressAsync(userId, rangeForMonth, cancellationToken);
    }

    public async Task<WorkoutProgress> CalculateWorkoutProgressForYearAsync(string userId, int year, CancellationToken cancellationToken)
    {
        var rangeForYear = DateTimeRange.GetRangeRangeByYear(year);
        return await CalculateWorkoutProgressAsync(userId, rangeForYear, cancellationToken);
    }

    WorkoutProgress CalculateWorkoutProgress(IEnumerable<WorkoutRecord> workoutRecords, IEnumerable<BodyWeight> bodyWeights, DateTimeRange? range = null)
    {
        var totalProgress = new WorkoutProgress();

        if (range is null)
        {
            range = GetTotalDateRangeTillToday(workoutRecords);
        }

        var baseInfoProgress = baseInfoProgressService.CalculateBaseInfoProgress(workoutRecords, range);
        totalProgress.BaseInfoProgress = baseInfoProgress;

        totalProgress.TotalCompletedProgress = totalCompletedProgressService.CalculateTotalCompletedProgress(workoutRecords);
        totalProgress.WorkoutWeightLiftedProgress = workoutWeightLiftedProgressService.CalculateWorkoutWeightLiftedProgress(workoutRecords);
        totalProgress.StrikeDurationProgress = strikeDurationProgressService.CalculateStrikeDurationProgress(baseInfoProgress, range);
        totalProgress.WorkoutDurationProgress = workoutDurationProgressService.CalculateWorkoutDurationProgress(workoutRecords, baseInfoProgress);
        totalProgress.BodyWeightProgress = bodyWeightProgressService.CalculateBodyWeightProgress(bodyWeights);

        return totalProgress;
    }

    DateTimeRange GetTotalDateRangeTillToday(IEnumerable<WorkoutRecord> workoutRecords)
    {
        var firstWorkoutDate = workoutRecords.MinBy(wr => wr.Date)!.Date;
        return new DateTimeRange(firstWorkoutDate, DateTime.Today);
    }
}