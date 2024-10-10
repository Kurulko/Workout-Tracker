namespace WorkoutTrackerAPI.Exceptions;

public class ArgumentNullOrEmptyException : ArgumentException
{
    public ArgumentNullOrEmptyException(string paramName)
            : base($"{paramName} cannot be null or empty.", paramName)
    {

    }
}