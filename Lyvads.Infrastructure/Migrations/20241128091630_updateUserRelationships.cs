using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId1",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId1",
                table: "Creators");

            migrationBuilder.DropForeignKey(
                name: "FK_RegularUsers_AspNetUsers_ApplicationUserId1",
                table: "RegularUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId1",
                table: "SuperAdmins");

            migrationBuilder.DropIndex(
                name: "IX_SuperAdmins_ApplicationUserId1",
                table: "SuperAdmins");

            migrationBuilder.DropIndex(
                name: "IX_RegularUsers_ApplicationUserId1",
                table: "RegularUsers");

            migrationBuilder.DropIndex(
                name: "IX_Creators_ApplicationUserId1",
                table: "Creators");

            migrationBuilder.DropIndex(
                name: "IX_Admins_ApplicationUserId1",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "SuperAdmins");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "RegularUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "Creators");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "Admins");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "SuperAdmins",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "RegularUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "Creators",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "Admins",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdmins_ApplicationUserId1",
                table: "SuperAdmins",
                column: "ApplicationUserId1",
                unique: true,
                filter: "[ApplicationUserId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegularUsers_ApplicationUserId1",
                table: "RegularUsers",
                column: "ApplicationUserId1",
                unique: true,
                filter: "[ApplicationUserId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Creators_ApplicationUserId1",
                table: "Creators",
                column: "ApplicationUserId1",
                unique: true,
                filter: "[ApplicationUserId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_ApplicationUserId1",
                table: "Admins",
                column: "ApplicationUserId1",
                unique: true,
                filter: "[ApplicationUserId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId1",
                table: "Admins",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId1",
                table: "Creators",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegularUsers_AspNetUsers_ApplicationUserId1",
                table: "RegularUsers",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId1",
                table: "SuperAdmins",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
