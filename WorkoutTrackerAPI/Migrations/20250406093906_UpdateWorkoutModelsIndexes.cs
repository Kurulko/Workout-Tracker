using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTrackerAPI.Migrations
{
    public partial class UpdateWorkoutModelsIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_Name",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Name",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_Name",
                table: "Equipments");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_Name_UserId",
                table: "Workouts",
                columns: new[] { "Name", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name_CreatedByUserId",
                table: "Exercises",
                columns: new[] { "Name", "CreatedByUserId" },
                unique: true,
                filter: "[CreatedByUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_Name_OwnedByUserId",
                table: "Equipments",
                columns: new[] { "Name", "OwnedByUserId" },
                unique: true,
                filter: "[OwnedByUserId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_Name_UserId",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Name_CreatedByUserId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_Name_OwnedByUserId",
                table: "Equipments");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_Name",
                table: "Workouts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_Name",
                table: "Equipments",
                column: "Name",
                unique: true);
        }
    }
}
