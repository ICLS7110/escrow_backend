using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MvcTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            //    table: "AspNetUserClaims");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            //    table: "AspNetUserLogins");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            //    table: "AspNetUserRoles");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            //    table: "AspNetUserTokens");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_ContractDetails_UserDetails_UserDetailId",
            //    table: "ContractDetails");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_MileStone_ContractDetails_ContractId",
            //    table: "MileStone");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_MileStone",
            //    table: "MileStone");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_AspNetUsers",
            //    table: "AspNetUsers");

            //migrationBuilder.RenameTable(
            //    name: "MileStone",
            //    newName: "MileStones");

            //migrationBuilder.RenameTable(
            //    name: "AspNetUsers",
            //    newName: "UserDetail");

            //migrationBuilder.RenameIndex(
            //    name: "IX_MileStone_ContractId",
            //    table: "MileStones",
            //    newName: "IX_MileStones_ContractId");

            //migrationBuilder.AlterColumn<int>(
            //    name: "UserDetailId",
            //    table: "ContractDetails",
            //    type: "integer",
            //    nullable: true,
            //    oldClrType: typeof(int),
            //    oldType: "integer");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_MileStones",
            //    table: "MileStones",
            //    column: "Id");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_UserDetail",
            //    table: "UserDetail",
            //    column: "Id");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUserClaims_UserDetail_UserId",
            //    table: "AspNetUserClaims",
            //    column: "UserId",
            //    principalTable: "UserDetail",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUserLogins_UserDetail_UserId",
            //    table: "AspNetUserLogins",
            //    column: "UserId",
            //    principalTable: "UserDetail",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUserRoles_UserDetail_UserId",
            //    table: "AspNetUserRoles",
            //    column: "UserId",
            //    principalTable: "UserDetail",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUserTokens_UserDetail_UserId",
            //    table: "AspNetUserTokens",
            //    column: "UserId",
            //    principalTable: "UserDetail",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_ContractDetails_UserDetails_UserDetailId",
            //    table: "ContractDetails",
            //    column: "UserDetailId",
            //    principalTable: "UserDetails",
            //    principalColumn: "Id");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_MileStones_ContractDetails_ContractId",
            //    table: "MileStones",
            //    column: "ContractId",
            //    principalTable: "ContractDetails",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_UserDetail_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_UserDetail_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_UserDetail_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_UserDetail_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractDetails_UserDetails_UserDetailId",
                table: "ContractDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_MileStones_ContractDetails_ContractId",
                table: "MileStones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDetail",
                table: "UserDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MileStones",
                table: "MileStones");

            migrationBuilder.RenameTable(
                name: "UserDetail",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "MileStones",
                newName: "MileStone");

            migrationBuilder.RenameIndex(
                name: "IX_MileStones_ContractId",
                table: "MileStone",
                newName: "IX_MileStone_ContractId");

            migrationBuilder.AlterColumn<int>(
                name: "UserDetailId",
                table: "ContractDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MileStone",
                table: "MileStone",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractDetails_UserDetails_UserDetailId",
                table: "ContractDetails",
                column: "UserDetailId",
                principalTable: "UserDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MileStone_ContractDetails_ContractId",
                table: "MileStone",
                column: "ContractId",
                principalTable: "ContractDetails",
                principalColumn: "Id");
        }
    }
}
