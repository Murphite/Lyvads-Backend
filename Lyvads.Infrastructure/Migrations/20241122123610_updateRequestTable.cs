using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Requests_RequestId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MakeRequestStatus",
                table: "Transactions");

            migrationBuilder.AddColumn<bool>(
                name: "TransactionStatus",
                table: "Requests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Requests_RequestId",
                table: "Transactions",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Requests_RequestId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionStatus",
                table: "Requests");

            migrationBuilder.AddColumn<bool>(
                name: "MakeRequestStatus",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Requests_RequestId",
                table: "Transactions",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id");
        }
    }
}
