using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedNewCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
       table: "Categories",
       columns: new[] { "Id", "Name", "Description", "IconUrl", "JobType", "ParentCategoryId", "CreatedAt", "UpdatedAt", "IsDeleted" },
       values: new object[,]
       {
            { new Guid("a1000001-0000-0000-0000-000000000001"), "Trade", "Savdo sohasidagi ishlar", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000002"), "Agriculture", "Qishloq xo'jaligi ishlari", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000003"), "Manufacturing", "Ishlab chiqarish ishlari", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000004"), "Courier", "Kuryer va yetkazib berish", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000005"), "Legal", "Huquqiy xizmatlar", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000006"), "HR", "Kadrlar boshqaruvi", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000007"), "Real Estate", "Ko'chmas mulk", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000008"), "Beauty", "Go'zallik va parvarish", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000009"), "Auto Service", "Avto ta'mir va xizmat", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("a1000001-0000-0000-0000-000000000010"), "Textile", "To'qimachilik va tikuvchilik", null, 0, null, DateTime.UtcNow, DateTime.UtcNow, false },
       });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
        table: "Categories",
        keyColumn: "Id",
        keyValues: new object[]
        {
            new Guid("a1000001-0000-0000-0000-000000000001"),
            new Guid("a1000001-0000-0000-0000-000000000002"),
            new Guid("a1000001-0000-0000-0000-000000000003"),
            new Guid("a1000001-0000-0000-0000-000000000004"),
            new Guid("a1000001-0000-0000-0000-000000000005"),
            new Guid("a1000001-0000-0000-0000-000000000006"),
            new Guid("a1000001-0000-0000-0000-000000000007"),
            new Guid("a1000001-0000-0000-0000-000000000008"),
            new Guid("a1000001-0000-0000-0000-000000000009"),
            new Guid("a1000001-0000-0000-0000-000000000010"),
        });
        }
    }
}
