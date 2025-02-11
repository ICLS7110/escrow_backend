using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_RecordState_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordState",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "RecordState",
                table: "BankDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecordState",
                table: "UserDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecordState",
                table: "BankDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
