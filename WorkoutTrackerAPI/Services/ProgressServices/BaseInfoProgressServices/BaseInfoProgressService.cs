using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.ProgressServices.BaseInfoProgressServices;

namespace WorkoutTrackerAPI.Services.WorkoutProgressServices;

public class BaseInfoProgressService : IBaseInfoProgressService
{
    public BaseInfoProgress CalculateBaseInfoProgress(IEnumerable<WorkoutRecord> workoutRecords, DateTimeRange range)
    {
        if (!workoutRecords.Any())
            return new BaseInfoProgress();

        BaseInfoProgress baseInfoProgress = new();

        var totalWorkouts = workoutRecords.Count();
        baseInfoProgress.TotalWorkouts = totalWorkouts;

        baseInfoProgress.TotalDuration = workoutRecords.GetTotalTime();
        baseInfoProgress.CountOfExercisesUsed = GetCountOfExercisesUsed(workoutRecords);
        baseInfoProgress.WorkoutDates = GetWorkoutDates(workoutRecords);

        baseInfoProgress.FrequencyPerWeek = GetFrequencyPerWeek(range.CountOfDays, totalWorkouts);

        return baseInfoProgress;
    }

    double GetFrequencyPerWeek(int countOfDays, int totalWorkouts)
    {
        const int daysInWeek = 7;

        double countOfWeeks = (double)countOfDays / daysInWeek;
        var frequencyPerWeek = Math.Round(totalWorkouts / countOfWeeks, 1);
        return Math.Min(frequencyPerWeek, daysInWeek);
    }

    int GetCountOfExercisesUsed(IEnumerable<WorkoutRecord> workoutRecords)
    {
        var exercises = workoutRecords.SelectMany(wr => wr.ExerciseRecordGroups.GetExercises()).Distinct();
        return exercises.Count();
    }

    IEnumerable<DateTime> GetWorkoutDates(IEnumerable<WorkoutRecord> workoutRecords)
    {
        return workoutRecords.Select(wr => wr.Date).Distinct();
    }
}
