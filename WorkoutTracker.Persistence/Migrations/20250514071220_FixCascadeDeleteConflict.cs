using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeleteConflict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                table: "ExerciseRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseSets_ExerciseSetGroups_ExerciseSetGroupId",
                table: "ExerciseSets");

            migrationBuilder.DropForeignKey(
                name: "FK_Muscles_Muscles_ParentMuscleId",
                table: "Muscles");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutRecords_Workouts_WorkoutId",
                table: "WorkoutRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                table: "ExerciseRecords",
                column: "ExerciseRecordGroupId",
                principalTable: "ExerciseRecordGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseSets_ExerciseSetGroups_ExerciseSetGroupId",
                table: "ExerciseSets",
                column: "ExerciseSetGroupId",
                principalTable: "ExerciseSetGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Muscles_Muscles_ParentMuscleId",
                table: "Muscles",
                column: "ParentMuscleId",
                principalTable: "Muscles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutRecords_Workouts_WorkoutId",
                table: "WorkoutRecords",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                table: "ExerciseRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseSets_ExerciseSetGroups_ExerciseSetGroupId",
                table: "ExerciseSets");

            migrationBuilder.DropForeignKey(
                name: "FK_Muscles_Muscles_ParentMuscleId",
                table: "Muscles");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutRecords_Workouts_WorkoutId",
                table: "WorkoutRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                table: "ExerciseRecords",
                column: "ExerciseRecordGroupId",
                principalTable: "ExerciseRecordGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseSets_ExerciseSetGroups_ExerciseSetGroupId",
                table: "ExerciseSets",
                column: "ExerciseSetGroupId",
                principalTable: "ExerciseSetGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Muscles_Muscles_ParentMuscleId",
                table: "Muscles",
                column: "ParentMuscleId",
                principalTable: "Muscles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutRecords_Workouts_WorkoutId",
                table: "WorkoutRecords",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
