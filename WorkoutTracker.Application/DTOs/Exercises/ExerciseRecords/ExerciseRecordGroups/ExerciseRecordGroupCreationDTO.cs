using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;

namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecordGroups;

public class ExerciseRecordGroupCreationDTO
{
    public long ExerciseId { get; set; }
    public IEnumerable<ExerciseRecordCreationDTO> ExerciseRecords { get; set; } = null!;
}
