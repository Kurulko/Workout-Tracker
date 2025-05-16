using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;

public class ExerciseRecordCreationDTO
{
    public int? Reps { get; set; }
    public ModelWeight? Weight { get; set; }
    public TimeSpanModel? Time { get; set; }

    public long ExerciseId { get; set; }
}
