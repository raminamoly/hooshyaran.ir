using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hooshyaran.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoRequestNotificationRecipients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "AdminUsers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ReceiveDemoRequestNotifications",
                table: "AdminUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "ReceiveDemoRequestNotifications",
                table: "AdminUsers");
        }
    }
}
