using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    public partial class SeedRolesData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "be6c5b47-82d5-4dfb-85b7-23f4bce9ee86",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "5f540d32-3fbd-47e5-9e8b-c319bd80bda8");

            // Insert initial data into AspNetRoles table
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                   { Guid.NewGuid().ToString(), "Admin", "ADMIN", Guid.NewGuid().ToString() },
                   { Guid.NewGuid().ToString(), "RegularUser", "REGULAR_USER", Guid.NewGuid().ToString() },
                   { Guid.NewGuid().ToString(), "Creator", "CREATOR", Guid.NewGuid().ToString() },
                   { Guid.NewGuid().ToString(), "SuperAdmin", "SUPER_ADMIN", Guid.NewGuid().ToString() }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "5f540d32-3fbd-47e5-9e8b-c319bd80bda8",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "be6c5b47-82d5-4dfb-85b7-23f4bce9ee86");

            // Remove the inserted data if rolling back
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValues: new object[] { "Admin", "RegularUser", "Creator", "SuperAdmin" });
        }
    }
}
