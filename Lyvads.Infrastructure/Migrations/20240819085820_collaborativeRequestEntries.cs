using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class collaborativeRequestEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "07e4ffc7-d5aa-4422-9005-67440b37aff0",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "6b96ac74-8ba3-45b2-bbee-99ea8e94f58f");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "6b96ac74-8ba3-45b2-bbee-99ea8e94f58f",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "07e4ffc7-d5aa-4422-9005-67440b37aff0");
        }
    }
}
