using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addUpdateLikeBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_RegularUsers_UserId",
                table: "Likes");

            migrationBuilder.AddColumn<string>(
                name: "LikedBy",
                table: "Likes",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "3d7fb996-cdae-44b0-a3c9-8dd5a414b981",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "9ee27785-b14a-4ab1-b720-f0b574f906aa");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_AspNetUsers_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_AspNetUsers_UserId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "LikedBy",
                table: "Likes");

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "9ee27785-b14a-4ab1-b720-f0b574f906aa",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "3d7fb996-cdae-44b0-a3c9-8dd5a414b981");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_RegularUsers_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "RegularUsers",
                principalColumn: "Id");
        }
    }
}
