using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTracker.Persistence.Configurations.Exercises.ExerciseSets;

internal class ExerciseSetGroupConfigurationn : DbModelConfiguration<ExerciseSetGroup>
{
    public override void Configure(EntityTypeBuilder<ExerciseSetGroup> builder)
    {
        base.Configure(builder);

        builder.HasMany(er => er.ExerciseSets)
            .WithOne(erg => erg.ExerciseSetGroup)
            .HasForeignKey(er => er.ExerciseSetGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}