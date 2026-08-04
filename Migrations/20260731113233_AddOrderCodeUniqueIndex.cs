using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCodeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Orders created before OrderCode existed (migration AddPayOSOrderCode)
            // defaulted to OrderCode = 0, and GenerateOrderCode can in theory collide
            // across two orders placed in the same second. Backfill any duplicate
            // OrderCode values to a fresh, guaranteed-unique value before creating the
            // unique index below — otherwise CreateIndex fails against existing data
            // and this migration (and every migration after it) never applies.
            migrationBuilder.Sql(@"
                ;WITH Ranked AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (PARTITION BY OrderCode ORDER BY OrderDate) AS DupRank
                    FROM Orders
                ),
                ToFix AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (ORDER BY Id) AS FixRank
                    FROM Ranked
                    WHERE DupRank > 1
                )
                UPDATE o
                SET o.OrderCode = 900000000000000 + f.FixRank
                FROM Orders o
                JOIN ToFix f ON o.Id = f.Id;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders");
        }
    }
}
