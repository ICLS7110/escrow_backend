using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Changes_into_Permission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "RoleMenuPermissions",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "UserDetails",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "UserDetails");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RoleMenuPermissions",
                newName: "RoleId");
        }
    }
}
