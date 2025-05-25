using FluentValidation;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Exercises.ExerciseRecords;

internal class ExerciseRecordCreationDTOValidator : AbstractValidator<ExerciseRecordCreationDTO>
{
    public ExerciseRecordCreationDTOValidator()
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
                context.AddFailure($"Only one of {nameof(ExerciseRecordCreationDTO.Reps)} or {nameof(ExerciseRecordCreationDTO.Time)} should be provided, not both.");
            }
            else if (!hasReps && !hasTime)
            {
                context.AddFailure($"Either {nameof(ExerciseRecordCreationDTO.Reps)} or {nameof(ExerciseRecordCreationDTO.Time)} must be provided.");
            }
        });
    }
}