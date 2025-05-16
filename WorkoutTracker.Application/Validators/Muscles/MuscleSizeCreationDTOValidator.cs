using FluentValidation;
using WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Muscles;

internal class MuscleSizeCreationDTOValidator : AbstractValidator<MuscleSizeCreationDTO>
{
    public MuscleSizeCreationDTOValidator()
    {
        RuleFor(m => m.Date)
            .DateNotInFuture();

        RuleFor(m => m.Size)
            .ValidModelSize();

        RuleFor(m => m.MuscleId)
            .ValidID("Invalid Muscle ID.");
    }
}