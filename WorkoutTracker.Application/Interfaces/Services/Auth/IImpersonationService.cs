using WorkoutTracker.Application.DTOs.Account;

namespace WorkoutTracker.Application.Interfaces.Services.Auth;

public interface IImpersonationService
{
    Task<TokenModel> ImpersonateAsync(string userId);
    Task<TokenModel> RevertAsync();
    bool IsImpersonating();
}
