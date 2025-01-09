using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePermissionDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminRoleId1",
                table: "AdminPermissions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_AdminRoleId1",
                table: "AdminPermissions",
                column: "AdminRoleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminPermissions_AdminRoles_AdminRoleId1",
                table: "AdminPermissions",
                column: "AdminRoleId1",
                principalTable: "AdminRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminPermissions_AdminRoles_AdminRoleId1",
                table: "AdminPermissions");

            migrationBuilder.DropIndex(
                name: "IX_AdminPermissions_AdminRoleId1",
                table: "AdminPermissions");

            migrationBuilder.DropColumn(
                name: "AdminRoleId1",
                table: "AdminPermissions");
        }
    }
}
