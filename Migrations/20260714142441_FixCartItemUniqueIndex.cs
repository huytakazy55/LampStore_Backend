using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class FixCartItemUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_CartItems_ProductId'
                      AND object_id = OBJECT_ID(N'[CartItems]')
                )
                    DROP INDEX [IX_CartItems_ProductId] ON [CartItems];

                CREATE INDEX [IX_CartItems_ProductId] ON [CartItems] ([ProductId]);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_CartItems_ProductId'
                      AND object_id = OBJECT_ID(N'[CartItems]')
                )
                    DROP INDEX [IX_CartItems_ProductId] ON [CartItems];

                CREATE UNIQUE INDEX [IX_CartItems_ProductId]
                    ON [CartItems] ([ProductId])
                    WHERE [ProductId] IS NOT NULL;
                """);
        }
    }
}
