namespace WorkoutTracker.Application.Common.Exceptions;

public interface IWorkoutException
{
    string ErrorCode { get; }
    string? CustomMessage { get; }
}
