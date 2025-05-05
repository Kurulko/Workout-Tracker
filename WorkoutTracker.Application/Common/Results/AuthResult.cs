using WorkoutTracker.Application.DTOs.Account;

namespace WorkoutTracker.Application.Common.Results;

public class AuthResult
{
    /// <summary>
    /// TRUE if the login attempt is successful, FALSE otherwise.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Login attempt result message
    /// </summary>
    public string Message { get; } = null!;

    /// <summary>
    /// The JWT token if the login attempt is successful, or NULL if not
    /// </summary>
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
