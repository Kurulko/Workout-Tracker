using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Exercises.ExerciseSets;

public class ExerciseSetGroupValidator : DbModelValidator<ExerciseSetGroup>
{
    public ExerciseSetGroupValidator(IExerciseSetGroupRepository exerciseSetGroupRepository)
        : base("Exercise set group", exerciseSetGroupRepository)
    {
    }

    public override void Validate(ExerciseSetGroup model)
    {
    }
}