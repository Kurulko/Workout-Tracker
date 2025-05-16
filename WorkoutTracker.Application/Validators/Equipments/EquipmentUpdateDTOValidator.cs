using FluentValidation;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Equipments;

internal class EquipmentUpdateDTOValidator : AbstractValidator<EquipmentUpdateDTO>
{
    public EquipmentUpdateDTOValidator()
    {
        RuleFor(m => m.Id)
            .ValidID();

        RuleFor(m => m.Name)
            .ValidName();

        RuleFor(m => m.Image)
            .NotEmpty().WithMessage("Image must not be empty.");
    }
}
