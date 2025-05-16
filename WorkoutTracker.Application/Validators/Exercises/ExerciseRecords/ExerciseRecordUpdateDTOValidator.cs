using FluentValidation;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Exercises.ExerciseRecords;

internal class ExerciseRecordUpdateDTOValidator : AbstractValidator<ExerciseRecordUpdateDTO>
{
    public ExerciseRecordUpdateDTOValidator()
    {
        RuleFor(m => m.Id)
            .ValidID();

        RuleFor(m => m.Date)
            .DateNotInFuture();

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
                context.AddFailure($"Only one of {nameof(ExerciseRecordUpdateDTO.Reps)} or {nameof(ExerciseRecordUpdateDTO.Time)} should be provided, not both.");
            }
            else if (!hasReps && !hasTime)
            {
                context.AddFailure($"Either {nameof(ExerciseRecordUpdateDTO.Reps)} or {nameof(ExerciseRecordUpdateDTO.Time)} must be provided.");
            }
        });
    }
}