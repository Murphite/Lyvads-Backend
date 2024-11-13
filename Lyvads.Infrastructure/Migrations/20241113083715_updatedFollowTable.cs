using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedFollowTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follows_AspNetUsers_FollowerId",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Creators_CreatorId",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Follows");

            migrationBuilder.RenameColumn(
                name: "FollowerId",
                table: "Follows",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Follows_FollowerId",
                table: "Follows",
                newName: "IX_Follows_ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_AspNetUsers_ApplicationUserId",
                table: "Follows",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Creators_CreatorId",
                table: "Follows",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follows_AspNetUsers_ApplicationUserId",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Creators_CreatorId",
                table: "Follows");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Follows",
                newName: "FollowerId");

            migrationBuilder.RenameIndex(
                name: "IX_Follows_ApplicationUserId",
                table: "Follows",
                newName: "IX_Follows_FollowerId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Follows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_AspNetUsers_FollowerId",
                table: "Follows",
                column: "FollowerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Creators_CreatorId",
                table: "Follows",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id");
        }
    }
}
