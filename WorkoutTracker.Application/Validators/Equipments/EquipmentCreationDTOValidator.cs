using FluentValidation;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Equipments;

internal class EquipmentCreationDTOValidator : AbstractValidator<EquipmentCreationDTO>
{
    public EquipmentCreationDTOValidator()
    {
        RuleFor(m => m.Name)
            .ValidName();
    }
}
