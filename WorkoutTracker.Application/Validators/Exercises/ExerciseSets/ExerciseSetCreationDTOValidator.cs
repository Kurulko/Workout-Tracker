using FluentValidation;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSets;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Exercises.ExerciseSets;

internal class ExerciseSetCreationDTOValidator : AbstractValidator<ExerciseSetCreationDTO>
{
    public ExerciseSetCreationDTOValidator()
    {
        RuleFor(m => m.Weight)
            .ValidModelWeight(isRequired: false);

        RuleFor(m => m.Time)
            .ValidTimeSpanModel(isRequired: false);

        RuleFor(m => m.Reps)
            .PositiveNumber(isRequired: false);

        RuleFor(m => m.ExerciseId)
            .ValidID("Invalid Exercise ID.");

        RuleFor(x => x).Custom((er, context) =>
        {
            bool hasReps = er.Reps.HasValue, hasTime = er.Time.HasValue;

            if (hasReps && hasTime)
            {
                context.AddFailure($"Only one of {nameof(ExerciseSetCreationDTO.Reps)} or {nameof(ExerciseSetCreationDTO.Time)} should be provided, not both.");
            }
            else if (!hasReps && !hasTime)
            {
                context.AddFailure($"Either {nameof(ExerciseSetCreationDTO.Reps)} or {nameof(ExerciseSetCreationDTO.Time)} must be provided.");
            }
        });
    }
}