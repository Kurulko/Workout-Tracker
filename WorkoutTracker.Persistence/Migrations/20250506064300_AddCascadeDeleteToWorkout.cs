using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteToWorkout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_AspNetUsers_OwnedByUserId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_AspNetUsers_CreatedByUserId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseSets_AspNetUsers_UserId",
                table: "ExerciseSets");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseSets_UserId",
                table: "ExerciseSets");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CreatedByUserId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_OwnedByUserId",
                table: "Equipments");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Workouts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ExerciseSets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Exercises",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Equipments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId1",
                table: "Workouts",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_UserId",
                table: "Exercises",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_UserId",
                table: "Equipments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_AspNetUsers_UserId",
                table: "Equipments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_AspNetUsers_UserId",
                table: "Exercises",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_AspNetUsers_UserId1",
                table: "Workouts",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_AspNetUsers_UserId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_AspNetUsers_UserId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_AspNetUsers_UserId1",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_UserId1",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_UserId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_UserId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Equipments");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ExerciseSets",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_UserId",
                table: "ExerciseSets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CreatedByUserId",
                table: "Exercises",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_OwnedByUserId",
                table: "Equipments",
                column: "OwnedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_AspNetUsers_OwnedByUserId",
                table: "Equipments",
                column: "OwnedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_AspNetUsers_CreatedByUserId",
                table: "Exercises",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseSets_AspNetUsers_UserId",
                table: "ExerciseSets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
