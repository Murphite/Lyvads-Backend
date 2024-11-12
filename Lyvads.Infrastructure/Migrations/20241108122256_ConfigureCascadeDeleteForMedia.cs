using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDeleteForMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_Posts_PostId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Posts_PostId",
                table: "Media",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_Posts_PostId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Posts_PostId",
                table: "Media",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
