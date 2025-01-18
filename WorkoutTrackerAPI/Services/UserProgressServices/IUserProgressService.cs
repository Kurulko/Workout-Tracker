using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Services.ProgressServices;

public interface IUserProgressService
{
    Task<TotalUserProgress> CalculateUserProgressAsync(string userId);
}
