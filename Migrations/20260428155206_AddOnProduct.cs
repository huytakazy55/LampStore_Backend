using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class AddOnProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Column and index may already exist from a previous failed migration attempt
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'AddOnProductId')
                BEGIN
                    ALTER TABLE [Products] ADD [AddOnProductId] uniqueidentifier NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_AddOnProductId' AND object_id = OBJECT_ID('Products'))
                BEGIN
                    CREATE INDEX [IX_Products_AddOnProductId] ON [Products] ([AddOnProductId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_Products_AddOnProductId')
                BEGIN
                    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Products_AddOnProductId] 
                        FOREIGN KEY ([AddOnProductId]) REFERENCES [Products] ([Id]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Products_AddOnProductId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_AddOnProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AddOnProductId",
                table: "Products");
        }
    }
}
