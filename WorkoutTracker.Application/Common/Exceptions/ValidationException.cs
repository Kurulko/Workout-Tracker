namespace WorkoutTracker.Application.Common.Exceptions;

public class ValidationException : ArgumentException, IWorkoutException
{
    public string ErrorCode => "VALIDATION_ERROR";
    public string? CustomMessage => Message;

    public ValidationException(string message)
            : base(message)
    {

    }
}