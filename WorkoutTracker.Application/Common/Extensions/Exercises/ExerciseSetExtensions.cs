using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;

namespace WorkoutTracker.Application.Common.Extensions.Exercises;

public static class ExerciseSetExtensions
{
    public static ExerciseRecordGroup ToExerciseRecordGroup(this ExerciseSetGroup exerciseSetGroup, long workoutRecordId)
    {
        ExerciseRecordGroup exerciseRecordGroup = new()
        {
            ExerciseId = exerciseSetGroup.ExerciseId,
            WorkoutRecordId = workoutRecordId,
        };

        return exerciseRecordGroup;
    }
}
