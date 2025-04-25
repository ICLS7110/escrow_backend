using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Social_Id_UserDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SocialId",
                table: "UserDetails",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocialId",
                table: "UserDetails");
        }
    }
}
