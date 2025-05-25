using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Workouts;

public class WorkoutValidator : BaseWorkoutModelValidator<Workout>
{
    public WorkoutValidator(IWorkoutRepository workoutRepository)
        : base("Workout", workoutRepository)
    {
    }

    public override void Validate(Workout model)
    {
        ArgumentValidator.ThrowIfDateInFuture(model.Created, nameof(Workout.Created));
        ArgumentValidator.ThrowIfValueNegative(model.CountOfTrainings, nameof(Workout.CountOfTrainings));
    }
}
