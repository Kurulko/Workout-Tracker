using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Persistence.Extensions;

namespace WorkoutTracker.Persistence.Configurations;

internal class BodyWeightConfiguration : DbModelConfiguration<BodyWeight>
{
    public override void Configure(EntityTypeBuilder<BodyWeight> builder)
    {
        base.Configure(builder);

        builder.ToTable(b => b.HasCheckConstraint("CK_BodyWeight_Date", $"{nameof(BodyWeight.Date)} <= GETDATE()"));

        builder.ApplyModelWeightConversion(m => m.Weight);

        builder.Property(er => er.Date).IsRequired();
        builder.Property(er => er.Weight).IsRequired();
    }
}
