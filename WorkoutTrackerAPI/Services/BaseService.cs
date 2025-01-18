using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories.UserRepositories;

namespace WorkoutTrackerAPI.Services;

public class BaseService<TModel> where TModel : class
{
    protected readonly ArgumentNullOrEmptyException userIdIsNullOrEmptyException = new ("User ID");
    
    protected NotFoundException UserNotFoundByIDException(string userId) 
        => NotFoundException.NotFoundExceptionByID(nameof(User), userId);
    protected NotFoundException UserNotFoundByNameException(string userName) 
        => NotFoundException.NotFoundExceptionByName(nameof(User), userName);

    protected string FailedToActionStr(string modelName, string action, string? message = null)
    {
        string result = $"Failed to {action} {modelName}";

        if (!string.IsNullOrEmpty(message))
            result += $": {message}";

        return result + ".";
    }

    protected string FailedToActionStr(string modelName, string action, Exception ex)
        => FailedToActionStr(modelName, action, ex.InnerException?.Message ?? ex.Message);

    protected string InvalidEntryIDWhileAddingStr(string entryName, string modelName)
        => $"{entryName} ID must not be set when adding a new {modelName}.";
    protected ArgumentException InvalidEntryIDWhileAddingException(string entryName, string modelName)
        => new (InvalidEntryIDWhileAddingStr(entryName, modelName));

    protected string UserNotHavePermissionStr(string action, string entryName)
        => $"User does not have permission to {action} this {entryName} entry";
    protected UnauthorizedAccessException UserNotHavePermissionException(string action, string entryName)
        => new (UserNotHavePermissionStr(action, entryName));

    protected async Task CheckUserIdAsync(UserRepository userRepository, string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        bool userExists = await userRepository.UserExistsAsync(userId);
        if (!userExists)
            throw UserNotFoundByIDException(userId);
    }
}
