using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Persistence.Extensions;

namespace WorkoutTracker.Persistence.Configurations.Exercises.ExerciseSets;

internal class ExerciseSetConfiguration : DbModelConfiguration<ExerciseSet>
{
    public override void Configure(EntityTypeBuilder<ExerciseSet> builder)
    {
        base.Configure(builder);

        builder.ApplyModelWeightConversion(m => m.Weight);

        builder.Property(er => er.Weight).IsRequired(false);
        builder.Property(er => er.Time).IsRequired(false);
        builder.Property(er => er.Reps).IsRequired(false);
    }
}