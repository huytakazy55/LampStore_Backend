using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class EnforceOneVariantPerProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants");

            // Xóa các variant trùng lặp, chỉ giữ lại 1 variant đầu tiên cho mỗi product
            migrationBuilder.Sql(@"
                DELETE pv FROM ProductVariants pv
                INNER JOIN (
                    SELECT ProductId, MIN(Id) AS KeepId
                    FROM ProductVariants
                    WHERE ProductId IS NOT NULL
                    GROUP BY ProductId
                    HAVING COUNT(*) > 1
                ) dup ON pv.ProductId = dup.ProductId AND pv.Id != dup.KeepId;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId",
                unique: true,
                filter: "[ProductId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");
        }
    }
}
