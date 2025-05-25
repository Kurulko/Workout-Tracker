using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Interfaces.Services;

namespace WorkoutTracker.Infrastructure.Services.Base;

internal class BaseService<TService, TModel> 
    where TModel : class
    where TService : IBaseService
{
    protected readonly ILogger<TService> _logger;
    public BaseService(ILogger<TService> logger)
        => _logger = logger;

    protected string FailedToActionStr(string modelName, string action, string? message = null)
    {
        string result = $"Failed to {action} {modelName}";

        if (!string.IsNullOrEmpty(message))
            result += $": {message}";

        return result + ".";
    }

    protected string FailedToActionStr(string modelName, string action, Exception ex)
        => FailedToActionStr(modelName, action, ex.InnerException?.Message ?? ex.Message);


    protected string FailedToActionForUserStr(string modelName, string action, string userId, string? message = null)
    {
        string result = $"Failed to {action} {modelName} for user '{userId}'";

        if (!string.IsNullOrEmpty(message))
            result += $": {message}";

        return result + ".";
    }

    protected string FailedToActionForUserStr(string modelName, string action, string userId, Exception ex)
        => FailedToActionForUserStr(modelName, action, userId, ex.InnerException?.Message ?? ex.Message);
}
