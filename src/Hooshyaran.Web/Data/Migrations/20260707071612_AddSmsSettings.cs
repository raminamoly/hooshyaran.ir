using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hooshyaran.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmsApiKey",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmsApiUrl",
                table: "SiteSettings",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "https://api.sms.ir/v1/send/verify");

            migrationBuilder.AddColumn<int>(
                name: "SmsMessageTemplateId",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 391212);

            migrationBuilder.AddColumn<int>(
                name: "SmsOtpTemplateId",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 160052);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmsApiKey",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SmsApiUrl",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SmsMessageTemplateId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SmsOtpTemplateId",
                table: "SiteSettings");
        }
    }
}
