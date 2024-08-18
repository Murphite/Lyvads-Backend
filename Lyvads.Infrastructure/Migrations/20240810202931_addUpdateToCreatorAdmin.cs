using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addUpdateToCreatorAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creators_AspNetUsers_Id",
                table: "Creators");

            migrationBuilder.DropForeignKey(
                name: "FK_RegularUsers_AspNetUsers_Id",
                table: "RegularUsers");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "RegularUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "RegularUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "RegularUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RegularUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Creators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Creators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Creators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Creators",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "7bf0e0e8-f21a-4f26-a54e-99f135a30824",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "b078cbab-7622-4065-bcfc-ccf68fa67285");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Admins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Admins",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Admins",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RegularUsers_ApplicationUserId",
                table: "RegularUsers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Creators_ApplicationUserId",
                table: "Creators",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegularUsers_AspNetUsers_ApplicationUserId",
                table: "RegularUsers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Creators_AspNetUsers_ApplicationUserId",
                table: "Creators");

            migrationBuilder.DropForeignKey(
                name: "FK_RegularUsers_AspNetUsers_ApplicationUserId",
                table: "RegularUsers");

            migrationBuilder.DropIndex(
                name: "IX_RegularUsers_ApplicationUserId",
                table: "RegularUsers");

            migrationBuilder.DropIndex(
                name: "IX_Creators_ApplicationUserId",
                table: "Creators");

            migrationBuilder.DropIndex(
                name: "IX_Admins_ApplicationUserId",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "RegularUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RegularUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RegularUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RegularUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Creators");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Creators");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Creators");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Creators");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Admins");

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "b078cbab-7622-4065-bcfc-ccf68fa67285",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "7bf0e0e8-f21a-4f26-a54e-99f135a30824");

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_AspNetUsers_Id",
                table: "Creators",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegularUsers_AspNetUsers_Id",
                table: "RegularUsers",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
