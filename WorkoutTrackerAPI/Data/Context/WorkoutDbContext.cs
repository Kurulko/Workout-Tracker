using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data;

public class WorkoutDbContext : IdentityDbContext<User, Role, string>
{
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Muscle> Muscles => Set<Muscle>();
    public DbSet<Workout> Workouts => Set<Workout>();

    public WorkoutDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.CreatedByUser)
            .WithMany(u => u.CreatedExercises)
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull); 

        modelBuilder.Entity<ExerciseRecord>()
            .HasOne(er => er.User)
            .WithMany(u => u.ExerciseRecords)
            .HasForeignKey(er => er.UserId);

        modelBuilder.Entity<ExerciseRecord>()
            .HasOne(er => er.Exercise)
            .WithMany(e => e.ExerciseRecords)
            .HasForeignKey(er => er.ExerciseId);
    }
}
