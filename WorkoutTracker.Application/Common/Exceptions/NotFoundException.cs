namespace WorkoutTracker.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public string ParamName { get; init; }
    public NotFoundException(string paramName) : base($"{paramName} not found.") 
        => ParamName = paramName;
    public NotFoundException(string paramName, string message) : base(message) 
        => ParamName = paramName;

    public static NotFoundException NotFoundExceptionByID(string paramName, object id)
        => new NotFoundException(paramName, $"{paramName} (ID '{id}') not found.");
    public static NotFoundException NotFoundExceptionByName(string paramName, string name)
        => new NotFoundException(paramName, $"{paramName} (Name '{name}') not found.");
}