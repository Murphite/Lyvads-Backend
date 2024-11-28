using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Creators_CreatorId",
                table: "Deals");

            migrationBuilder.DropTable(
                name: "Collaborations");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Requests",
                newName: "TotalAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "RequestAmount",
                table: "Requests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Creators_CreatorId",
                table: "Deals",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Creators_CreatorId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "RequestAmount",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Requests",
                newName: "Amount");

            migrationBuilder.CreateTable(
                name: "Collaborations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RegularUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisputeReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiptUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collaborations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collaborations_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Collaborations_RegularUsers_RegularUserId",
                        column: x => x.RegularUserId,
                        principalTable: "RegularUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collaborations_CreatorId",
                table: "Collaborations",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Collaborations_RegularUserId",
                table: "Collaborations",
                column: "RegularUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Creators_CreatorId",
                table: "Deals",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
