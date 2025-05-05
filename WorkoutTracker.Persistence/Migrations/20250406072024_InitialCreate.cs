using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Persistence.Migrations;

internal partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        /*
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CountOfTrainings = table.Column<int>(type: "int", nullable: false),
                Registered = table.Column<DateTime>(type: "datetime2", nullable: false),
                StartedWorkingOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Muscles",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsMeasurable = table.Column<bool>(type: "bit", nullable: false),
                ParentMuscleId = table.Column<long>(type: "bigint", nullable: true),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Muscles", x => x.Id);
                table.ForeignKey(
                    name: "FK_Muscles_Muscles_ParentMuscleId",
                    column: x => x.ParentMuscleId,
                    principalTable: "Muscles",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BodyWeights",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                Weight = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BodyWeights", x => x.Id);
                table.ForeignKey(
                    name: "FK_BodyWeights_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Equipments",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                OwnedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Equipments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Equipments_AspNetUsers_OwnedByUserId",
                    column: x => x.OwnedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Exercises",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Type = table.Column<int>(type: "int", nullable: false),
                CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Exercises", x => x.Id);
                table.ForeignKey(
                    name: "FK_Exercises_AspNetUsers_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "UsersDetails",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Gender = table.Column<int>(type: "int", nullable: true),
                Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                BodyFatPercentage = table.Column<double>(type: "float", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UsersDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_UsersDetails_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Workouts",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsPinned = table.Column<bool>(type: "bit", nullable: false),
                CountOfTrainings = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Workouts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Workouts_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MuscleSizes",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MuscleId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MuscleSizes", x => x.Id);
                table.ForeignKey(
                    name: "FK_MuscleSizes_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MuscleSizes_Muscles_MuscleId",
                    column: x => x.MuscleId,
                    principalTable: "Muscles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EquipmentExercise",
            columns: table => new
            {
                EquipmentsId = table.Column<long>(type: "bigint", nullable: false),
                ExercisesId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EquipmentExercise", x => new { x.EquipmentsId, x.ExercisesId });
                table.ForeignKey(
                    name: "FK_EquipmentExercise_Equipments_EquipmentsId",
                    column: x => x.EquipmentsId,
                    principalTable: "Equipments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EquipmentExercise_Exercises_ExercisesId",
                    column: x => x.ExercisesId,
                    principalTable: "Exercises",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExerciseMuscle",
            columns: table => new
            {
                ExercisesId = table.Column<long>(type: "bigint", nullable: false),
                WorkingMusclesId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExerciseMuscle", x => new { x.ExercisesId, x.WorkingMusclesId });
                table.ForeignKey(
                    name: "FK_ExerciseMuscle_Exercises_ExercisesId",
                    column: x => x.ExercisesId,
                    principalTable: "Exercises",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ExerciseMuscle_Muscles_WorkingMusclesId",
                    column: x => x.WorkingMusclesId,
                    principalTable: "Muscles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExerciseSetGroups",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                WorkoutId = table.Column<long>(type: "bigint", nullable: false),
                ExerciseId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExerciseSetGroups", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExerciseSetGroups_Exercises_ExerciseId",
                    column: x => x.ExerciseId,
                    principalTable: "Exercises",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ExerciseSetGroups_Workouts_WorkoutId",
                    column: x => x.WorkoutId,
                    principalTable: "Workouts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WorkoutRecords",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Time = table.Column<TimeSpan>(type: "time", nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                WorkoutId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkoutRecords", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkoutRecords_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_WorkoutRecords_Workouts_WorkoutId",
                    column: x => x.WorkoutId,
                    principalTable: "Workouts",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "ExerciseSets",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Time = table.Column<TimeSpan>(type: "time", nullable: true),
                Reps = table.Column<int>(type: "int", nullable: true),
                ExerciseId = table.Column<long>(type: "bigint", nullable: false),
                ExerciseSetGroupId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExerciseSets", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExerciseSets_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ExerciseSets_Exercises_ExerciseId",
                    column: x => x.ExerciseId,
                    principalTable: "Exercises",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ExerciseSets_ExerciseSetGroups_ExerciseSetGroupId",
                    column: x => x.ExerciseSetGroupId,
                    principalTable: "ExerciseSetGroups",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "ExerciseRecordGroups",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ExerciseId = table.Column<long>(type: "bigint", nullable: false),
                WorkoutRecordId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExerciseRecordGroups", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExerciseRecordGroups_Exercises_ExerciseId",
                    column: x => x.ExerciseId,
                    principalTable: "Exercises",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ExerciseRecordGroups_WorkoutRecords_WorkoutRecordId",
                    column: x => x.WorkoutRecordId,
                    principalTable: "WorkoutRecords",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExerciseRecords",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                Weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Time = table.Column<TimeSpan>(type: "time", nullable: true),
                Reps = table.Column<int>(type: "int", nullable: true),
                ExerciseId = table.Column<long>(type: "bigint", nullable: false),
                ExerciseRecordGroupId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExerciseRecords", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExerciseRecords_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                    column: x => x.ExerciseRecordGroupId,
                    principalTable: "ExerciseRecordGroups",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ExerciseRecords_Exercises_ExerciseId",
                    column: x => x.ExerciseId,
                    principalTable: "Exercises",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true,
            filter: "[NormalizedName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true,
            filter: "[NormalizedUserName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_BodyWeights_UserId",
            table: "BodyWeights",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_EquipmentExercise_ExercisesId",
            table: "EquipmentExercise",
            column: "ExercisesId");

        migrationBuilder.CreateIndex(
            name: "IX_Equipments_Name",
            table: "Equipments",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Equipments_OwnedByUserId",
            table: "Equipments",
            column: "OwnedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseMuscle_WorkingMusclesId",
            table: "ExerciseMuscle",
            column: "WorkingMusclesId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseRecordGroups_ExerciseId",
            table: "ExerciseRecordGroups",
            column: "ExerciseId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseRecordGroups_WorkoutRecordId",
            table: "ExerciseRecordGroups",
            column: "WorkoutRecordId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseRecords_ExerciseId",
            table: "ExerciseRecords",
            column: "ExerciseId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseRecords_ExerciseRecordGroupId",
            table: "ExerciseRecords",
            column: "ExerciseRecordGroupId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseRecords_UserId",
            table: "ExerciseRecords",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Exercises_CreatedByUserId",
            table: "Exercises",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Exercises_Name",
            table: "Exercises",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseSetGroups_ExerciseId",
            table: "ExerciseSetGroups",
            column: "ExerciseId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseSetGroups_WorkoutId",
            table: "ExerciseSetGroups",
            column: "WorkoutId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseSets_ExerciseId",
            table: "ExerciseSets",
            column: "ExerciseId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseSets_ExerciseSetGroupId",
            table: "ExerciseSets",
            column: "ExerciseSetGroupId");

        migrationBuilder.CreateIndex(
            name: "IX_ExerciseSets_UserId",
            table: "ExerciseSets",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Muscles_Name",
            table: "Muscles",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Muscles_ParentMuscleId",
            table: "Muscles",
            column: "ParentMuscleId");

        migrationBuilder.CreateIndex(
            name: "IX_MuscleSizes_MuscleId",
            table: "MuscleSizes",
            column: "MuscleId");

        migrationBuilder.CreateIndex(
            name: "IX_MuscleSizes_UserId",
            table: "MuscleSizes",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UsersDetails_UserId",
            table: "UsersDetails",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_WorkoutRecords_UserId",
            table: "WorkoutRecords",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkoutRecords_WorkoutId",
            table: "WorkoutRecords",
            column: "WorkoutId");

        migrationBuilder.CreateIndex(
            name: "IX_Workouts_Name",
            table: "Workouts",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Workouts_UserId",
            table: "Workouts",
            column: "UserId");
        */
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        /*
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "BodyWeights");

        migrationBuilder.DropTable(
            name: "EquipmentExercise");

        migrationBuilder.DropTable(
            name: "ExerciseMuscle");

        migrationBuilder.DropTable(
            name: "ExerciseRecords");

        migrationBuilder.DropTable(
            name: "ExerciseSets");

        migrationBuilder.DropTable(
            name: "MuscleSizes");

        migrationBuilder.DropTable(
            name: "UsersDetails");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "Equipments");

        migrationBuilder.DropTable(
            name: "ExerciseRecordGroups");

        migrationBuilder.DropTable(
            name: "ExerciseSetGroups");

        migrationBuilder.DropTable(
            name: "Muscles");

        migrationBuilder.DropTable(
            name: "WorkoutRecords");

        migrationBuilder.DropTable(
            name: "Exercises");

        migrationBuilder.DropTable(
            name: "Workouts");

        migrationBuilder.DropTable(
            name: "AspNetUsers");
        */
    }
}
