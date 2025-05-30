using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Persistence.Migrations
{
    public partial class UpdateModelImagesPaths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Muscles 
                SET Image = 'images' + SUBSTRING(Image, LEN('photos') + 1, LEN(Image))
                WHERE Image LIKE 'photos%' AND Image IS NOT NULL;
        
                UPDATE Exercises 
                SET Image = 'images' + SUBSTRING(Image, LEN('photos') + 1, LEN(Image))
                WHERE Image LIKE 'photos%' AND Image IS NOT NULL;

                UPDATE Equipments 
                SET Image = 'images' + SUBSTRING(Image, LEN('photos') + 1, LEN(Image))
                WHERE Image LIKE 'photos%' AND Image IS NOT NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Muscles 
                SET Image = 'photos' + SUBSTRING(Image, LEN('images') + 1, LEN(Image))
                WHERE Image LIKE 'images%' AND Image IS NOT NULL;

                UPDATE Exercises 
                SET Image = 'photos' + SUBSTRING(Image, LEN('images') + 1, LEN(Image))
                WHERE Image LIKE 'images%' AND Image IS NOT NULL;

                UPDATE Equipments 
                SET Image = 'photos' + SUBSTRING(Image, LEN('images') + 1, LEN(Image))
                WHERE Image LIKE 'images%' AND Image IS NOT NULL;
            ");
        }
    }
}
