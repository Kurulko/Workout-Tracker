using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecordGroups;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Workouts.WorkoutRecords;

public class WorkoutRecordDTO
{
    public long Id { get; set; }
    public TimeSpanModel Time { get; set; }
    public DateTime Date { get; set; }
    public ModelWeight Weight { get; set; }

    public long WorkoutId { get; set; }
    public string WorkoutName { get; set; } = null!;

    public IEnumerable<ExerciseRecordGroupDTO> ExerciseRecordGroups { get; set; } = null!;
    public IEnumerable<ExerciseDTO> Exercises { get; set; } = null!;
}
