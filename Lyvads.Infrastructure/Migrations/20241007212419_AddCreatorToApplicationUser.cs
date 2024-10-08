using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators");

            migrationBuilder.DropIndex(
                name: "IX_Creators_ApplicationUserId",
                table: "Creators");

            migrationBuilder.CreateIndex(
                name: "IX_Creators_ApplicationUserId",
                table: "Creators",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators");

            migrationBuilder.DropIndex(
                name: "IX_Creators_ApplicationUserId",
                table: "Creators");

            migrationBuilder.CreateIndex(
                name: "IX_Creators_ApplicationUserId",
                table: "Creators",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
