using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Escrow.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dispute_changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisputeMessages_Disputes_DisputeId",
                table: "DisputeMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_ContractDetails_ContractDetailsId",
                table: "Disputes");

            migrationBuilder.DropIndex(
                name: "IX_Disputes_ContractDetailsId",
                table: "Disputes");

            migrationBuilder.DropIndex(
                name: "IX_DisputeMessages_DisputeId",
                table: "DisputeMessages");

            migrationBuilder.DropColumn(
                name: "ArbitratorId",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "ContractAmount",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "ContractDetailsId",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "EscrowAmount",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "FeesTaxes",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "ReleaseAmount",
                table: "Disputes");

            migrationBuilder.RenameColumn(
                name: "ReleaseTo",
                table: "Disputes",
                newName: "DisputeReason");

            migrationBuilder.RenameColumn(
                name: "AdminDecision",
                table: "Disputes",
                newName: "DisputeDoc");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Disputes",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "DisputeDescription",
                table: "Disputes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "DisputeDescription",
                table: "Disputes");

            migrationBuilder.RenameColumn(
                name: "DisputeReason",
                table: "Disputes",
                newName: "ReleaseTo");

            migrationBuilder.RenameColumn(
                name: "DisputeDoc",
                table: "Disputes",
                newName: "AdminDecision");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Disputes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArbitratorId",
                table: "Disputes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ContractAmount",
                table: "Disputes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ContractDetailsId",
                table: "Disputes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EscrowAmount",
                table: "Disputes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeesTaxes",
                table: "Disputes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReleaseAmount",
                table: "Disputes",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_ContractDetailsId",
                table: "Disputes",
                column: "ContractDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_DisputeMessages_DisputeId",
                table: "DisputeMessages",
                column: "DisputeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DisputeMessages_Disputes_DisputeId",
                table: "DisputeMessages",
                column: "DisputeId",
                principalTable: "Disputes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_ContractDetails_ContractDetailsId",
                table: "Disputes",
                column: "ContractDetailsId",
                principalTable: "ContractDetails",
                principalColumn: "Id");
        }
    }
}
