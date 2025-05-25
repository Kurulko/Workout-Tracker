namespace WorkoutTracker.Application.Common.Exceptions;

public class UnauthorizedException : Exception, IWorkoutException
{
    public string ErrorCode => "UNAUTHORIZED_ERROR";
    public string? CustomMessage => Message;

    public UnauthorizedException(string message) : base(message)
    {

    }

    public static UnauthorizedException HaveNoPermissionToAction(string action, string entityName)
         => new UnauthorizedException($"User has no permission to {action} this {entityName} entry");

    public static UnauthorizedException UserNotAuthenticated()
         => new UnauthorizedException("User not authenticated");
}