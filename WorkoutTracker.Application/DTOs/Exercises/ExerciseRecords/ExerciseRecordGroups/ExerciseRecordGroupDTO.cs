using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecordGroups;
public class ExerciseRecordGroupDTO
{
    public long Id { get; set; }

    public ModelWeight Weight { get; set; }
    public int Sets { get; set; }

    public long ExerciseId { get; set; }
    public string? ExerciseName { get; set; }
    public ExerciseType? ExerciseType { get; set; }

    public IEnumerable<ExerciseRecordDTO> ExerciseRecords { get; set; } = null!;
}
