using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateUserDeletions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId",
                table: "SuperAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Deals_DealId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_DealId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DealId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId",
                table: "SuperAdmins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId",
                table: "SuperAdmins");

            migrationBuilder.AddColumn<string>(
                name: "DealId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deals_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Deals_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DealId",
                table: "Transactions",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CreatorId",
                table: "Deals",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_RequestId",
                table: "Deals",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SuperAdmins_AspNetUsers_ApplicationUserId",
                table: "SuperAdmins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Deals_DealId",
                table: "Transactions",
                column: "DealId",
                principalTable: "Deals",
                principalColumn: "Id");
        }
    }
}
