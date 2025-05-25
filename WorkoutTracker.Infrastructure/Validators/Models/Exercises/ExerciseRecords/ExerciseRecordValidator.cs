using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Exercises.ExerciseRecords;

public class ExerciseRecordValidator : DbModelValidator<ExerciseRecord>
{
    public ExerciseRecordValidator(IExerciseRecordRepository exerciseRecordRepository) 
        : base("Exercise record", exerciseRecordRepository)
    {
    }

    public override void Validate(ExerciseRecord model)
    {
        ArgumentValidator.ThrowIfDateInFuture(model.Date, nameof(ExerciseRecord.Date));

        if(model.Weight.HasValue)
            ArgumentValidator.ThrowIfModelWeightNegative(model.Weight.Value, nameof(ExerciseRecord.Weight));

        if(model.Reps.HasValue)
            ArgumentValidator.ThrowIfValueNegative(model.Reps.Value, nameof(ExerciseRecord.Reps));
    }
}
