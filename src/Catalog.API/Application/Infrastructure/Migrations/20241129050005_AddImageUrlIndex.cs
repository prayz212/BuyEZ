using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Images_URL",
                table: "Images",
                column: "URL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Images_URL",
                table: "Images");
        }
    }
}
