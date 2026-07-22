using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OpenAIKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AWSKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Creativity = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Precision = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Penalty = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    SystemAlerts = table.Column<bool>(type: "bit", nullable: false),
                    WeeklyReport = table.Column<bool>(type: "bit", nullable: false),
                    ApiWarnings = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PlatformSettings",
                columns: new[] { "Id", "AWSKey", "ApiWarnings", "CompanyName", "Creativity", "EmailTemplate", "Industry", "OpenAIKey", "Penalty", "Precision", "SystemAlerts", "UpdatedAt", "WebsiteUrl", "WeeklyReport" },
                values: new object[] { 1, "AKIAIOSFODNN7EXAMPLE", false, "Acme Corporation", 0.70m, "Hi {{first_name}},\n\nWelcome to Acme Corporation! We are excited to have you on board.\n\nTo get started, please log in and configure your profile.\n\nBest,\nThe Team", "Technology", "sk-1234567890abcdefghijklmnopqrstuvwxyz", 0.00m, 0.90m, true, new DateTime(2026, 7, 22, 7, 31, 50, 731, DateTimeKind.Utc).AddTicks(7708), "https://acme.inc", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformSettings");
        }
    }
}
