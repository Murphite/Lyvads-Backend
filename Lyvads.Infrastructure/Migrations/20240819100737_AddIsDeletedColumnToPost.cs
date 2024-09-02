using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedColumnToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "31626c73-5ee4-4e58-9f59-9114d6f9d444",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "e41cc7de-d73b-4e15-a863-2ee2aa67f6ec");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Posts",
                nullable: false,
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
                defaultValue: "e41cc7de-d73b-4e15-a863-2ee2aa67f6ec",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "31626c73-5ee4-4e58-9f59-9114d6f9d444");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Posts",
                nullable: false,
                defaultValue: false);
        }
    }
}
