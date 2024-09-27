using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;

namespace WorkoutTrackerAPI.Services;

public class BaseService<TModel> where TModel : class
{
    protected readonly ArgumentNullOrEmptyException userIdIsNullOrEmptyException = new ("User ID");
    protected readonly NotFoundException userNotFoundException = new NotFoundException(nameof(User));

    protected string FailedToAction(string modelName, string action, string? message = null)
    {
        string result = $"Failed to {action} {modelName}";

        if (!string.IsNullOrEmpty(message))
            result += $": {message}";

        return result;
    }

    protected string InvalidEntryIDWhileAddingStr(string entryName, string modelName)
        => $"{entryName} ID must not be set when adding a new {modelName}.";
    protected ArgumentException InvalidEntryIDWhileAdding(string entryName, string modelName)
        => new ArgumentException(InvalidEntryIDWhileAddingStr(entryName, modelName));

    protected string UserNotHavePermissionStr(string action, string entryName)
        => $"User does not have permission to {action} this {entryName} entry";
    protected UnauthorizedAccessException UserNotHavePermission(string action, string entryName)
        => new UnauthorizedAccessException(UserNotHavePermissionStr(action, entryName));
}
