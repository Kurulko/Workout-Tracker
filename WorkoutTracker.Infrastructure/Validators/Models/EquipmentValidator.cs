using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models;

public class EquipmentValidator : BaseWorkoutModelValidator<Equipment>
{
    public EquipmentValidator(IEquipmentRepository equipmentRepository)
        : base("Equipment", equipmentRepository)
    {
    }

    public override void Validate(Equipment model)
    {
    }
}
