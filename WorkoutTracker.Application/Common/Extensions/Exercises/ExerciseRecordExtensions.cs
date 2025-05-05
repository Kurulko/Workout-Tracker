using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;

namespace WorkoutTracker.Application.Common.Extensions.Exercises;

public static class ExerciseRecordExtensions
{
    public static ExerciseRecord ToExerciseRecord(this ExerciseSet exerciseSet, DateTime date, long exerciseRecordGroupId)
    {
        ExerciseRecord exerciseRecord = new()
        {
            Date = date,
            ExerciseId = exerciseSet.ExerciseId,
            ExerciseRecordGroupId = exerciseRecordGroupId,
            UserId = exerciseSet.UserId,
            Reps = exerciseSet.Reps,
            Weight = exerciseSet.Weight,
            Time = exerciseSet.Time,
        };

        return exerciseRecord;
    }
}
