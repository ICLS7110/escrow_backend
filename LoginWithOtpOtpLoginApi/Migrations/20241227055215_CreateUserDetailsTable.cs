using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OtpLoginApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessManagerName = table.Column<string>(type: "text", nullable: false),
                    BusinessEmail = table.Column<string>(type: "text", nullable: false),
                    VatId = table.Column<string>(type: "text", nullable: false),
                    ProofOfBusiness = table.Column<byte[]>(type: "bytea", nullable: false),
                    AccountHolderName = table.Column<string>(type: "text", nullable: false),
                    IBANNumber = table.Column<string>(type: "text", nullable: false),
                    BICCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDetails");
        }
    }
}
