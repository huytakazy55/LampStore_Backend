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
            migrationBuilder.AddColumn<Guid>(
                name: "AddOnProductId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_AddOnProductId",
                table: "Products",
                column: "AddOnProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Products_AddOnProductId",
                table: "Products",
                column: "AddOnProductId",
                principalTable: "Products",
                principalColumn: "Id");
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
