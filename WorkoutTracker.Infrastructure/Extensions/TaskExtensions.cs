using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Exceptions;

namespace WorkoutTracker.Infrastructure.Extensions;

public static class TaskExtensions
{
    public static async Task<T> LogExceptionsAsync<T>(
        this Task<T> task,
        ILogger logger,
        string contextMessage
    )
    {
        try
        {
            var result = await task;
            return result;
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            logger.LogWarning(ex, contextMessage);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, contextMessage);
            throw;
        }
    }
    
    public static async Task LogExceptionsAsync(
        this Task task,
        ILogger logger,
        string contextMessage
    )
    {
        try
        {
            await task;
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            logger.LogWarning(ex, contextMessage);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, contextMessage);
            throw;
        }
    }
}
