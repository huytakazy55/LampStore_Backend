using LampStoreProjects.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260521093000_AddFacebookUserId")]
    public partial class AddFacebookUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.AspNetUsers', 'FacebookUserId') IS NULL
BEGIN
    ALTER TABLE [dbo].[AspNetUsers] ADD [FacebookUserId] nvarchar(max) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.AspNetUsers', 'FacebookUserId') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[AspNetUsers] DROP COLUMN [FacebookUserId];
END
");
        }
    }
}
