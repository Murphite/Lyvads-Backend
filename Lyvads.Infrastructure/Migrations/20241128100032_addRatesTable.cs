using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addRatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Rate_Creators_CreatorId",
                table: "Rate");

            migrationBuilder.DropForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId",
                table: "SuperAdmins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rate",
                table: "Rate");

            migrationBuilder.RenameTable(
                name: "Rate",
                newName: "Rates");

            migrationBuilder.RenameIndex(
                name: "IX_Rate_CreatorId",
                table: "Rates",
                newName: "IX_Rates_CreatorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rates",
                table: "Rates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rates_Creators_CreatorId",
                table: "Rates",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId",
                table: "SuperAdmins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Rates_Creators_CreatorId",
                table: "Rates");

            migrationBuilder.DropForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId",
                table: "SuperAdmins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rates",
                table: "Rates");

            migrationBuilder.RenameTable(
                name: "Rates",
                newName: "Rate");

            migrationBuilder.RenameIndex(
                name: "IX_Rates_CreatorId",
                table: "Rate",
                newName: "IX_Rate_CreatorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rate",
                table: "Rate",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rate_Creators_CreatorId",
                table: "Rate",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId",
                table: "SuperAdmins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
