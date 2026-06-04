using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyCategory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IconUrl", "IsDeleted", "JobType", "Name", "ParentCategoryId", "UpdatedAt" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111112"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kunlik ishlar", "", false, 5, "Daily", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111112"));
        }
    }
}
