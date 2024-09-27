namespace WorkoutTrackerAPI.Exceptions;

public class InvalidIDException : ArgumentException
{
    public InvalidIDException(string paramName)
            : base($"Invalid {paramName} ID.")
    {

    }
}