namespace WorkoutTrackerAPI.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string paramName) 
        : base($"{paramName} not found.") 
    {
    
    }
}