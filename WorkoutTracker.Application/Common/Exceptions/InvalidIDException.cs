namespace WorkoutTracker.Application.Common.Exceptions;

public class InvalidIDException : ArgumentException, IWorkoutException
{
    public string ErrorCode => "INVALID_ID";
    public string? CustomMessage => Message;

    public InvalidIDException(string paramName)
            : base($"Invalid {paramName} ID.", paramName)
    {

    }

    public InvalidIDException(string paramName, object id)
            : base($"Invalid {paramName} ID ('{id}').", paramName)
    {

    }
}