using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Contents_ContentId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_RegularUsers_UserId",
                table: "Likes");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Likes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PostId",
                table: "Likes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContentId",
                table: "Likes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CommentId",
                table: "Likes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "9ee27785-b14a-4ab1-b720-f0b574f906aa",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "fb06a4d3-f9ad-4a43-89a4-5dc7c6221549");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Contents_ContentId",
                table: "Likes",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_RegularUsers_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "RegularUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Contents_ContentId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_RegularUsers_UserId",
                table: "Likes");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Likes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostId",
                table: "Likes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentId",
                table: "Likes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommentId",
                table: "Likes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "fb06a4d3-f9ad-4a43-89a4-5dc7c6221549",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "9ee27785-b14a-4ab1-b720-f0b574f906aa");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Contents_ContentId",
                table: "Likes",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_RegularUsers_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "RegularUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
