using FluentValidation;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Exercises.Exercises;

internal class ExerciseUpdateDTOValidator : AbstractValidator<ExerciseUpdateDTO>
{
    public ExerciseUpdateDTOValidator()
    {
        RuleFor(m => m.Id)
            .ValidID();

        RuleFor(m => m.Name)
            .ValidName(isRequired: false);

        RuleFor(m => m.Image)
            .NotEmpty().WithMessage("Image must not be empty.");

        RuleFor(m => m.Description)
            .ValidDescription(isRequired: false);

        RuleFor(m => m.Type)
            .NotNull().WithMessage("Type is required.");
    }
}