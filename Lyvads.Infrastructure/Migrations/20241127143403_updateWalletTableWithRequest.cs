using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateWalletTableWithRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                table: "Wallets",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_RequestId",
                table: "Wallets",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Requests_RequestId",
                table: "Wallets",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Requests_RequestId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_RequestId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Wallets");
        }
    }
}
