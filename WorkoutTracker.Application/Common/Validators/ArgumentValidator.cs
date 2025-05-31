using Microsoft.AspNetCore.Identity;
using System.Numerics;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.ValueObjects;
using WorkoutTracker.Application.Common.Extensions;

namespace WorkoutTracker.Application.Common.Validators;

public static class ArgumentValidator
{
    #region EntryNullException

    public static void ThrowIfEntryNull<T>(T? entry, string paramName)
    {
        if (entry is null)
            throw new EntryNullException(paramName);
    }

    #endregion

    #region ArgumentNullOrEmptyException

    public static void ThrowIfArgumentNullOrEmpty<T>(T? argument, string paramName)
    {
        if (argument is null)
            throw new ArgumentNullOrEmptyException(paramName);
    }

    #endregion

    #region NotFoundException

    public static void ThrowIfNotFound<T>(T? entry, string paramName)
    {
        if (entry is null)
            throw new NotFoundException(paramName);
    }

    public static void ThrowIfNotFoundById<T>(T? entry, string paramName, object id)
    {
        if (entry is null)
            throw NotFoundException.NotFoundExceptionByID(paramName, id);
    }

    public static void ThrowIfNotFoundByName<T>(T? entry, string paramName, string name)
    {
        if (entry is null)
            throw NotFoundException.NotFoundExceptionByName(paramName, name);
    }

    public static async Task<TOutput> EnsureExistsByIdAsync<TInput, TOutput>(Func<TInput, CancellationToken, Task<TOutput?>> fetchFunc, TInput id, string paramName, CancellationToken cancellationToken = default) 
        where TOutput : class
    {
        var result = await fetchFunc(id, cancellationToken);

        ThrowIfNotFoundById(result, paramName, id!);

        return result!;
    }

    public static async Task<TOutput> EnsureExistsByIdAsync<TInput, TOutput>(Func<TInput, Task<TOutput?>> fetchFunc, TInput id, string paramName) 
        where TOutput : class
    {
        var result = await fetchFunc(id);

        ThrowIfNotFoundById(result, paramName, id!);

        return result!;
    }

    public static async Task<T> EnsureExistsByNameAsync<T>(Func<string, CancellationToken, Task<T?>> fetchFunc, string name, string paramName, CancellationToken cancellationToken = default) where T : class
    {
        var result = await fetchFunc(name, cancellationToken);

        ThrowIfNotFoundByName(result, paramName, name);

        return result!;
    }

    public static async Task<T> EnsureExistsByNameAsync<T>(Func<string, Task<T?>> fetchFunc, string name, string paramName) where T : class
    {
        var result = await fetchFunc(name);

        ThrowIfNotFoundByName(result, paramName, name);

        return result!;
    }

    #endregion

    #region ValidationException

    public static void ThrowIfIdNonZero<TNumber>(TNumber id, string entityName)
        where TNumber : struct, INumber<TNumber>
    {
        if (id != TNumber.Zero)
            throw new ValidationException($"{entityName} ID must not be set when adding a new entry.");
    }

    public static void ThrowIfDateInFuture(DateTime date, string propertyName)
    {
        if (date > DateTime.UtcNow)
            throw new ValidationException($"{propertyName} cannot be in the future.");
    }

    public static void ThrowIfRangeInFuture(DateTimeRange range, string propertyName)
    {
        if (range.LastDate > DateTime.UtcNow)
            throw new ValidationException($"{propertyName} cannot be in the future.");
    }

    public static void ThrowIfValueNegative<TNumber>(TNumber value, string propertyName)
        where TNumber : struct, INumber<TNumber>
    {
        if (value < TNumber.Zero)
            throw new ValidationException($"{propertyName} must not be negative.");
    }

    public static void ThrowIfModelWeightNegative(ModelWeight weight, string propertyName)
    {
        ThrowIfValueNegative(weight.Weight, propertyName);
    }

    public static void ThrowIfModelSizeNegative(ModelSize size, string propertyName)
    {
        ThrowIfValueNegative(size.Size, propertyName);
    }

    public static void ThrowIfOutOfRange<TNumber>(Range range, TNumber value, string propertyName)
        where TNumber : struct, INumber<TNumber>
    {
        TNumber start = TNumber.CreateChecked(range.Start.Value);
        TNumber end = TNumber.CreateChecked(range.End.Value);

        if (value < start || value > end)
            throw new ValidationException($"{propertyName} must be between {start} and {end}.");
    }


    public static async Task EnsureNonExistsByIdAsync<TInput, TOutput>(Func<TInput, Task<TOutput?>> fetchFunc, TInput id)
         where TOutput : class
    {
        var result = await fetchFunc(id);

        if (result is not null)
            throw new ValidationException($"An entity with ID '{id}' already exists.");
    }

    public static async Task EnsureNonExistsByNameAsync<T>(Func<string, Task<T?>> fetchFunc, string name)
         where T : class
    {
        var result = await fetchFunc(name);

        if (result is not null)
            throw new ValidationException($"An entity with '{name}' name already exists.");
    }

    public static async Task EnsureNonExistsByNameAsync<T>(Func<string, CancellationToken, Task<T?>> fetchFunc, string name, CancellationToken cancellationToken = default)
         where T : class
    {
        var result = await fetchFunc(name, cancellationToken);

        if (result is not null)
            throw new ValidationException($"An entity with '{name}' name already exists.");
    }

    public static async Task EnsureNonExistsByEmailAsync<T>(Func<string, Task<T?>> fetchFunc, string? email)
         where T : class
    {
        if (!string.IsNullOrEmpty(email))
        {
            var result = await fetchFunc(email);

            if (result is not null)
                throw new ValidationException($"An entity with '{email}' email already exists.");
        }
    }

    public static async Task EnsureNameUniqueAsync<T>(Func<string, Task<T?>> fetchFunc, string name, long id, string propertyName)
         where T : class, IDbModel
    {
        var result = await fetchFunc(name);

        if (result != null && result.Id != id)
            throw new ValidationException($"{propertyName} must be unique."); ;
    }

    public static async Task EnsureNameUniqueAsync<T>(Func<string, CancellationToken, Task<T?>> fetchFunc, string name, long id, string propertyName, CancellationToken cancellationToken = default)
         where T : class, IDbModel
    {
        var result = await fetchFunc(name, cancellationToken);

        if (result != null && result.Id != id)
            throw new ValidationException($"{propertyName} must be unique."); ;
    }

    public static void ThrowIfCollectionNullOrEmpty<T>(IEnumerable<T> collection, string propertyName)
    {
        if (collection is null || collection.Count() == 0)
            throw new ValidationException($"{propertyName} collection cannot be null or empty.");
    }

    public static void ThrowIfNotSucceeded(string action, string entity, IdentityResult result)
    {
        if (!result.Succeeded)
            throw new ValidationException(FailedToAction(action, entity, result));
    }

    public static void ThrowIfPasswordsMismatch(string password, string passwordConfirm)
    {
        if (password != passwordConfirm)
            throw new ValidationException("Passwords mismatch.");
    }


    #endregion

    #region UnauthorizedException


    #endregion

    #region InvalidIDException

    public static void ThrowIfIdNonPositive<TNumber>(TNumber id, string entityName, bool showIdInException = false)
        where TNumber : struct, INumber<TNumber>
    {
        if (id < TNumber.One)
            throw showIdInException ? new InvalidIDException(entityName, id) : new InvalidIDException(entityName);
    }

    public static void ThrowIfIdNullOrEmpty(string? id, string entityName)
    {
        if (string.IsNullOrEmpty(id))
            throw new InvalidIDException(entityName);
    }

    #endregion

    static string FailedToAction(string action, string entity, IdentityResult result)
        => $"Failed to {action} {entity}: {result.IdentityErrorsToString()}";
}
