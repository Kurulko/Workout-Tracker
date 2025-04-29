using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Extentions;

public static class ExerciseSetExtentions
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
