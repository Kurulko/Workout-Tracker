using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Persistence.Configurations.Workouts;

internal class WorkoutRecordConfiguration : DbModelConfiguration<WorkoutRecord>
{
    public override void Configure(EntityTypeBuilder<WorkoutRecord> builder)
    {
        base.Configure(builder);

        builder.ToTable(b => b.HasCheckConstraint("CK_WorkoutRecord_Date", $"{nameof(WorkoutRecord.Date)} <= GETDATE()"));

        builder.Property(er => er.Date).IsRequired();
        builder.Property(er => er.Time).IsRequired();

        builder.HasMany(wr => wr.ExerciseRecordGroups)
            .WithOne(erg => erg.WorkoutRecord)
            .HasForeignKey(er => er.WorkoutRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
