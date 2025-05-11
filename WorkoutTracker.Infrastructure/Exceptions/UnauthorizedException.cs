using WorkoutTracker.Application.Common.Exceptions;

namespace WorkoutTracker.Infrastructure.Exceptions;

public class UnauthorizedException : Exception, IWorkoutException
{
    public string ErrorCode => "UNAUTHORIZED_ERROR";
    public string? CustomMessage => Message;

    public UnauthorizedException(string message) : base(message)
    {

    }
}