using System.ComponentModel.DataAnnotations;

namespace WorkoutTrackerAPI.ValidationAttributes;

public class PositiveNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        switch (value)
        {
            case int intValue when intValue > 0:
            case float floatValue when floatValue > 0:
            case double doubleValue when doubleValue > 0:
            case decimal decimalValue when decimalValue > 0:
                return ValidationResult.Success;

            default:
                return new ValidationResult(ErrorMessage ?? "Value must be a positive number.");
        }
    }
}