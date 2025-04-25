using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Pages_Arabic_column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ar_Description",
                table: "Pages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ar_Description",
                table: "Pages");
        }
    }
}
