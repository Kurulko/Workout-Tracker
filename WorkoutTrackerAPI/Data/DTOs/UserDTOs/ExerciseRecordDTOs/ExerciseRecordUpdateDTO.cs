using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs.UserDTOs.ExerciseRecordDTOs;

public class ExerciseRecordUpdateDTO
{
    public long Id { get; set; }

    [DateNotInFuture]
    public DateTime Date { get; set; }

    public ModelWeight? Weight { get; set; }
    public TimeSpanModel? Time { get; set; }

    [PositiveNumber]
    public int? Reps { get; set; }

    public long ExerciseId { get; set; }
}
