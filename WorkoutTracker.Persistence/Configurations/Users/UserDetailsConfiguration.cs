using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Persistence.Extensions;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Persistence.Configurations.Users;

internal class UserDetailsConfiguration : DbModelConfiguration<UserDetails>
{
    public override void Configure(EntityTypeBuilder<UserDetails> builder)
    {
        base.Configure(builder);

        builder.ToTable(b => {
            b.HasCheckConstraint("CK_UserDetails_DateOfBirth", $"{nameof(UserDetails.DateOfBirth)} <= GETDATE()");
            b.HasCheckConstraint("CK_UserDetails_BodyFatPercentage", $"[{nameof(UserDetails.BodyFatPercentage)}] >= 0 AND [{nameof(UserDetails.BodyFatPercentage)}] <= 100");
        });

        builder.ApplyModelSizeConversion(m => m.Height);
        builder.ApplyModelWeightConversion(m => m.Weight);

        builder.Property(er => er.Gender).IsRequired(false);
        builder.Property(er => er.Height).IsRequired(false);
        builder.Property(er => er.Weight).IsRequired(false);
        builder.Property(er => er.DateOfBirth).IsRequired(false);
        builder.Property(er => er.BodyFatPercentage).IsRequired(false);
    }
}