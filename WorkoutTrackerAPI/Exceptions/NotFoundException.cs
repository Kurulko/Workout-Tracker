namespace WorkoutTrackerAPI.Exceptions;

public class NotFoundException : Exception
{
    public string ParamName { get; init; }
    public NotFoundException(string paramName) : base($"{paramName} not found.") 
        => ParamName = paramName;
}