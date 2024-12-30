using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtpLoginApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginMethodToUserDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoginMethod",
                table: "UserDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginMethod",
                table: "UserDetails");
        }
    }
}
