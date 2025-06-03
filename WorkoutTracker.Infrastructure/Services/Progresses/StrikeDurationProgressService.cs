using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Services.Progresses;
using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Infrastructure.Models;

namespace WorkoutTracker.Infrastructure.Services.Progresses;

internal class StrikeDurationProgressService : IStrikeDurationProgressService
{
    public StrikeDurationProgress CalculateStrikeDurationProgress(BaseInfoProgress baseInfoProgress, DateTimeRange range)
    {
        var workoutDates = baseInfoProgress.WorkoutDates;

        if (workoutDates is null || !workoutDates.Any())
            return new StrikeDurationProgress();

        StrikeDurationProgress strikeDurationProgress = new();

        var allStrikes = WorkoutStrikeModel.GetAllStrikes(workoutDates, range);

        if (!allStrikes.Any())
            return strikeDurationProgress;

        strikeDurationProgress.AverageWorkoutStrikeDays = GetAverageWorkoutStrikeInDays(allStrikes);
        strikeDurationProgress.MaxWorkoutStrikeDays = GetMaxWorkoutStrikeInDays(allStrikes);
        strikeDurationProgress.MaxRestStrikeDays = GetMaxRestStrikeInDays(allStrikes);

        return strikeDurationProgress;
    }

    static double GetAverageWorkoutStrikeInDays(IEnumerable<WorkoutStrikeModel> strikes)
    {
        var workoutStrikes = strikes.Where(s => s.IsWorkoutStrike);
        var averageStrike = workoutStrikes.Average(ws => ws.Range.CountOfDays);
        return Math.Round(averageStrike);
    }

    static int GetMaxWorkoutStrikeInDays(IEnumerable<WorkoutStrikeModel> strikes)
    {
        var workoutStrikes = strikes.Where(s => s.IsWorkoutStrike);
        var maxWorkoutStrike = workoutStrikes.MaxBy(s => s.Range.CountOfDays);
        return maxWorkoutStrike?.Range.CountOfDays ?? 0;
    }

    static int GetMaxRestStrikeInDays(IEnumerable<WorkoutStrikeModel> strikes)
    {
        var restStrikes = strikes.Where(s => s.IsRestStrike);
        var maxRestStrike = restStrikes.MaxBy(s => s.Range.CountOfDays);
        return maxRestStrike?.Range.CountOfDays ?? 0;
    }
}