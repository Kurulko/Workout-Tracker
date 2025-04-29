using System;
using System.Linq.Dynamic.Core;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.BodyWeightServices;
using WorkoutTrackerAPI.Services.ProgressServices.BaseInfoProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.BodyWeightProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.StrikeDurationProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.TotalCompletedProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.WorkoutDurationProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.WorkoutWeightLiftedProgressServices;
using WorkoutTrackerAPI.Services.WorkoutRecordServices;

namespace WorkoutTrackerAPI.Services.ProgressServices;

public class WorkoutProgressService : IWorkoutProgressService
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

    public async Task<CurrentUserProgress> CalculateCurrentUserProgressAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullOrEmptyException("User ID");

        var workoutRecords = (await workoutRecordService.GetUserWorkoutRecordsAsync(userId)).Model?.ToList();

        if (workoutRecords is null || !workoutRecords.Any())
            return new CurrentUserProgress();

        CurrentUserProgress currentUserProgress = new();

        currentUserProgress.CurrentWorkoutStrikeDays = GetCurrentWorkoutStrikeInDays(workoutRecords);

        currentUserProgress.CurrentBodyWeight = (await bodyWeightService.GetCurrentUserBodyWeightAsync(userId)).Model!.Weight;

        var firstWorkoutDate = workoutRecords.MinBy(wr => wr.Date)!.Date;
        currentUserProgress.FirstWorkoutDate = firstWorkoutDate;
        currentUserProgress.LastWorkoutDate = workoutRecords.MaxBy(wr => wr.Date)!.Date;

        var countOfWorkoutDays = (int)(DateTime.Today - firstWorkoutDate).TotalDays + 1;
        currentUserProgress.CountOfWorkoutDays = countOfWorkoutDays;

        currentUserProgress.WorkoutStatus = WorkoutStatusExtentions.GetWorkoutStatusByDays(countOfWorkoutDays);

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

    public async Task<WorkoutProgress> CalculateWorkoutProgressAsync(string userId, DateTimeRange? range)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullOrEmptyException("User ID");

        var workoutRecordsForMonth = (await workoutRecordService.GetUserWorkoutRecordsAsync(userId, range: range)).Model!.ToList();
        var bodyWeightsForMonth = (await bodyWeightService.GetUserBodyWeightsInKilogramsAsync(userId, range: range)).Model!.ToList();

        return CalculateWorkoutProgress(workoutRecordsForMonth, bodyWeightsForMonth, range);
    }

    public async Task<WorkoutProgress> CalculateWorkoutProgressForMonthAsync(string userId, YearMonth yearMonth)
    {
        var rangeForMonth = DateTimeRange.GetRangeRangeByMonth(yearMonth);
        return await CalculateWorkoutProgressAsync(userId, rangeForMonth);
    }

    public async Task<WorkoutProgress> CalculateWorkoutProgressForYearAsync(string userId, int year)
    {
        var rangeForYear = DateTimeRange.GetRangeRangeByYear(year);
        return await CalculateWorkoutProgressAsync(userId, rangeForYear);
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