using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientManagement.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AliasName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BriefDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SubscriptionType = table.Column<int>(type: "integer", nullable: false),
                    RegisteredProductType = table.Column<int[]>(type: "integer[]", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActivated = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Filename = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    URL = table.Column<string>(type: "text", nullable: false),
                    AltText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AliasName",
                table: "Clients",
                column: "AliasName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ClientId",
                table: "Images",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_URL",
                table: "Images",
                column: "URL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
