using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Exercises;

public class ExerciseValidator : BaseWorkoutModelValidator<Exercise>
{
    public ExerciseValidator(IExerciseRepository exerciseRepository) 
        : base("Exercise", exerciseRepository)
    {
    }

    public override void Validate(Exercise model)
    {
    }
}
