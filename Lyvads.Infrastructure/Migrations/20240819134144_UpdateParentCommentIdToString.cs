using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParentCommentIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "c049ba92-31df-46cb-8869-ec79146c59d1",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "7338a0e4-ffff-4ea9-b012-ca04f6d4f923");

            migrationBuilder.AddColumn<bool>(
              name: "ParentCommentId",
              table: "Comments",
              type: "text",
              nullable: true,
              defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "7338a0e4-ffff-4ea9-b012-ca04f6d4f923",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "c049ba92-31df-46cb-8869-ec79146c59d1");

            migrationBuilder.AddColumn<bool>(
              name: "ParentCommentId",
              table: "Comments",
              type: "text",
              nullable: true,
              defaultValue: false);
        }
    }
}
