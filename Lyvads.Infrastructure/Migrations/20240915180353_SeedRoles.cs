using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the inserted data if rolling back
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValues: new object[] { "Admin", "RegularUser", "Creator", "SuperAdmin" });

        }
    }
}
