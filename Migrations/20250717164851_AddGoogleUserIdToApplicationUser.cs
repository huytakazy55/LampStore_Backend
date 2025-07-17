using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleUserIdToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleUserId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleUserId",
                table: "AspNetUsers");
        }
    }
}
