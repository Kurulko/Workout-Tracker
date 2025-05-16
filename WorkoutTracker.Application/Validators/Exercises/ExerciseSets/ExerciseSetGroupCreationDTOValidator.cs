using FluentValidation;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSetGroups;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Exercises.ExerciseSets;

internal class ExerciseSetGroupCreationDTOValidator : AbstractValidator<ExerciseSetGroupCreationDTO>
{
    public ExerciseSetGroupCreationDTOValidator()
    {
        RuleFor(m => m.ExerciseId)
            .ValidID("Invalid Exercise ID.");

        RuleFor(m => m.WorkoutId)
            .ValidID("Invalid Workout ID.");

        RuleFor(m => m.ExerciseSets)
            .NotEmpty().WithMessage("ExerciseSets must not be empty.");
    }
}