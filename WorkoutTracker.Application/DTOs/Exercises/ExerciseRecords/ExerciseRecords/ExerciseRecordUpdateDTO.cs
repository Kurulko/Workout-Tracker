using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.ValidationAttributes;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;

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
