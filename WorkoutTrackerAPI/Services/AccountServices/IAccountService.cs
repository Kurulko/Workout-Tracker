using WorkoutTrackerAPI.Data.Account;

namespace WorkoutTrackerAPI.Services.AccountServices;

public interface IAccountService
{
    Task<AuthResult> LoginAsync(LoginModel model);
    Task<AuthResult> RegisterAsync(RegisterModel model);
    Task<TokenModel> GetTokenAsync();
    Task LogoutAsync();
}
