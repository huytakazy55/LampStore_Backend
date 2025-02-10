using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class CreateProductVariantValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "productVariantValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VariantValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productVariantValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_productVariantValues_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_productVariantValues_VariantValues_VariantValueId",
                        column: x => x.VariantValueId,
                        principalTable: "VariantValues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_productVariantValues_ProductVariantId",
                table: "productVariantValues",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_productVariantValues_VariantValueId",
                table: "productVariantValues",
                column: "VariantValueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "productVariantValues");
        }
    }
}
