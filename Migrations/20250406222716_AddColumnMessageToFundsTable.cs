using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Banko.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnMessageToFundsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Funds",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Funds");
        }
    }
}
