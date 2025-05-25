using FluentValidation;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Muscles;

internal class MuscleCreationDTOValidator : AbstractValidator<MuscleCreationDTO>
{
    public MuscleCreationDTOValidator()
    {
        RuleFor(m => m.Name)
            .ValidName();

        RuleFor(m => m.ParentMuscleId)
            .ValidID("Invalid Parent Muscle ID.", false);
    }
}
