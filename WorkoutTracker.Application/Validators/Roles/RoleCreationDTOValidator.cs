using FluentValidation;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.DTOs.Roles;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Roles;

internal class RoleCreationDTOValidator : AbstractValidator<RoleCreationDTO>
{
    public RoleCreationDTOValidator()
    {
        RuleFor(m => m.Name)
            .ValidName();
    }
}
