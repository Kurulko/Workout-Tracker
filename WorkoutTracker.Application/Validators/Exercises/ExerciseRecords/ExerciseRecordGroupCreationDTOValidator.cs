using FluentValidation;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecordGroups;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Exercises.ExerciseRecords.ExerciseRecordGroups;

internal class ExerciseRecordGroupCreationDTOValidator : AbstractValidator<ExerciseRecordGroupCreationDTO>
{
    public ExerciseRecordGroupCreationDTOValidator()
    {
        RuleFor(m => m.ExerciseId)
            .ValidID("Invalid Exercise ID.");

        RuleFor(m => m.ExerciseRecords)
            .NotEmpty().WithMessage("ExerciseRecords must not be empty.");
    }
}