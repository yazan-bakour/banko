using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Banko.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateColumnToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "varchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "varchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");
        }
    }
}
