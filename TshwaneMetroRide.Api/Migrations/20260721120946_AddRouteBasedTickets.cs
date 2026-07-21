using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TshwaneMetroRide.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteBasedTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TicketNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QrCodeValue = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FareAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PassengerPhone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PurchasedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidUntilUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BusCardId = table.Column<int>(type: "int", nullable: false),
                    BusRouteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_BusCards_BusCardId",
                        column: x => x.BusCardId,
                        principalTable: "BusCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_BusRoutes_BusRouteId",
                        column: x => x.BusRouteId,
                        principalTable: "BusRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BusCardId_PurchasedAtUtc",
                table: "Tickets",
                columns: new[] { "BusCardId", "PurchasedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BusRouteId",
                table: "Tickets",
                column: "BusRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_QrCodeValue",
                table: "Tickets",
                column: "QrCodeValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketNumber",
                table: "Tickets",
                column: "TicketNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
