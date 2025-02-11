using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_BankName_Property_To_BankDetails_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "BankDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                table: "BankDetails");
        }
    }
}
