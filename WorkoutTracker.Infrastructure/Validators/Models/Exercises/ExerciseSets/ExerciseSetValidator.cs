using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Exercises.ExerciseSets;

public class ExerciseSetValidator : DbModelValidator<ExerciseSet>
{
    public ExerciseSetValidator(IExerciseSetRepository exerciseSetRepository)
        : base("Exercise set", exerciseSetRepository)
    {
    }

    public override void Validate(ExerciseSet model)
    {
        if (model.Weight.HasValue)
            ArgumentValidator.ThrowIfModelWeightNegative(model.Weight.Value, nameof(ExerciseSet.Weight));

        if (model.Reps.HasValue)
            ArgumentValidator.ThrowIfValueNegative(model.Reps.Value, nameof(ExerciseSet.Reps));
    }
}