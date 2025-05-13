using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Persistence.Configurations.Base;

internal  abstract class BaseWorkoutModelConfiguration<T> : DbModelConfiguration<T>
    where T : BaseWorkoutModel
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(20);
    }
}
