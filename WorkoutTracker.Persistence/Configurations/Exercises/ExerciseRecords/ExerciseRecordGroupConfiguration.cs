using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Persistence.Configurations.Base;

namespace WorkoutTracker.Persistence.Configurations.Exercises.ExerciseRecords;

internal class ExerciseRecordGroupConfiguration : DbModelConfiguration<ExerciseRecordGroup>
{
    public override void Configure(EntityTypeBuilder<ExerciseRecordGroup> builder)
    {
        base.Configure(builder);

        builder.HasMany(er => er.ExerciseRecords)
            .WithOne(erg => erg.ExerciseRecordGroup)
            .HasForeignKey(er => er.ExerciseRecordGroupId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}
