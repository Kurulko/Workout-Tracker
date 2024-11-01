using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data.Models.WorkoutModels;

public class Equipment : WorkoutModel
{
    public byte[]? Image { get; set; }

    public string? OwnedByUserId { get; set; }
    public User? OwnedByUser { get; set; }

    public IEnumerable<Exercise>? Exercises { get; set; } = null!;
}
