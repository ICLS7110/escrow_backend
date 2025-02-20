using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Modify_RecordState_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "UserDetails",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordState",
                table: "UserDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BankDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "BankDetails",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordState",
                table: "BankDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "RecordState",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BankDetails");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "BankDetails");

            migrationBuilder.DropColumn(
                name: "RecordState",
                table: "BankDetails");
        }
    }
}
