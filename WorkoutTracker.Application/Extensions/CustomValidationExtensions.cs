using FluentValidation;
using System.Linq.Expressions;
using System.Numerics;
using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.Extensions;

internal static class CustomValidationExtensions
{
    const string defaultRequiredMessage = "{PropertyName} is required.";

    public static IRuleBuilder<T, string?> ValidPassword<T>(this IRuleBuilder<T, string?> ruleBuilder, string? message = null, bool isRequired = true, int minLength = 8, bool withOneUpperCaseLetter = true, bool withOneNumber = true)
    {
        if (isRequired)
            ruleBuilder = ruleBuilder.NotEmptyOrNull(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder.MinimumLength(minLength).WithMessage("Password must be at least {MinLength} characters long.");

        if (withOneUpperCaseLetter)
            ruleBuilder = ruleBuilder.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.");

        if (withOneNumber)
            ruleBuilder = ruleBuilder.Matches("[0-9]").WithMessage("Password must contain at least one number.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, string?> ValidName<T>(this IRuleBuilder<T, string?> ruleBuilder, string? message = null, bool isRequired = true, int minLength = 3, int maxLength = 50)
    {
        if (isRequired)
            ruleBuilder = ruleBuilder.NotEmptyOrNull(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder.MinimumLength(minLength).WithMessage("Name must be at least {MinLength} characters long.");
        ruleBuilder = ruleBuilder.MaximumLength(maxLength).WithMessage("Name must be at most {MaxLength} characters long.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, string?> ValidDescription<T>(this IRuleBuilder<T, string?> ruleBuilder, string? message = null, bool isRequired = true, int minLength = 10, int maxLength = 500)
    {
        if (isRequired)
            ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder.NotEmpty().WithMessage("Description should not be empty.");

        ruleBuilder = ruleBuilder.MinimumLength(minLength).WithMessage("Description should not be shorter than {MinLength} characters.");
        ruleBuilder = ruleBuilder.MaximumLength(maxLength).WithMessage("Description should not be longer than {MaxLength} characters.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, TNumber?> ValidID<T, TNumber>(this IRuleBuilder<T, TNumber?> ruleBuilder, string? message = null, bool isRequired = true)
        where TNumber : struct, INumber<TNumber>
    {
        if (isRequired)
            ruleBuilder = ruleBuilder.NotNull().WithMessage(defaultRequiredMessage);

        ruleBuilder = ruleBuilder.GreaterThan(TNumber.Zero).WithMessage(message ?? "Invalid ID.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, TNumber> ValidID<T, TNumber>(this IRuleBuilder<T, TNumber> ruleBuilder, string? message = null)
    where TNumber : struct, INumber<TNumber>
    {
        return ruleBuilder.GreaterThan(TNumber.Zero).WithMessage(message ?? "Invalid ID.");
    }


    public static IRuleBuilder<T, TNumber?> PositiveNumber<T, TNumber>(this IRuleBuilder<T, TNumber?> ruleBuilder, string? message = null, bool isRequired = true)
        where TNumber : struct, INumber<TNumber>
    {
        if (isRequired)
            ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder
            .GreaterThan(TNumber.Zero).WithMessage("{PropertyName} must be a positive number.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, DateTime> DateNotInFuture<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        ruleBuilder = ruleBuilder
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date cannot be in the future.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, DateTime?> DateNotInFuture<T>(this IRuleBuilder<T, DateTime?> ruleBuilder, string? message = null, bool isRequired = true)
    {
        if (isRequired)
            ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date cannot be in the future.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, string?> ValidEmail<T>(this IRuleBuilder<T, string?> ruleBuilder, string? message = null, bool isRequired = true)
    {
        if (isRequired)
            ruleBuilder = ruleBuilder.NotEmptyOrNull(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder.EmailAddress().WithMessage("Invalid email format."); ;

        return ruleBuilder;
    }

    public static IRuleBuilder<T, string?> PasswordsMatch<T>(this IRuleBuilder<T, string?> ruleBuilder, Expression<Func<T, string?>> expression, string? message = null)
    {
        return ruleBuilder.Equal(expression).WithMessage(message ?? "Passwords do not match."); ;
    }

    public static IRuleBuilder<T, ModelWeight?> ValidModelWeight<T>(this IRuleBuilder<T, ModelWeight?> ruleBuilder, string? message = null, bool isRequired = true, double minWeight = 0)
    {
        if (isRequired)
        {
            ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

            ruleBuilder = ruleBuilder
                .Must(mw => mw != null && mw.Value.Weight >= minWeight)
                .WithMessage($"Weight must be at least {minWeight}.");
        }
        else
        {
            ruleBuilder = ruleBuilder
                .Must(mw => mw == null || mw.Value.Weight >= minWeight)
                .WithMessage($"Weight must be at least {minWeight}.");
        }

        return ruleBuilder;
    }

    public static IRuleBuilder<T, ModelWeight> ValidModelWeight<T>(this IRuleBuilder<T, ModelWeight> ruleBuilder, string? message = null, double minWeight = 0)
    {
        ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder
            .Must(mw => mw.Weight >= minWeight)
            .WithMessage($"Weight must be at least {minWeight}.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, ModelSize?> ValidModelSize<T>(this IRuleBuilder<T, ModelSize?> ruleBuilder, string? message = null, bool isRequired = true, double minSize = 0)
    {
        if (isRequired)
        {
            ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

            ruleBuilder = ruleBuilder
                .Must(ms => ms != null && ms.Value.Size >= minSize)
                .WithMessage($"Size must be at least {minSize}.");
        }
        else
        {
            ruleBuilder = ruleBuilder
                .Must(ms => ms == null || ms.Value.Size >= minSize)
                .WithMessage($"Size must be at least {minSize}.");
        }

        return ruleBuilder;
    }

    public static IRuleBuilder<T, ModelSize> ValidModelSize<T>(this IRuleBuilder<T, ModelSize> ruleBuilder, string? message = null, double minSize = 0)
    {
        ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder
            .Must(ms => ms.Size >= minSize)
            .WithMessage($"Size must be at least {minSize}.");

        return ruleBuilder;
    }

    public static IRuleBuilder<T, TimeSpanModel?> ValidTimeSpanModel<T>(this IRuleBuilder<T, TimeSpanModel?> ruleBuilder, string? message = null, bool isRequired = true)
    {
        Func<TimeSpanModel, bool> hasTimeSpanValue = (ts) =>
        {
            return ts.Days > 0 || ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 || ts.Milliseconds > 0;
        };

        if (isRequired)
        {
            ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

            ruleBuilder = ruleBuilder
                .Must(ts => ts != null && hasTimeSpanValue(ts.Value))
                .WithMessage("Time is required.");
        }
        else
        {
            ruleBuilder = ruleBuilder
                .Must(ts => ts == null || hasTimeSpanValue(ts.Value))
                .WithMessage("Time is required.");
        }

        return ruleBuilder;
    }

    public static IRuleBuilder<T, TimeSpanModel> ValidTimeSpanModel<T>(this IRuleBuilder<T, TimeSpanModel> ruleBuilder, string? message = null)
    {
        Func<TimeSpanModel, bool> hasTimeSpanValue = (ts) =>
        {
            return ts.Days > 0 || ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 || ts.Milliseconds > 0;
        };

        ruleBuilder = ruleBuilder.NotNull().WithMessage(message ?? defaultRequiredMessage);

        ruleBuilder = ruleBuilder
            .Must(ts => hasTimeSpanValue(ts))
            .WithMessage("Time is required.");

        return ruleBuilder;
    }


    public static IRuleBuilder<T, string?> NotEmptyOrNull<T>(this IRuleBuilder<T, string?> ruleBuilder, string message)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(message)
            .NotNull().WithMessage(message); ;
    }
}
