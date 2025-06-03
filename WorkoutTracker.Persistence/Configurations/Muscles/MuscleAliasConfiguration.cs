using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Persistence.Configurations.Base;

namespace WorkoutTracker.Persistence.Configurations.Muscles;

internal class MuscleAliasConfiguration : BaseWorkoutModelConfiguration<MuscleAlias>
{
    public override void Configure(EntityTypeBuilder<MuscleAlias> builder)
    {
        base.Configure(builder);

        builder
            .HasIndex(e => new { e.Name, e.MuscleId })
            .IsUnique();
    }
}