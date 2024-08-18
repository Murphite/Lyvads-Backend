using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class madeChangesToCreatorDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_RegularUsers_UserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FacebookHandle",
                table: "Creators");

            migrationBuilder.RenameColumn(
                name: "TwitterHandle",
                table: "Creators",
                newName: "WearBrand");

            migrationBuilder.RenameColumn(
                name: "TikTokHandle",
                table: "Creators",
                newName: "SongAdvert");

            migrationBuilder.RenameColumn(
                name: "InstagramHandle",
                table: "Creators",
                newName: "SimpleAdvert");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Notifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Request",
                table: "Creators",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentBy",
                table: "Comments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegularUserId",
                table: "Comments",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "fb06a4d3-f9ad-4a43-89a4-5dc7c6221549",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "7bf0e0e8-f21a-4f26-a54e-99f135a30824");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_RegularUserId",
                table: "Comments",
                column: "RegularUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_RegularUsers_RegularUserId",
                table: "Comments",
                column: "RegularUserId",
                principalTable: "RegularUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_RegularUsers_RegularUserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_RegularUserId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Request",
                table: "Creators");

            migrationBuilder.DropColumn(
                name: "CommentBy",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "RegularUserId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "WearBrand",
                table: "Creators",
                newName: "TwitterHandle");

            migrationBuilder.RenameColumn(
                name: "SongAdvert",
                table: "Creators",
                newName: "TikTokHandle");

            migrationBuilder.RenameColumn(
                name: "SimpleAdvert",
                table: "Creators",
                newName: "InstagramHandle");

            migrationBuilder.AddColumn<string>(
                name: "FacebookHandle",
                table: "Creators",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "7bf0e0e8-f21a-4f26-a54e-99f135a30824",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "fb06a4d3-f9ad-4a43-89a4-5dc7c6221549");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_RegularUsers_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "RegularUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
