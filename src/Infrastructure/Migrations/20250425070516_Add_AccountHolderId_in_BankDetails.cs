using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_AccountHolderId_in_BankDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountHolderId",
                table: "BankDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountHolderId",
                table: "BankDetails");
        }
    }
}
