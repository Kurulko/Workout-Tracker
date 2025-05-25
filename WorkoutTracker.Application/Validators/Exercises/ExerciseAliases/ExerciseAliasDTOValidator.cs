using FluentValidation;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseAliases;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Exercises.ExerciseAliases;

internal class ExerciseAliasDTOValidator : AbstractValidator<ExerciseAliasDTO>
{
    public ExerciseAliasDTOValidator()
    {
        RuleFor(m => m.Id)
            .ValidID();

        RuleFor(m => m.Name)
            .ValidName();

        RuleFor(m => m.ExerciseId)
            .ValidID("Invalid Exercise ID."); 
    }
}