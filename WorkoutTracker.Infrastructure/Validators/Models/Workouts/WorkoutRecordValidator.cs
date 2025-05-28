using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Workouts;

public class WorkoutRecordValidator : DbModelValidator<WorkoutRecord>
{
    public WorkoutRecordValidator(IWorkoutRecordRepository workoutRecordRepository)
        : base("Workout record", workoutRecordRepository)
    {
    }

    public override void Validate(WorkoutRecord model)
    {
        ArgumentValidator.ThrowIfDateInFuture(model.Date, nameof(WorkoutRecord.Date));
    }
}