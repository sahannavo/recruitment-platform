using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailApiKeyToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SendGridApiKey",
                table: "PlatformSettings",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PlatformSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SendGridApiKey", "UpdatedAt" },
                values: new object[] { null, new DateTime(2026, 7, 22, 21, 40, 38, 449, DateTimeKind.Utc).AddTicks(9832) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendGridApiKey",
                table: "PlatformSettings");

            migrationBuilder.UpdateData(
                table: "PlatformSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 7, 22, 12, 49, 30, 840, DateTimeKind.Utc).AddTicks(9825));
        }
    }
}
