using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Progresses;

public class CurrentUserProgressDTO
{
    public WorkoutStatus WorkoutStatus { get; set; }

    public DateTime? FirstWorkoutDate { get; set; }
    public DateTime? LastWorkoutDate { get; set; }
    public int CountOfWorkoutDays { get; set; }

    public int CurrentWorkoutStrikeDays { get; set; }
    public ModelWeight CurrentBodyWeight { get; set; }
}
