using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Muscles;

public class MuscleValidator : BaseWorkoutModelValidator<Muscle>
{
    public MuscleValidator(IMuscleRepository muscleRepository)
        : base("Muscle", muscleRepository)
    {
    }

    public override void Validate(Muscle model)
    {
    }
}
