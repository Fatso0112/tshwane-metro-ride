using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TshwaneMetroRide.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailOtpVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerifiedAtUtc",
                table: "Passengers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Passengers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE `Passengers`
                SET
                    `IsEmailVerified` = 1,
                    `EmailVerifiedAtUtc` =
                        UTC_TIMESTAMP();
                """);

            migrationBuilder.CreateTable(
                name: "EmailOtpVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VerificationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OtpHash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Purpose = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false),
                    MaximumAttempts = table.Column<int>(type: "int", nullable: false),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PassengerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOtpVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailOtpVerifications_Passengers_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "Passengers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtpVerifications_PassengerId_CreatedAtUtc",
                table: "EmailOtpVerifications",
                columns: new[] { "PassengerId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtpVerifications_PassengerId_IsUsed",
                table: "EmailOtpVerifications",
                columns: new[] { "PassengerId", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtpVerifications_VerificationId",
                table: "EmailOtpVerifications",
                column: "VerificationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailOtpVerifications");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAtUtc",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Passengers");
        }
    }
}
