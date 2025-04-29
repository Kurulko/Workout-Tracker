using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerAPI.Data.Models.WorkoutModels;

public class Equipment : WorkoutModel
{
    public string? Image { get; set; }

    public string? OwnedByUserId { get; set; }
    public User? OwnedByUser { get; set; }

    public IEnumerable<Exercise>? Exercises { get; set; }
}
