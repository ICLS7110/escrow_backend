using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Changes_Into_Contract_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerPayableAmount",
                table: "ContractDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerPayableAmount",
                table: "ContractDetails",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerPayableAmount",
                table: "ContractDetails");

            migrationBuilder.DropColumn(
                name: "SellerPayableAmount",
                table: "ContractDetails");
        }
    }
}
