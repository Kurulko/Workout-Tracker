using FluentValidation;
using WorkoutTracker.Application.DTOs.Workouts.Workouts;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Workouts;

internal class WorkoutCreationDTOValidator : AbstractValidator<WorkoutCreationDTO>
{
    public WorkoutCreationDTOValidator()
    {
        RuleFor(m => m.Name)
            .ValidName();

        RuleFor(m => m.Description)
            .ValidDescription(isRequired: false);

        RuleFor(m => m.ExerciseSetGroups)
            .NotEmpty().WithMessage("ExerciseSetGroups must not be empty.");
    }
}
