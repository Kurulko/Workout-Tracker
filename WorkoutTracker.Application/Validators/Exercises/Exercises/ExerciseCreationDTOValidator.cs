using FluentValidation;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Exercises.Exercises;

internal class ExerciseCreationDTOValidator : AbstractValidator<ExerciseCreationDTO>
{
    public ExerciseCreationDTOValidator()
    {
        RuleFor(m => m.Name)
            .ValidName(isRequired: false);

        RuleFor(m => m.Description)
            .ValidDescription(isRequired: false);

        RuleFor(m => m.Type)
            .NotNull().WithMessage("Type is required.");
    }
}