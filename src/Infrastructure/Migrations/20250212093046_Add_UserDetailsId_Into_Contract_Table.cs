using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_UserDetailsId_Into_Contract_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserDetailId",
                table: "ContractDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ContractDetails_UserDetailId",
                table: "ContractDetails",
                column: "UserDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractDetails_UserDetails_UserDetailId",
                table: "ContractDetails",
                column: "UserDetailId",
                principalTable: "UserDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractDetails_UserDetails_UserDetailId",
                table: "ContractDetails");

            migrationBuilder.DropIndex(
                name: "IX_ContractDetails_UserDetailId",
                table: "ContractDetails");

            migrationBuilder.DropColumn(
                name: "UserDetailId",
                table: "ContractDetails");
        }
    }
}
