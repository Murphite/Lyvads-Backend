using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addColumnIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_RegularUsers_UserId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_UserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserAds");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "AdminPermissions");

            migrationBuilder.AlterColumn<string>(
                name: "RegularUserId",
                table: "Requests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RegularUserId",
                table: "Requests",
                column: "RegularUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_RegularUsers_RegularUserId",
                table: "Requests",
                column: "RegularUserId",
                principalTable: "RegularUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_RegularUsers_RegularUserId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_RegularUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Posts");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserAds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "RegularUserId",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Requests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "AdminPermissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_UserId",
                table: "Requests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_RegularUsers_UserId",
                table: "Requests",
                column: "UserId",
                principalTable: "RegularUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
