using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecordGroups;

namespace WorkoutTracker.Application.DTOs.Workouts.WorkoutRecords;

public class WorkoutRecordCreationDTO
{
    public TimeSpanModel Time { get; set; }
    public DateTime Date { get; set; }
    public long WorkoutId { get; set; }

    public IEnumerable<ExerciseRecordGroupCreationDTO> ExerciseRecordGroups { get; set; } = null!;
}
