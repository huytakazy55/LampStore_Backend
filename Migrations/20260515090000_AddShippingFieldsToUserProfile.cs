using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingFieldsToUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('UserProfiles', 'Address') IS NOT NULL
BEGIN
    ALTER TABLE [UserProfiles] ALTER COLUMN [Address] nvarchar(255) NOT NULL;
END

IF COL_LENGTH('UserProfiles', 'City') IS NULL
BEGIN
    ALTER TABLE [UserProfiles] ADD [City] nvarchar(20) NOT NULL CONSTRAINT [DF_UserProfiles_City] DEFAULT('');
END

IF COL_LENGTH('UserProfiles', 'CityName') IS NULL
BEGIN
    ALTER TABLE [UserProfiles] ADD [CityName] nvarchar(100) NOT NULL CONSTRAINT [DF_UserProfiles_CityName] DEFAULT('');
END

IF COL_LENGTH('UserProfiles', 'District') IS NULL
BEGIN
    ALTER TABLE [UserProfiles] ADD [District] nvarchar(20) NOT NULL CONSTRAINT [DF_UserProfiles_District] DEFAULT('');
END

IF COL_LENGTH('UserProfiles', 'DistrictName') IS NULL
BEGIN
    ALTER TABLE [UserProfiles] ADD [DistrictName] nvarchar(100) NOT NULL CONSTRAINT [DF_UserProfiles_DistrictName] DEFAULT('');
END

IF COL_LENGTH('UserProfiles', 'Ward') IS NULL
BEGIN
    ALTER TABLE [UserProfiles] ADD [Ward] nvarchar(20) NOT NULL CONSTRAINT [DF_UserProfiles_Ward] DEFAULT('');
END

IF COL_LENGTH('UserProfiles', 'WardName') IS NULL
BEGIN
    ALTER TABLE [UserProfiles] ADD [WardName] nvarchar(100) NOT NULL CONSTRAINT [DF_UserProfiles_WardName] DEFAULT('');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('UserProfiles', 'WardName') IS NOT NULL
BEGIN
    ALTER TABLE [UserProfiles] DROP CONSTRAINT IF EXISTS [DF_UserProfiles_WardName];
    ALTER TABLE [UserProfiles] DROP COLUMN [WardName];
END

IF COL_LENGTH('UserProfiles', 'Ward') IS NOT NULL
BEGIN
    ALTER TABLE [UserProfiles] DROP CONSTRAINT IF EXISTS [DF_UserProfiles_Ward];
    ALTER TABLE [UserProfiles] DROP COLUMN [Ward];
END

IF COL_LENGTH('UserProfiles', 'DistrictName') IS NOT NULL
BEGIN
    ALTER TABLE [UserProfiles] DROP CONSTRAINT IF EXISTS [DF_UserProfiles_DistrictName];
    ALTER TABLE [UserProfiles] DROP COLUMN [DistrictName];
END

IF COL_LENGTH('UserProfiles', 'District') IS NOT NULL
BEGIN
    ALTER TABLE [UserProfiles] DROP CONSTRAINT IF EXISTS [DF_UserProfiles_District];
    ALTER TABLE [UserProfiles] DROP COLUMN [District];
END

IF COL_LENGTH('UserProfiles', 'CityName') IS NOT NULL
BEGIN
    ALTER TABLE [UserProfiles] DROP CONSTRAINT IF EXISTS [DF_UserProfiles_CityName];
    ALTER TABLE [UserProfiles] DROP COLUMN [CityName];
END

IF COL_LENGTH('UserProfiles', 'City') IS NOT NULL
BEGIN
    ALTER TABLE [UserProfiles] DROP CONSTRAINT IF EXISTS [DF_UserProfiles_City];
    ALTER TABLE [UserProfiles] DROP COLUMN [City];
END

IF COL_LENGTH('UserProfiles', 'Address') IS NOT NULL
BEGIN
    ALTER TABLE [UserProfiles] ALTER COLUMN [Address] nvarchar(100) NOT NULL;
END
");
        }
    }
}
