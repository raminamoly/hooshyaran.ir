using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hooshyaran.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteVisitLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "SiteVisitLogs" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_SiteVisitLogs" PRIMARY KEY AUTOINCREMENT,
                    "VisitorKey" TEXT NOT NULL,
                    "EventType" TEXT NOT NULL,
                    "Path" TEXT NOT NULL,
                    "PageTitle" TEXT NOT NULL,
                    "IpAddress" TEXT NOT NULL,
                    "UserAgent" TEXT NOT NULL,
                    "Browser" TEXT NOT NULL,
                    "Device" TEXT NOT NULL,
                    "Referrer" TEXT NOT NULL,
                    "BlogArticleId" INTEGER NULL,
                    "CreatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_SiteVisitLogs_BlogArticles_BlogArticleId"
                        FOREIGN KEY ("BlogArticleId") REFERENCES "BlogArticles" ("Id") ON DELETE SET NULL
                );
                """);

            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_SiteVisitLogs_BlogArticleId" ON "SiteVisitLogs" ("BlogArticleId");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_SiteVisitLogs_CreatedAt" ON "SiteVisitLogs" ("CreatedAt");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_SiteVisitLogs_EventType" ON "SiteVisitLogs" ("EventType");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_SiteVisitLogs_VisitorKey" ON "SiteVisitLogs" ("VisitorKey");""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteVisitLogs");
        }
    }
}
