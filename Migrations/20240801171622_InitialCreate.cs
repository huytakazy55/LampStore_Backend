using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LampImage_Lamp_LampId",
                table: "LampImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lamp",
                table: "Lamp");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Lamp");

            migrationBuilder.RenameTable(
                name: "Lamp",
                newName: "Lamps");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Lamps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lamps",
                table: "Lamps",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lamps_CategoryId",
                table: "Lamps",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_LampImage_Lamps_LampId",
                table: "LampImage",
                column: "LampId",
                principalTable: "Lamps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lamps_Categories_CategoryId",
                table: "Lamps",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LampImage_Lamps_LampId",
                table: "LampImage");

            migrationBuilder.DropForeignKey(
                name: "FK_Lamps_Categories_CategoryId",
                table: "Lamps");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lamps",
                table: "Lamps");

            migrationBuilder.DropIndex(
                name: "IX_Lamps_CategoryId",
                table: "Lamps");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Lamps");

            migrationBuilder.RenameTable(
                name: "Lamps",
                newName: "Lamp");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Lamp",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lamp",
                table: "Lamp",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LampImage_Lamp_LampId",
                table: "LampImage",
                column: "LampId",
                principalTable: "Lamp",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
