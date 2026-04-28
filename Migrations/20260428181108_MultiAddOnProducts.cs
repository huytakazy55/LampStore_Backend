using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class MultiAddOnProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductAddOns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddOnProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAddOns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAddOns_Products_AddOnProductId",
                        column: x => x.AddOnProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAddOns_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddOns_AddOnProductId",
                table: "ProductAddOns",
                column: "AddOnProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddOns_Product_AddOn",
                table: "ProductAddOns",
                columns: new[] { "ProductId", "AddOnProductId" },
                unique: true);

            // Migrate existing AddOnProductId data to new junction table
            migrationBuilder.Sql(@"
                INSERT INTO [ProductAddOns] ([Id], [ProductId], [AddOnProductId], [SortOrder])
                SELECT NEWID(), [Id], [AddOnProductId], 0
                FROM [Products]
                WHERE [AddOnProductId] IS NOT NULL
                AND NOT EXISTS (
                    SELECT 1 FROM [ProductAddOns] pa
                    WHERE pa.[ProductId] = [Products].[Id]
                    AND pa.[AddOnProductId] = [Products].[AddOnProductId]
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAddOns");
        }
    }
}
