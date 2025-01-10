using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogAPI.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");
        }
    }
}
