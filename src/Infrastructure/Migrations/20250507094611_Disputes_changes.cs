using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Disputes_changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerNote",
                table: "Disputes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseAmount",
                table: "Disputes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseTo",
                table: "Disputes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerNote",
                table: "Disputes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerNote",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "ReleaseAmount",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "ReleaseTo",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "SellerNote",
                table: "Disputes");
        }
    }
}
