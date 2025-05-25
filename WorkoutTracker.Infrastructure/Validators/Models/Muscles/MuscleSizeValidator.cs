using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Muscles;

public class MuscleSizeValidator : DbModelValidator<MuscleSize>
{
    public MuscleSizeValidator(IMuscleSizeRepository muscleSizeRepository)
        : base("Muscle size", muscleSizeRepository)
    {
    }

    public override void Validate(MuscleSize model)
    {
        ArgumentValidator.ThrowIfDateInFuture(model.Date, nameof(MuscleSize.Date));
        ArgumentValidator.ThrowIfModelSizeNegative(model.Size, nameof(MuscleSize.Size));
    }
}