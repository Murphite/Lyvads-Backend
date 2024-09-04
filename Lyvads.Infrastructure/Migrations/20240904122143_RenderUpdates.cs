using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyvads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenderUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Requests_RequestId",
                table: "Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Creators_CreatorId",
                table: "Requests");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "WaitlistEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "VerificationRecords",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "VerificationRecords",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "Transactions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Requests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Requests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            //migrationBuilder.AddColumn<int>(
            //    name: "PaymentMethod",
            //    table: "Requests",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RegularUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Notifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RequestId",
                table: "Deals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Deals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Creators",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Contents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RequestId",
                table: "Contents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Comments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PostId",
                table: "Comments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CommentBy",
                table: "Comments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsDeleted",
            //    table: "Comments",
            //    type: "boolean",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<string>(
            //    name: "ParentCommentId",
            //    table: "Comments",
            //    type: "text",
            //    nullable: true);

            //migrationBuilder.AddColumn<DateTimeOffset>(
            //    name: "UpdatedAt",
            //    table: "Comments",
            //    type: "timestamp with time zone",
            //    nullable: false,
            //    defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "0d83084f-34aa-44e9-b44d-71c186f1f479",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "3d7fb996-cdae-44b0-a3c9-8dd5a414b981");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Admins",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            //migrationBuilder.CreateTable(
            //    name: "BankAccounts",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "text", nullable: false),
            //        UserId = table.Column<string>(type: "text", nullable: true),
            //        BankName = table.Column<string>(type: "text", nullable: true),
            //        AccountNumber = table.Column<string>(type: "text", nullable: true),
            //        AccountHolderName = table.Column<string>(type: "text", nullable: true),
            //        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            //        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BankAccounts", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_BankAccounts_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id");
            //    });

            migrationBuilder.CreateTable(
                name: "CollaborationRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    CreatorId = table.Column<string>(type: "text", nullable: true),
                    VideoUrl = table.Column<string>(type: "text", nullable: false),
                    UserResponse = table.Column<string>(type: "text", nullable: false),
                    DisputeReason = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollaborationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollaborationRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollaborationRequests_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id");
                });

            //migrationBuilder.CreateTable(
            //    name: "Transfers",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "text", nullable: false),
            //        UserId = table.Column<string>(type: "text", nullable: false),
            //        Amount = table.Column<decimal>(type: "numeric", nullable: false),
            //        TransferReference = table.Column<string>(type: "text", nullable: false),
            //        Status = table.Column<string>(type: "text", nullable: false),
            //        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            //        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Transfers", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Transfers_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Withdrawals",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "text", nullable: false),
            //        UserId = table.Column<string>(type: "text", nullable: true),
            //        Amount = table.Column<decimal>(type: "numeric", nullable: false),
            //        TransferReference = table.Column<string>(type: "text", nullable: true),
            //        Status = table.Column<int>(type: "integer", nullable: false),
            //        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            //        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Withdrawals", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Withdrawals_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Comments_ParentCommentId",
            //    table: "Comments",
            //    column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_BankAccounts_UserId",
            //    table: "BankAccounts",
            //    column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaborationRequests_CreatorId",
                table: "CollaborationRequests",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaborationRequests_UserId",
                table: "CollaborationRequests",
                column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Transfers_UserId",
            //    table: "Transfers",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Withdrawals_UserId",
            //    table: "Withdrawals",
            //    column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Requests_RequestId",
                table: "Contents",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Creators_CreatorId",
                table: "Requests",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Requests_RequestId",
                table: "Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Creators_CreatorId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "CollaborationRequests");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Withdrawals");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId",
                table: "Comments");

            //migrationBuilder.DropColumn(
            //    name: "PaymentMethod",
            //    table: "Requests");

            //migrationBuilder.DropColumn(
            //    name: "IsDeleted",
            //    table: "Comments");

            //migrationBuilder.DropColumn(
            //    name: "ParentCommentId",
            //    table: "Comments");

            //migrationBuilder.DropColumn(
            //    name: "UpdatedAt",
            //    table: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "WaitlistEntries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "VerificationRecords",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "VerificationRecords",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "Transactions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RegularUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Notifications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RequestId",
                table: "Deals",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Deals",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Creators",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Contents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RequestId",
                table: "Contents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Comments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostId",
                table: "Comments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommentBy",
                table: "Comments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WalletId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "3d7fb996-cdae-44b0-a3c9-8dd5a414b981",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "0d83084f-34aa-44e9-b44d-71c186f1f479");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Requests_RequestId",
                table: "Contents",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Creators_CreatorId",
                table: "Requests",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
