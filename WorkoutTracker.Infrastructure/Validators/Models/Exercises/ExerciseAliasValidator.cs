using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Exercises;

public class ExerciseAliasValidator : BaseWorkoutModelValidator<ExerciseAlias>
{
    public ExerciseAliasValidator(IExerciseAliasRepository exerciseAliasRepository)
        : base("Exercise alias", exerciseAliasRepository)
    {
    }

    public override void Validate(ExerciseAlias model)
    {
    }
}
