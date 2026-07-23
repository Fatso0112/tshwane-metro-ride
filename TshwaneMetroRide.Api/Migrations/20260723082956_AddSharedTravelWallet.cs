using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TshwaneMetroRide.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedTravelWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TravelWalletId",
                table: "WalletTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TravelWalletId",
                table: "BusCards",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TravelWallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Balance = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PassengerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelWallets_Passengers_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "Passengers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_TravelWalletId",
                table: "WalletTransactions",
                column: "TravelWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_BusCards_TravelWalletId",
                table: "BusCards",
                column: "TravelWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelWallets_PassengerId",
                table: "TravelWallets",
                column: "PassengerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelWallets_Status",
                table: "TravelWallets",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_BusCards_TravelWallets_TravelWalletId",
                table: "BusCards",
                column: "TravelWalletId",
                principalTable: "TravelWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_TravelWallets_TravelWalletId",
                table: "WalletTransactions",
                column: "TravelWalletId",
                principalTable: "TravelWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                """
                INSERT INTO `TravelWallets`
                (
                    `Balance`,
                    `Status`,
                    `CreatedAtUtc`,
                    `UpdatedAtUtc`,
                    `PassengerId`
                )
                SELECT
                    COALESCE(SUM(`b`.`Balance`), 0.00),
                    'Active',
                    UTC_TIMESTAMP(),
                    NULL,
                    `p`.`Id`
                FROM `Passengers` AS `p`
                LEFT JOIN `BusCards` AS `b`
                    ON `b`.`PassengerId` = `p`.`Id`
                GROUP BY `p`.`Id`;
                """);

            migrationBuilder.Sql(
                """
                UPDATE `BusCards` AS `b`
                INNER JOIN `TravelWallets` AS `w`
                    ON `w`.`PassengerId` = `b`.`PassengerId`
                SET `b`.`TravelWalletId` = `w`.`Id`;
                """);

            migrationBuilder.Sql(
                """
                UPDATE `WalletTransactions` AS `wt`
                INNER JOIN `BusCards` AS `b`
                    ON `b`.`Id` = `wt`.`BusCardId`
                INNER JOIN `TravelWallets` AS `w`
                    ON `w`.`PassengerId` = `b`.`PassengerId`
                SET `wt`.`TravelWalletId` = `w`.`Id`;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusCards_TravelWallets_TravelWalletId",
                table: "BusCards");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_TravelWallets_TravelWalletId",
                table: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "TravelWallets");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_TravelWalletId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BusCards_TravelWalletId",
                table: "BusCards");

            migrationBuilder.DropColumn(
                name: "TravelWalletId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "TravelWalletId",
                table: "BusCards");
        }
    }
}
