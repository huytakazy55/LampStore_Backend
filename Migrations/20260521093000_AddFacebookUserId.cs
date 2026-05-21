using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    [Migration("20260521093000_AddFacebookUserId")]
    public partial class AddFacebookUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookUserId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookUserId",
                table: "AspNetUsers");
        }
    }
}
