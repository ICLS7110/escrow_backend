using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Changes_Into_MilestoneTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MileStoneEscrowAmount",
                table: "MileStones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MileStoneTaxAmount",
                table: "MileStones",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MileStoneEscrowAmount",
                table: "MileStones");

            migrationBuilder.DropColumn(
                name: "MileStoneTaxAmount",
                table: "MileStones");
        }
    }
}
