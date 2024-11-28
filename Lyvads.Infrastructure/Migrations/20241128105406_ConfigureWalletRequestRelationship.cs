using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureWalletRequestRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Requests_RequestId",
                table: "Wallets");

            migrationBuilder.AlterColumn<string>(
                name: "RequestId",
                table: "Wallets",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "WalletId",
                table: "Requests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_WalletId",
                table: "Requests",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Wallets_WalletId",
                table: "Requests",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Requests_RequestId",
                table: "Wallets",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Wallets_WalletId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Requests_RequestId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Requests_WalletId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Requests");

            migrationBuilder.AlterColumn<string>(
                name: "RequestId",
                table: "Wallets",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Requests_RequestId",
                table: "Wallets",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
