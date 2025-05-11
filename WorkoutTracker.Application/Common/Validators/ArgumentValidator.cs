using WorkoutTracker.Application.Common.Exceptions;

namespace WorkoutTracker.Application.Common.Validators;

public static class ArgumentValidator
{
    public static void ThrowIfNull<T>(T? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }

    public static void ThrowIfIdNotZero(int id, string entityName, string? friendlyName = null)
    {
        if (id != 0)
            throw new InvalidOperationException(
                $"Invalid {friendlyName ?? entityName} entry. New {entityName} must not have an ID set.");
    }

    public static async Task<T> EnsureExistsAsync<T>(Func<Task<T?>> fetchFunc, string errorMessage) where T : class
    {
        var result = await fetchFunc();

        if (result is null)
            throw new NotFoundException(errorMessage);

        return result;
    }

    public static void ThrowIfNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} is required.", paramName);
    }
}
