using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpAbuseProtection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtpAbuseEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IpHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Purpose = table.Column<int>(type: "integer", nullable: true),
                    VerificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpAbuseEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockType = table.Column<int>(type: "integer", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BlockReason = table.Column<int>(type: "integer", nullable: false),
                    ViolationCount = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastViolationAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpBlocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtpAbuseEvents_DeviceHash_EventType_CreatedAt",
                table: "OtpAbuseEvents",
                columns: new[] { "DeviceHash", "EventType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpAbuseEvents_IpHash_EventType_CreatedAt",
                table: "OtpAbuseEvents",
                columns: new[] { "IpHash", "EventType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpAbuseEvents_Phone_EventType_CreatedAt",
                table: "OtpAbuseEvents",
                columns: new[] { "PhoneNumber", "EventType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpBlocks_BlockType_ScopeKey",
                table: "OtpBlocks",
                columns: new[] { "BlockType", "ScopeKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpAbuseEvents");

            migrationBuilder.DropTable(
                name: "OtpBlocks");
        }
    }
}
