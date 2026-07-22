using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TshwaneMetroRide.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusCardStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Passengers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "BlockedAtUtc",
                table: "BusCards",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "BusCards",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "BusCards",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "BlockedAtUtc",
                table: "BusCards");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "BusCards");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BusCards");
        }
    }
}
