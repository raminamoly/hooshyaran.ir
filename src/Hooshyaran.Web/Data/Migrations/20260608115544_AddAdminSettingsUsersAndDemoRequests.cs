using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hooshyaran.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminSettingsUsersAndDemoRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminUserId",
                table: "BlogArticles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AdminUsers",
                type: "TEXT",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AdminUsers",
                type: "TEXT",
                maxLength: 60,
                nullable: false,
                defaultValue: "SuperAdmin");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AdminUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql("UPDATE AdminUsers SET Role = 'SuperAdmin' WHERE Role IS NULL OR Role = '';");

            migrationBuilder.CreateTable(
                name: "DemoRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    OrganizationName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    NeedArea = table.Column<string>(type: "TEXT", maxLength: 220, nullable: false),
                    PreferredTime = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1400, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    AdminNotes = table.Column<string>(type: "TEXT", maxLength: 1400, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WebsiteUrl = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    SmtpHost = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    SmtpPort = table.Column<int>(type: "INTEGER", nullable: false),
                    SmtpUserName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    SmtpPassword = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FromEmail = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    FromName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    EnableSsl = table.Column<bool>(type: "INTEGER", nullable: false),
                    AdminNotificationEmail = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogArticles_AdminUserId",
                table: "BlogArticles",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequests_CreatedAt",
                table: "DemoRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequests_Status",
                table: "DemoRequests",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogArticles_AdminUsers_AdminUserId",
                table: "BlogArticles",
                column: "AdminUserId",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogArticles_AdminUsers_AdminUserId",
                table: "BlogArticles");

            migrationBuilder.DropTable(
                name: "DemoRequests");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_BlogArticles_AdminUserId",
                table: "BlogArticles");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "BlogArticles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AdminUsers");
        }
    }
}
