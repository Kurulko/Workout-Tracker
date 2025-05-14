using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Persistence.Configurations.Users;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(b => {
            b.HasCheckConstraint("CK_User_Registered", $"{nameof(User.Registered)} <= GETDATE()");
            b.HasCheckConstraint("CK_User_StartedWorkingOut", $"{nameof(User.StartedWorkingOut)} <= GETDATE()");
        });


        builder.Property(er => er.CountOfTrainings).HasDefaultValue(0).IsRequired();
        builder.Property(er => er.Registered).IsRequired();
        builder.Property(er => er.StartedWorkingOut).IsRequired(false);

        builder
            .HasOne(u => u.UserDetails)
            .WithOne()
            .HasForeignKey<UserDetails>(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.BodyWeights)
            .WithOne()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.MuscleSizes)
            .WithOne()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.Workouts)
            .WithOne()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.WorkoutRecords)
            .WithOne()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.CreatedExercises)
            .WithOne()
            .HasForeignKey(m => m.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(u => u.UserEquipments)
            .WithOne()
            .HasForeignKey(m => m.OwnedByUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}