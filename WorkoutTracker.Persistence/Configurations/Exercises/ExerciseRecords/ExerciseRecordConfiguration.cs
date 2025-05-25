using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Persistence.Extensions;

namespace WorkoutTracker.Persistence.Configurations.Exercises.ExerciseRecords;

internal class ExerciseRecordConfiguration : DbModelConfiguration<ExerciseRecord>
{
    public override void Configure(EntityTypeBuilder<ExerciseRecord> builder)
    {
        base.Configure(builder);

        builder.ToTable(b => b.HasCheckConstraint("CK_ExerciseRecord_Date", $"{nameof(ExerciseRecord.Date)} <= GETDATE()"));

        builder.ApplyModelWeightConversion(m => m.Weight);

        builder.Property(er => er.Date).IsRequired();

        builder.Property(er => er.Time).IsRequired(false);
        builder.Property(er => er.Weight).IsRequired(false);
        builder.Property(er => er.Reps).IsRequired(false);
    }
}
