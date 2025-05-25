using WorkoutTracker.Application.DTOs.Account;

namespace WorkoutTracker.Application.Common.Results;

public class AuthResult
{
    public bool Success { get; }
    public string Message { get; } = null!;
    public TokenModel? Token { get; }

    private AuthResult(bool success, string message, TokenModel? token = null)
    {
        Success = success;
        Message = message;
        Token = token;
    }

    public static AuthResult Ok(string message, TokenModel token)
        => new AuthResult(true, message, token);

    public static AuthResult Fail(string message)
        => new AuthResult(false, message);
}
