namespace WorkoutTrackerAPI.Services.ImpersonationServices;

public interface IImpersonationService
{
    Task ImpersonateAsync(string userId);
    Task RevertAsync();
    bool IsImpersonating();
}
