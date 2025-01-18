using WorkoutTrackerAPI.Data.Account;

namespace WorkoutTrackerAPI.Services.ImpersonationServices;

public interface IImpersonationService
{
    Task<TokenModel> ImpersonateAsync(string userId);
    Task<TokenModel> RevertAsync();
    bool IsImpersonating();
}
