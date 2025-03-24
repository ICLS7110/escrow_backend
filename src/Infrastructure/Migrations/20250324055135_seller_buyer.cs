using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seller_buyer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerPhoneNumber",
                table: "SellerBuyerInvitations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerPhoneNumber",
                table: "SellerBuyerInvitations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerPhoneNumber",
                table: "SellerBuyerInvitations");

            migrationBuilder.DropColumn(
                name: "SellerPhoneNumber",
                table: "SellerBuyerInvitations");
        }
    }
}
