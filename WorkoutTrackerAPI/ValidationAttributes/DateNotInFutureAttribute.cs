using System.ComponentModel.DataAnnotations;

namespace WorkoutTrackerAPI.ValidationAttributes;

public class DateNotInFutureAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date && date > DateTime.Now)
        {
            return new ValidationResult(ErrorMessage ?? "Date cannot be in the future.");
        }

        return ValidationResult.Success;
    }
}