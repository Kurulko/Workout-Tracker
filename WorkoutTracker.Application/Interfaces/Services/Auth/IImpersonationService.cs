using WorkoutTracker.Application.DTOs.Account;

namespace WorkoutTracker.Application.Interfaces.Services.Auth;

public interface IImpersonationService : IBaseService
{
    Task<TokenModel> ImpersonateAsync(string userId);
    Task<TokenModel> RevertAsync();
    bool IsImpersonating();
}
