using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Persistence.Context;

internal class WorkoutDbContext : IdentityDbContext<User>
{
    public DbSet<UserDetails> UsersDetails => Set<UserDetails>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseAlias> ExerciseAliases => Set<ExerciseAlias>();
    public DbSet<ExerciseRecord> ExerciseRecords => Set<ExerciseRecord>();
    public DbSet<ExerciseRecordGroup> ExerciseRecordGroups => Set<ExerciseRecordGroup>();
    public DbSet<ExerciseSet> ExerciseSets => Set<ExerciseSet>();
    public DbSet<ExerciseSetGroup> ExerciseSetGroups => Set<ExerciseSetGroup>();
    public DbSet<Muscle> Muscles => Set<Muscle>();
    public DbSet<MuscleSize> MuscleSizes => Set<MuscleSize>();
    public DbSet<BodyWeight> BodyWeights => Set<BodyWeight>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutRecord> WorkoutRecords => Set<WorkoutRecord>();

    public WorkoutDbContext(DbContextOptions options) : base(options) { }
         //=> Database.EnsureCreated();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkoutDbContext).Assembly);
    }
}