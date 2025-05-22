using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_paymentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_ContractDetails_ContractDetailsId",
                table: "Disputes");

            migrationBuilder.DropForeignKey(
                name: "FK_SellerBuyerInvitations_ContractDetails_ContractDetailsId",
                table: "SellerBuyerInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_ContractDetails_ContractDetailsId",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_ContractDetailsId",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_SellerBuyerInvitations_ContractDetailsId",
                table: "SellerBuyerInvitations");

            migrationBuilder.DropIndex(
                name: "IX_Disputes_ContractDetailsId",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "ContractDetailsId",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "ContractDetailsId",
                table: "SellerBuyerInvitations");

            migrationBuilder.DropColumn(
                name: "ContractDetailsId",
                table: "Disputes");

            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "Transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "ContractDetailsId",
                table: "TeamMembers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractDetailsId",
                table: "SellerBuyerInvitations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractDetailsId",
                table: "Disputes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_ContractDetailsId",
                table: "TeamMembers",
                column: "ContractDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerBuyerInvitations_ContractDetailsId",
                table: "SellerBuyerInvitations",
                column: "ContractDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_ContractDetailsId",
                table: "Disputes",
                column: "ContractDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_ContractDetails_ContractDetailsId",
                table: "Disputes",
                column: "ContractDetailsId",
                principalTable: "ContractDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SellerBuyerInvitations_ContractDetails_ContractDetailsId",
                table: "SellerBuyerInvitations",
                column: "ContractDetailsId",
                principalTable: "ContractDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_ContractDetails_ContractDetailsId",
                table: "TeamMembers",
                column: "ContractDetailsId",
                principalTable: "ContractDetails",
                principalColumn: "Id");
        }
    }
}
