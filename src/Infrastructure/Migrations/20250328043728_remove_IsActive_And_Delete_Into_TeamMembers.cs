using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class remove_IsActive_And_Delete_Into_TeamMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AMLFlaggedTransactions_UserDetails_UserId1",
                table: "AMLFlaggedTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AMLFlaggedTransactions_UserId1",
                table: "AMLFlaggedTransactions");

          

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "AMLFlaggedTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "AMLFlaggedTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AMLFlaggedTransactions_UserId1",
                table: "AMLFlaggedTransactions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AMLFlaggedTransactions_UserDetails_UserId1",
                table: "AMLFlaggedTransactions",
                column: "UserId1",
                principalTable: "UserDetails",
                principalColumn: "Id");
        }
    }
}
