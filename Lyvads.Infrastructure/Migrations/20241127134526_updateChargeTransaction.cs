using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateChargeTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "ChargeTransactions",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "ChargeTransactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChargeTransactions_TransactionId",
                table: "ChargeTransactions",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeTransactions_Transactions_TransactionId",
                table: "ChargeTransactions",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargeTransactions_Transactions_TransactionId",
                table: "ChargeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ChargeTransactions_TransactionId",
                table: "ChargeTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "ChargeTransactions");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "ChargeTransactions",
                newName: "UserName");
        }
    }
}
