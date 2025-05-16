using FluentValidation;
using WorkoutTracker.Application.DTOs.Workouts.WorkoutRecords;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Workouts;

internal class WorkoutRecordCreationDTOValidator : AbstractValidator<WorkoutRecordCreationDTO>
{
    public WorkoutRecordCreationDTOValidator()
    {
        RuleFor(m => m.Time)
            .ValidTimeSpanModel();

        RuleFor(m => m.Date)
            .DateNotInFuture();

        RuleFor(m => m.WorkoutId)
            .ValidID("Invalid Workout ID.");

        RuleFor(m => m.ExerciseRecordGroups)
            .NotEmpty().WithMessage("ExerciseRecordGroups must not be empty.");
    }
}