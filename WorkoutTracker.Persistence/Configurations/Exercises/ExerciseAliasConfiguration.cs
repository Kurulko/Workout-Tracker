using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Persistence.Configurations.Base;

namespace WorkoutTracker.Persistence.Configurations.Exercises;

internal class ExerciseAliasConfiguration : BaseWorkoutModelConfiguration<ExerciseAlias>
{
    public override void Configure(EntityTypeBuilder<ExerciseAlias> builder)
    {
        base.Configure(builder);

        builder
            .HasIndex(e => new { e.Name, e.ExerciseId })
            .IsUnique();
    }
}