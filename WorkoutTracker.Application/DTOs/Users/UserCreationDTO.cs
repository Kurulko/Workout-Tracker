namespace WorkoutTracker.Application.DTOs.Users;

public class UserCreationDTO
{
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime Registered { get; set; }
    public DateTime? StartedWorkingOut { get; set; }
}
