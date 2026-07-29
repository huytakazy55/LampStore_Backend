using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LampStoreProjects.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSlugIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: `dotnet ef migrations add` also scaffolded a DropColumn("VideoPath",
            // "News") + AddColumn("VideoPath", "Products") pair here. That was NOT a real
            // pending change — it was fallout from a pre-existing drift in the checked-in
            // ApplicationDbContextModelSnapshot.cs (VideoPath was mis-recorded under the
            // News entity instead of Product, even though the actual 20260724090000_
            // AddProductVideoPath migration correctly added the column to "Products").
            // Applying those column operations against the real database would error
            // (News.VideoPath doesn't exist there; Products.VideoPath already does), so
            // they were removed here. The snapshot itself has been corrected as part of
            // regenerating it via this migration — only the intended change (the unique
            // index on Product.Slug) is applied below.
            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                table: "Products");
        }
    }
}
