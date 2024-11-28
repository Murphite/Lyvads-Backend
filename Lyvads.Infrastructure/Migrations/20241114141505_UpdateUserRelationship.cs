using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SuperAdmins_ApplicationUserId",
                table: "SuperAdmins");

            migrationBuilder.DropIndex(
                name: "IX_Admins_ApplicationUserId",
                table: "Admins");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdmins_ApplicationUserId",
                table: "SuperAdmins",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SuperAdmins_ApplicationUserId",
                table: "SuperAdmins");

            migrationBuilder.DropIndex(
                name: "IX_Admins_ApplicationUserId",
                table: "Admins");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdmins_ApplicationUserId",
                table: "SuperAdmins",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId");
        }
    }
}
