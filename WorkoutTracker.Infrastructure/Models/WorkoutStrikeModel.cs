using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Extensions;

namespace WorkoutTracker.Infrastructure.Models;

internal record WorkoutStrikeModel
{
    public WorkoutStrikeModel(DateTimeRange range, bool isWorkoutStrike)
    {
        Range = range;
        IsWorkoutStrike = isWorkoutStrike;
        IsRestStrike = !isWorkoutStrike;
    }

    public DateTimeRange Range { get; init; }
    public bool IsWorkoutStrike { get; init; }
    public bool IsRestStrike { get; init; }


    public static IEnumerable<WorkoutStrikeModel> GetAllStrikes(IEnumerable<DateTime> workoutDates, DateTimeRange range)
    {
        var allDates = range.GetDatesBetween().OrderBy(d => d).ToList();

        var allWorkoutStrikes = new List<WorkoutStrikeModel>();

        bool? isWorkout = null;
        DateTime? firstStrikeDate = null;
        DateTime? lastStrikeDate = null;

        for (int i = 0; i < allDates.Count; i++)
        {
            var date = allDates[i];
            var isCurrentWorkout = workoutDates.Contains(date);

            if (firstStrikeDate == null) //if it's the first item
            {
                isWorkout = isCurrentWorkout;
                firstStrikeDate = date;
                lastStrikeDate = date;
            }
            else if (isWorkout == isCurrentWorkout) //if it's a strike
            {
                lastStrikeDate = date;
            }
            else //if it's not a strike
            {
                if (firstStrikeDate!.Value != lastStrikeDate!.Value)
                {
                    var currentRange = new DateTimeRange(firstStrikeDate!.Value, lastStrikeDate!.Value);
                    var workoutStrikeModel = new WorkoutStrikeModel(currentRange, isWorkout!.Value);
                    allWorkoutStrikes.Add(workoutStrikeModel);
                }

                firstStrikeDate = date;
                lastStrikeDate = date;
                isWorkout = isCurrentWorkout;
            }

            if (i == allDates.Count - 1) //if it's the last item
            {
                if (firstStrikeDate!.Value != lastStrikeDate!.Value)
                {
                    var currentRange = new DateTimeRange(firstStrikeDate!.Value, lastStrikeDate!.Value);
                    var workoutStrikeModel = new WorkoutStrikeModel(currentRange, isWorkout!.Value);
                    allWorkoutStrikes.Add(workoutStrikeModel);
                }
            }
        }

        return allWorkoutStrikes;
    }
}