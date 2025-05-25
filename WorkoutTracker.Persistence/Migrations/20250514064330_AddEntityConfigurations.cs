using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_AspNetUsers_UserId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseRecords_AspNetUsers_UserId",
                table: "ExerciseRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                table: "ExerciseRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_AspNetUsers_UserId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseSets_ExerciseSetGroups_ExerciseSetGroupId",
                table: "ExerciseSets");

            migrationBuilder.DropForeignKey(
                name: "FK_Muscles_Muscles_ParentMuscleId",
                table: "Muscles");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutRecords_Workouts_WorkoutId",
                table: "WorkoutRecords");

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
                name: "IX_ExerciseRecords_UserId",
                table: "ExerciseRecords");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_UserId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExerciseSets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExerciseRecords");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Equipments");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Workouts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPinned",
                table: "Workouts",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Workouts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CountOfTrainings",
                table: "Workouts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Muscles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsMeasurable",
                table: "Muscles",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Exercises",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Exercises",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExerciseAliases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Equipments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "CountOfTrainings",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Workout_Created",
                table: "Workouts",
                sql: "Created <= GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorkoutRecord_Date",
                table: "WorkoutRecords",
                sql: "Date <= GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserDetails_BodyFatPercentage",
                table: "UsersDetails",
                sql: "[BodyFatPercentage] >= 0 AND [BodyFatPercentage] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserDetails_DateOfBirth",
                table: "UsersDetails",
                sql: "DateOfBirth <= GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MuscleSize_Date",
                table: "MuscleSizes",
                sql: "Date <= GETDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CreatedByUserId",
                table: "Exercises",
                column: "CreatedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExerciseRecord_Date",
                table: "ExerciseRecords",
                sql: "Date <= GETDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAliases_Name_ExerciseId",
                table: "ExerciseAliases",
                columns: new[] { "Name", "ExerciseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_OwnedByUserId",
                table: "Equipments",
                column: "OwnedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BodyWeight_Date",
                table: "BodyWeights",
                sql: "Date <= GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_Registered",
                table: "AspNetUsers",
                sql: "Registered <= GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_StartedWorkingOut",
                table: "AspNetUsers",
                sql: "StartedWorkingOut <= GETDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_AspNetUsers_OwnedByUserId",
                table: "Equipments",
                column: "OwnedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                table: "ExerciseRecords",
                column: "ExerciseRecordGroupId",
                principalTable: "ExerciseRecordGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_AspNetUsers_CreatedByUserId",
                table: "Exercises",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_AspNetUsers_OwnedByUserId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                table: "ExerciseRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_AspNetUsers_CreatedByUserId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseSets_ExerciseSetGroups_ExerciseSetGroupId",
                table: "ExerciseSets");

            migrationBuilder.DropForeignKey(
                name: "FK_Muscles_Muscles_ParentMuscleId",
                table: "Muscles");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutRecords_Workouts_WorkoutId",
                table: "WorkoutRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Workout_Created",
                table: "Workouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WorkoutRecord_Date",
                table: "WorkoutRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserDetails_BodyFatPercentage",
                table: "UsersDetails");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserDetails_DateOfBirth",
                table: "UsersDetails");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MuscleSize_Date",
                table: "MuscleSizes");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CreatedByUserId",
                table: "Exercises");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExerciseRecord_Date",
                table: "ExerciseRecords");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseAliases_Name_ExerciseId",
                table: "ExerciseAliases");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_OwnedByUserId",
                table: "Equipments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BodyWeight_Date",
                table: "BodyWeights");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_Registered",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_StartedWorkingOut",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Workouts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPinned",
                table: "Workouts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Workouts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CountOfTrainings",
                table: "Workouts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Workouts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Muscles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsMeasurable",
                table: "Muscles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ExerciseSets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Exercises",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Exercises",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ExerciseRecords",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExerciseAliases",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Equipments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Equipments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CountOfTrainings",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId1",
                table: "Workouts",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_UserId",
                table: "Exercises",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseRecords_UserId",
                table: "ExerciseRecords",
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
                name: "FK_ExerciseRecords_AspNetUsers_UserId",
                table: "ExerciseRecords",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseRecords_ExerciseRecordGroups_ExerciseRecordGroupId",
                table: "ExerciseRecords",
                column: "ExerciseRecordGroupId",
                principalTable: "ExerciseRecordGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_AspNetUsers_UserId",
                table: "Exercises",
                column: "UserId",
                principalTable: "AspNetUsers",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_AspNetUsers_UserId1",
                table: "Workouts",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
