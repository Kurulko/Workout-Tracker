using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class UserDTO
{
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime Registered { get; set; }
    public DateTime? StartedWorkingOut { get; set; }
}
