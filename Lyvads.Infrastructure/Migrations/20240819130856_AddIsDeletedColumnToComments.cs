using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedColumnToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Creators",
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
                defaultValue: "511d9c7f-e7bb-40c6-a910-78a7b3caa14b",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "31626c73-5ee4-4e58-9f59-9114d6f9d444");

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Comments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Creators",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "31626c73-5ee4-4e58-9f59-9114d6f9d444",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "511d9c7f-e7bb-40c6-a910-78a7b3caa14b");

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Comments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
