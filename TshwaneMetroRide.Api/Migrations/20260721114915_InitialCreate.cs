using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TshwaneMetroRide.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BusRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RouteCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RouteName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Origin = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Destination = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stops = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FareAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusRoutes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Passengers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FullName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passengers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BusCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CardNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Balance = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    LinkedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PassengerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusCards_Passengers_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "Passengers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TransactionType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reference = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BalanceAfter = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BusCardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_BusCards_BusCardId",
                        column: x => x.BusCardId,
                        principalTable: "BusCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "BusRoutes",
                columns: new[] { "Id", "Destination", "FareAmount", "IsActive", "Origin", "RouteCode", "RouteName", "Stops" },
                values: new object[,]
                {
                    { 1, "Pretoria CBD", 25.00m, true, "Soshanguve", "TMR-R001", "Soshanguve to Pretoria CBD", "Soshanguve, Mabopane, Akasia, Pretoria CBD" },
                    { 2, "Pretoria CBD", 20.00m, true, "Centurion", "TMR-R002", "Centurion to Pretoria CBD", "Centurion, Lyttelton, Groenkloof, Pretoria CBD" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusCards_CardNumber",
                table: "BusCards",
                column: "CardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusCards_PassengerId",
                table: "BusCards",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_BusRoutes_RouteCode",
                table: "BusRoutes",
                column: "RouteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_Email",
                table: "Passengers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_BusCardId_CreatedAtUtc",
                table: "WalletTransactions",
                columns: new[] { "BusCardId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_Reference",
                table: "WalletTransactions",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusRoutes");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "BusCards");

            migrationBuilder.DropTable(
                name: "Passengers");
        }
    }
}
