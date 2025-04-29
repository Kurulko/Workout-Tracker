using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Extentions;

public static class ExerciseRecordExtentions
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
