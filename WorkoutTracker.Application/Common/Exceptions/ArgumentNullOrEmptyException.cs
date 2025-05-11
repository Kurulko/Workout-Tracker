namespace WorkoutTracker.Application.Common.Exceptions;

public class ArgumentNullOrEmptyException : ArgumentException, IWorkoutException
{
    public string ErrorCode => "NULL_OR_EMPTY_ERROR";
    public string? CustomMessage => Message;

    public ArgumentNullOrEmptyException(string paramName)
                : base($"{paramName} cannot be null or empty.", paramName)
    {

    }
}
