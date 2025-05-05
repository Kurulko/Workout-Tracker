using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Workouts.Workouts;

public class WorkoutDetailsDTO
{
    public WorkoutDTO Workout { get; set; } = null!;

    public int TotalWorkouts { get; set; }
    public ModelWeight TotalWeight { get; set; }
    public TimeSpanModel TotalDuration { get; set; }
    public TimeSpanModel AverageWorkoutDuration { get; set; }

    public int CountOfDaysSinceFirstWorkout { get; set; }
    public IEnumerable<DateTime>? Dates { get; set; }

    public IEnumerable<MuscleDTO> Muscles { get; set; } = null!;
    public IEnumerable<EquipmentDTO> Equipments { get; set; } = null!;
}
