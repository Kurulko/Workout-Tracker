using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Persistence.Extensions;

namespace WorkoutTracker.Persistence.Configurations.Muscles;

internal class MuscleSizeConfiguration : DbModelConfiguration<MuscleSize>
{
    public override void Configure(EntityTypeBuilder<MuscleSize> builder)
    {
        base.Configure(builder);

        builder.ToTable(b => b.HasCheckConstraint("CK_MuscleSize_Date", $"{nameof(MuscleSize.Date)} <= GETDATE()"));

        builder.ApplyModelSizeConversion(m => m.Size);

        builder.Property(er => er.Date).IsRequired();
        builder.Property(er => er.Size).IsRequired();
    }
}
