namespace WorkoutTrackerAPI.Data.DTOs;

public class UserCreationDTO
{
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime? StartedWorkingOut { get; set; }
}
