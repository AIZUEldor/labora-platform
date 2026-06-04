using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Labora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IconUrl", "IsDeleted", "JobType", "Name", "ParentCategoryId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Qurilish", "", false, 5, "Construction", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111102"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Haydovchi", "", false, 5, "Driver", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111103"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Oshpaz", "", false, 5, "Chef", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111104"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tibbiyot", "", false, 5, "Medical", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111105"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ta'lim", "", false, 5, "Education", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111106"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Moliya", "", false, 5, "Finance", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111107"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Qorovul", "", false, 5, "Security", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111108"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tozalik", "", false, 5, "Cleaning", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111109"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dizayn", "", false, 5, "Design", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111110"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Marketing", "", false, 5, "Marketing", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ombor", "", false, 5, "Warehouse", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111103"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111104"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111105"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111106"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111107"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111108"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111109"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111110"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
