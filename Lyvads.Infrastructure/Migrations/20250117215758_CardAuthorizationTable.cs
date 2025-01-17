using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CardAuthorizationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminPermissions_AdminRoles_AdminRoleId1",
                table: "AdminPermissions");

            migrationBuilder.DropIndex(
                name: "IX_AdminPermissions_AdminRoleId1",
                table: "AdminPermissions");

            migrationBuilder.DropColumn(
                name: "AdminRoleId1",
                table: "AdminPermissions");

            migrationBuilder.CreateTable(
                name: "CardAuthorizations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthorizationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryMonth = table.Column<int>(type: "int", nullable: false),
                    ExpiryYear = table.Column<int>(type: "int", nullable: false),
                    Bank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reusable = table.Column<bool>(type: "bit", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardAuthorizations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardAuthorizations");

            migrationBuilder.AddColumn<string>(
                name: "AdminRoleId1",
                table: "AdminPermissions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_AdminRoleId1",
                table: "AdminPermissions",
                column: "AdminRoleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminPermissions_AdminRoles_AdminRoleId1",
                table: "AdminPermissions",
                column: "AdminRoleId1",
                principalTable: "AdminRoles",
                principalColumn: "Id");
        }
    }
}
