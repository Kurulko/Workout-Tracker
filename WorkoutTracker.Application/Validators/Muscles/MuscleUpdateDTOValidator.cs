using FluentValidation;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Muscles;

internal class MuscleUpdateDTOValidator : AbstractValidator<MuscleUpdateDTO>
{
    public MuscleUpdateDTOValidator()
    {
        RuleFor(m => m.Id)
            .ValidID();

        RuleFor(m => m.Name)
            .ValidName();

        RuleFor(m => m.Image)
            .NotEmpty().WithMessage("Image must not be empty.");

        RuleFor(m => m.ParentMuscleId)
            .ValidID("Invalid Parent Muscle ID.", false);
    }
}