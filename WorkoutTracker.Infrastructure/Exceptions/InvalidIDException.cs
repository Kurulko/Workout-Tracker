using WorkoutTracker.Application.Common.Exceptions;

namespace WorkoutTracker.Infrastructure.Exceptions;

public class InvalidIDException : ArgumentException, IWorkoutException
{
    public string ErrorCode => "INVALID_ID";
    public string? CustomMessage => Message;

    public InvalidIDException(string paramName)
            : base($"Invalid {paramName} ID.", paramName)
    {

    }
}