using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorAndRegularUserTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Contents_AspNetUsers_UserId",
                table: "Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_AspNetUsers_Id",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_ExclusiveDeals_AspNetUsers_CreatorId",
                table: "ExclusiveDeals");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_AspNetUsers_UserId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_Id",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_CreatorId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_UserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FacebookHandle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HasExclusiveDeal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InstagramHandle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TikTokHandle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwitterHandle",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "210cf4db-a135-4f65-9221-6676cdddf7ca",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "d65b7070-3b2d-46cd-a33f-fceac4fc6a52");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Creators",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FacebookHandle = table.Column<string>(type: "text", nullable: true),
                    InstagramHandle = table.Column<string>(type: "text", nullable: true),
                    TwitterHandle = table.Column<string>(type: "text", nullable: true),
                    TikTokHandle = table.Column<string>(type: "text", nullable: true),
                    HasExclusiveDeal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Creators_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegularUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegularUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegularUsers_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatorId",
                table: "Posts",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_RegularUsers_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "RegularUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Creators_UserId",
                table: "Contents",
                column: "UserId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Creators_Id",
                table: "Deals",
                column: "Id",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExclusiveDeals_Creators_CreatorId",
                table: "ExclusiveDeals",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_RegularUsers_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "RegularUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Creators_CreatorId",
                table: "Requests",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_RegularUsers_UserId",
                table: "Requests",
                column: "UserId",
                principalTable: "RegularUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_RegularUsers_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Creators_UserId",
                table: "Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Creators_Id",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_ExclusiveDeals_Creators_CreatorId",
                table: "ExclusiveDeals");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_RegularUsers_UserId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Creators_CreatorId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_RegularUsers_UserId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Creators");

            migrationBuilder.DropTable(
                name: "RegularUsers");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreatorId",
                table: "Posts");

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "d65b7070-3b2d-46cd-a33f-fceac4fc6a52",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "210cf4db-a135-4f65-9221-6676cdddf7ca");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FacebookHandle",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasExclusiveDeal",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramHandle",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TikTokHandle",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterHandle",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_AspNetUsers_UserId",
                table: "Contents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_AspNetUsers_Id",
                table: "Deals",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExclusiveDeals_AspNetUsers_CreatorId",
                table: "ExclusiveDeals",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_AspNetUsers_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_Id",
                table: "Posts",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_CreatorId",
                table: "Requests",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_UserId",
                table: "Requests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
