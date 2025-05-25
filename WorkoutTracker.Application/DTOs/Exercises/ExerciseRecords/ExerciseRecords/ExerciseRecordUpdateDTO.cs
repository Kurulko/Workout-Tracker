using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;

public class ExerciseRecordUpdateDTO
{
    public long Id { get; set; }
    public DateTime Date { get; set; }

    public int? Reps { get; set; }
    public ModelWeight? Weight { get; set; }
    public TimeSpanModel? Time { get; set; }

    public long ExerciseId { get; set; }
}
