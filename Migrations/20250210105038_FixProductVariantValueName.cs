using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class FixProductVariantValueName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_productVariantValues_ProductVariant_ProductVariantId",
                table: "productVariantValues");

            migrationBuilder.DropForeignKey(
                name: "FK_productVariantValues_VariantValues_VariantValueId",
                table: "productVariantValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_productVariantValues",
                table: "productVariantValues");

            migrationBuilder.RenameTable(
                name: "productVariantValues",
                newName: "ProductVariantValues");

            migrationBuilder.RenameIndex(
                name: "IX_productVariantValues_VariantValueId",
                table: "ProductVariantValues",
                newName: "IX_ProductVariantValues_VariantValueId");

            migrationBuilder.RenameIndex(
                name: "IX_productVariantValues_ProductVariantId",
                table: "ProductVariantValues",
                newName: "IX_ProductVariantValues_ProductVariantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductVariantValues",
                table: "ProductVariantValues",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantValues_ProductVariant_ProductVariantId",
                table: "ProductVariantValues",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantValues_VariantValues_VariantValueId",
                table: "ProductVariantValues",
                column: "VariantValueId",
                principalTable: "VariantValues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantValues_ProductVariant_ProductVariantId",
                table: "ProductVariantValues");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantValues_VariantValues_VariantValueId",
                table: "ProductVariantValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductVariantValues",
                table: "ProductVariantValues");

            migrationBuilder.RenameTable(
                name: "ProductVariantValues",
                newName: "productVariantValues");

            migrationBuilder.RenameIndex(
                name: "IX_ProductVariantValues_VariantValueId",
                table: "productVariantValues",
                newName: "IX_productVariantValues_VariantValueId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductVariantValues_ProductVariantId",
                table: "productVariantValues",
                newName: "IX_productVariantValues_ProductVariantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_productVariantValues",
                table: "productVariantValues",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_productVariantValues_ProductVariant_ProductVariantId",
                table: "productVariantValues",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_productVariantValues_VariantValues_VariantValueId",
                table: "productVariantValues",
                column: "VariantValueId",
                principalTable: "VariantValues",
                principalColumn: "Id");
        }
    }
}
