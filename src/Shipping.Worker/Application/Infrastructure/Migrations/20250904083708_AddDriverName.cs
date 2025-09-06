using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shipping.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "Shipments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "Shipments");
        }
    }
}
