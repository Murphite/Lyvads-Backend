using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeApplicationUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminPermissions_AspNetUsers_ApplicationUserId",
                table: "AdminPermissions");

            migrationBuilder.DropIndex(
                name: "IX_AdminPermissions_ApplicationUserId",
                table: "AdminPermissions");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "AdminPermissions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_ApplicationUserId",
                table: "AdminPermissions",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminPermissions_AspNetUsers_ApplicationUserId",
                table: "AdminPermissions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminPermissions_AspNetUsers_ApplicationUserId",
                table: "AdminPermissions");

            migrationBuilder.DropIndex(
                name: "IX_AdminPermissions_ApplicationUserId",
                table: "AdminPermissions");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "AdminPermissions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_ApplicationUserId",
                table: "AdminPermissions",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminPermissions_AspNetUsers_ApplicationUserId",
                table: "AdminPermissions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
