using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Banko.Migrations
{
    /// <inheritdoc />
    public partial class updateSettingsPrefrences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preferences",
                table: "Users");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.AddColumn<int>(
                name: "PreferencesId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Privacy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    HideEmail = table.Column<bool>(type: "boolean", nullable: false),
                    HideBalance = table.Column<bool>(type: "boolean", nullable: false),
                    EnableTwoFactorAuth = table.Column<bool>(type: "boolean", nullable: false),
                    ReceiveMarketingEmails = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privacy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DarkMode = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    DateFormat = table.Column<string>(type: "text", nullable: false),
                    CurrencyDisplay = table.Column<int>(type: "integer", nullable: false),
                    PushNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    TransactionAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    LowBalanceAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    LowBalanceThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    PrivacyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preferences_Privacy_PrivacyId",
                        column: x => x.PrivacyId,
                        principalTable: "Privacy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PreferencesId",
                table: "Users",
                column: "PreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_PrivacyId",
                table: "Preferences",
                column: "PrivacyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Preferences_PreferencesId",
                table: "Users",
                column: "PreferencesId",
                principalTable: "Preferences",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Preferences_PreferencesId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Preferences");

            migrationBuilder.DropTable(
                name: "Privacy");

            migrationBuilder.DropIndex(
                name: "IX_Users_PreferencesId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferencesId",
                table: "Users");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "Preferences",
                table: "Users",
                type: "hstore",
                nullable: true);
        }
    }
}
