using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureInheritanceStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "b078cbab-7622-4065-bcfc-ccf68fa67285",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "210cf4db-a135-4f65-9221-6676cdddf7ca");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "210cf4db-a135-4f65-9221-6676cdddf7ca",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "b078cbab-7622-4065-bcfc-ccf68fa67285");
        }
    }
}
