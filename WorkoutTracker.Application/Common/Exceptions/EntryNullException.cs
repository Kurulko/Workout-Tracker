namespace WorkoutTracker.Application.Common.Exceptions;

public class EntryNullException : ArgumentNullException, IWorkoutException
{
    public string ErrorCode => "NULL_ERROR";
    public string? CustomMessage => Message;

    public EntryNullException(string name)
            : base($"{name} entry cannot be null.", name)
    {

    }
}
