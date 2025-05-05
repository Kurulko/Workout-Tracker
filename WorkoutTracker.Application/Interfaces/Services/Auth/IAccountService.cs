using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.DTOs.Account;

namespace WorkoutTracker.Application.Interfaces.Services.Auth;

public interface IAccountService
{
    Task<AuthResult> LoginAsync(LoginModel model);
    Task<AuthResult> RegisterAsync(RegisterModel model);
    Task<TokenModel> GetTokenAsync(string userId);
    Task LogoutAsync();
}
