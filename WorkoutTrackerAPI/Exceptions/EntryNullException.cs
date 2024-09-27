namespace WorkoutTrackerAPI.Exceptions;

public class EntryNullException : ArgumentException
{
    public EntryNullException(string name)
            : base($"{name} entry cannot be null.")
    {

    }
}
