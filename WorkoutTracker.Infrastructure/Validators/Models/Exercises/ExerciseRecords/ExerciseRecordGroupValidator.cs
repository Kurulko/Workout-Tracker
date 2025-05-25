using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Exercises;

public class ExerciseRecordGroupValidator : DbModelValidator<ExerciseRecordGroup>
{
    public ExerciseRecordGroupValidator(IExerciseRecordGroupRepository exerciseRecordGroupRepository)
        : base("Exercise record group", exerciseRecordGroupRepository)
    {
    }

    public override void Validate(ExerciseRecordGroup model)
    {
    }
}