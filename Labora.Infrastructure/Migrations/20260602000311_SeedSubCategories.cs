using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSubCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
    table: "Categories",
    columns: new[] { "Id", "Name", "Description", "IconUrl", "JobType", "ParentCategoryId", "CreatedAt", "UpdatedAt", "IsDeleted" },
    values: new object[] { new Guid("81c83065-de38-4b79-b2cc-e5db3912b82c"), "IT", "Axborot texnologiyalari", null, 0, null, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, false }
);
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "IconUrl", "JobType", "ParentCategoryId", "CreatedAt", "UpdatedAt", "IsDeleted" },
                values: new object[,]
                {
            // IT (81c83065-de38-4b79-b2cc-e5db3912b82c)
            { new Guid("b1000001-0000-0000-0000-000000000001"), "Veb-dasturchi",     "Frontend va backend", null, 0, new Guid("81c83065-de38-4b79-b2cc-e5db3912b82c"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000001-0000-0000-0000-000000000002"), "Mobil dasturchi",   "iOS va Android",      null, 0, new Guid("81c83065-de38-4b79-b2cc-e5db3912b82c"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000001-0000-0000-0000-000000000003"), "1C dasturchi",      "1C konfiguratsiya",   null, 0, new Guid("81c83065-de38-4b79-b2cc-e5db3912b82c"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000001-0000-0000-0000-000000000004"), "Tarmoq muhandisi",  "Network va server",   null, 0, new Guid("81c83065-de38-4b79-b2cc-e5db3912b82c"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000001-0000-0000-0000-000000000005"), "UI/UX dizayner",    "Interfeys dizayn",    null, 0, new Guid("81c83065-de38-4b79-b2cc-e5db3912b82c"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000001-0000-0000-0000-000000000006"), "AI mutaxassis",     "Sun'iy intellekt",    null, 0, new Guid("81c83065-de38-4b79-b2cc-e5db3912b82c"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Construction (11111111-1111-1111-1111-111111111101)
            { new Guid("b1000002-0000-0000-0000-000000000001"), "Usta (umumiy)",     "Qurilish ustasi",     null, 0, new Guid("11111111-1111-1111-1111-111111111101"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000002-0000-0000-0000-000000000002"), "Elektrik",          "Elektr montaj",       null, 0, new Guid("11111111-1111-1111-1111-111111111101"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000002-0000-0000-0000-000000000003"), "Santexnik",         "Suv tizimi",          null, 0, new Guid("11111111-1111-1111-1111-111111111101"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000002-0000-0000-0000-000000000004"), "Bo'yoqchi",         "Devor bo'yash",       null, 0, new Guid("11111111-1111-1111-1111-111111111101"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000002-0000-0000-0000-000000000005"), "Gipschi",           "Gips ishlari",        null, 0, new Guid("11111111-1111-1111-1111-111111111101"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000002-0000-0000-0000-000000000006"), "Kafel ustasi",      "Kafel yotqizish",     null, 0, new Guid("11111111-1111-1111-1111-111111111101"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000002-0000-0000-0000-000000000007"), "Metall konstruksiya","Temir ishlari",      null, 0, new Guid("11111111-1111-1111-1111-111111111101"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Driver (11111111-1111-1111-1111-111111111102)
            { new Guid("b1000003-0000-0000-0000-000000000001"), "Taksi haydovchi",   "Yo'lovchi tashish",   null, 0, new Guid("11111111-1111-1111-1111-111111111102"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000003-0000-0000-0000-000000000002"), "Yuk haydovchi",     "Yuk tashish",         null, 0, new Guid("11111111-1111-1111-1111-111111111102"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000003-0000-0000-0000-000000000003"), "Avtobus haydovchi", "Jamoat transport",    null, 0, new Guid("11111111-1111-1111-1111-111111111102"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000003-0000-0000-0000-000000000004"), "Shaxsiy haydovchi", "VIP xizmat",          null, 0, new Guid("11111111-1111-1111-1111-111111111102"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000003-0000-0000-0000-000000000005"), "Kuryer haydovchi",  "Yetkazib berish",     null, 0, new Guid("11111111-1111-1111-1111-111111111102"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Chef (11111111-1111-1111-1111-111111111103)
            { new Guid("b1000004-0000-0000-0000-000000000001"), "Oshpaz",            "Taom tayyorlash",     null, 0, new Guid("11111111-1111-1111-1111-111111111103"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000004-0000-0000-0000-000000000002"), "Konditer",          "Shirinliklar",        null, 0, new Guid("11111111-1111-1111-1111-111111111103"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000004-0000-0000-0000-000000000003"), "Barista",           "Qahva xizmati",       null, 0, new Guid("11111111-1111-1111-1111-111111111103"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000004-0000-0000-0000-000000000004"), "Ofitsiant",         "Xizmat ko'rsatish",   null, 0, new Guid("11111111-1111-1111-1111-111111111103"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000004-0000-0000-0000-000000000005"), "Sushi master",      "Yapon taomlari",      null, 0, new Guid("11111111-1111-1111-1111-111111111103"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000004-0000-0000-0000-000000000006"), "Tandirchi",         "Non va tandir taom",  null, 0, new Guid("11111111-1111-1111-1111-111111111103"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Medical (11111111-1111-1111-1111-111111111104)
            { new Guid("b1000005-0000-0000-0000-000000000001"), "Vrach",             "Shifokor",            null, 0, new Guid("11111111-1111-1111-1111-111111111104"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000005-0000-0000-0000-000000000002"), "Hamshira",          "Tibbiy yordam",       null, 0, new Guid("11111111-1111-1111-1111-111111111104"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000005-0000-0000-0000-000000000003"), "Dorixonachi",       "Dori xizmati",        null, 0, new Guid("11111111-1111-1111-1111-111111111104"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000005-0000-0000-0000-000000000004"), "Stomatolog",        "Tish shifokori",      null, 0, new Guid("11111111-1111-1111-1111-111111111104"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000005-0000-0000-0000-000000000005"), "Psixolog",          "Ruhiy yordam",        null, 0, new Guid("11111111-1111-1111-1111-111111111104"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000005-0000-0000-0000-000000000006"), "Laborant",          "Tahlil xizmati",      null, 0, new Guid("11111111-1111-1111-1111-111111111104"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Education (11111111-1111-1111-1111-111111111105)
            { new Guid("b1000006-0000-0000-0000-000000000001"), "Maktab o'qituvchi", "Umumta'lim",          null, 0, new Guid("11111111-1111-1111-1111-111111111105"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000006-0000-0000-0000-000000000002"), "Repetitor",         "Xususiy dars",        null, 0, new Guid("11111111-1111-1111-1111-111111111105"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000006-0000-0000-0000-000000000003"), "Ingliz tili o'qituvchi", "Chet tili",      null, 0, new Guid("11111111-1111-1111-1111-111111111105"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000006-0000-0000-0000-000000000004"), "Sport murabbiy",    "Jismoniy tarbiya",    null, 0, new Guid("11111111-1111-1111-1111-111111111105"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000006-0000-0000-0000-000000000005"), "Tarbiyachi",        "Bog'cha",             null, 0, new Guid("11111111-1111-1111-1111-111111111105"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000006-0000-0000-0000-000000000006"), "Online o'qituvchi", "Masofaviy ta'lim",    null, 0, new Guid("11111111-1111-1111-1111-111111111105"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Finance (11111111-1111-1111-1111-111111111106)
            { new Guid("b1000007-0000-0000-0000-000000000001"), "Buxgalter",         "Hisobchi",            null, 0, new Guid("11111111-1111-1111-1111-111111111106"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000007-0000-0000-0000-000000000002"), "1C operator",       "1C buxgalteriya",     null, 0, new Guid("11111111-1111-1111-1111-111111111106"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000007-0000-0000-0000-000000000003"), "Kassir",            "Kassa xizmati",       null, 0, new Guid("11111111-1111-1111-1111-111111111106"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000007-0000-0000-0000-000000000004"), "Moliyaviy tahlilchi","Moliya tahlil",      null, 0, new Guid("11111111-1111-1111-1111-111111111106"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000007-0000-0000-0000-000000000005"), "Auditor",           "Audit xizmati",       null, 0, new Guid("11111111-1111-1111-1111-111111111106"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Security (11111111-1111-1111-1111-111111111107)
            { new Guid("b1000008-0000-0000-0000-000000000001"), "Qorovul",           "Ob'ekt qo'riqlash",   null, 0, new Guid("11111111-1111-1111-1111-111111111107"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000008-0000-0000-0000-000000000002"), "Xavfsizlik xodimi", "Xavfsizlik xizmat",   null, 0, new Guid("11111111-1111-1111-1111-111111111107"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000008-0000-0000-0000-000000000003"), "CCTV operator",     "Kamera nazorat",      null, 0, new Guid("11111111-1111-1111-1111-111111111107"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Cleaning (11111111-1111-1111-1111-111111111108)
            { new Guid("b1000009-0000-0000-0000-000000000001"), "Farrosh",           "Tozalash xizmati",    null, 0, new Guid("11111111-1111-1111-1111-111111111108"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000009-0000-0000-0000-000000000002"), "Tozalash brigadasi","Jamoa tozalash",      null, 0, new Guid("11111111-1111-1111-1111-111111111108"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000009-0000-0000-0000-000000000003"), "Bog'bon",           "Bog' parvarish",      null, 0, new Guid("11111111-1111-1111-1111-111111111108"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000009-0000-0000-0000-000000000004"), "Oyna tozalovchi",   "Oyna va fasad",       null, 0, new Guid("11111111-1111-1111-1111-111111111108"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Design (11111111-1111-1111-1111-111111111109)
            { new Guid("b1000010-0000-0000-0000-000000000001"), "Grafik dizayner",   "Grafik dizayn",       null, 0, new Guid("11111111-1111-1111-1111-111111111109"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000010-0000-0000-0000-000000000002"), "Video montajchi",   "Video tahrirlash",    null, 0, new Guid("11111111-1111-1111-1111-111111111109"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000010-0000-0000-0000-000000000003"), "3D dizayner",       "3D modellash",        null, 0, new Guid("11111111-1111-1111-1111-111111111109"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000010-0000-0000-0000-000000000004"), "Fotograf",          "Foto xizmati",        null, 0, new Guid("11111111-1111-1111-1111-111111111109"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000010-0000-0000-0000-000000000005"), "Kontent yaratuvchi","Kontent ishlab chiqish",null, 0, new Guid("11111111-1111-1111-1111-111111111109"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Marketing (11111111-1111-1111-1111-111111111110)
            { new Guid("b1000011-0000-0000-0000-000000000001"), "SMM mutaxassis",    "Ijtimoiy tarmoq",     null, 0, new Guid("11111111-1111-1111-1111-111111111110"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000011-0000-0000-0000-000000000002"), "SEO mutaxassis",    "Qidiruv optimallashtirish", null, 0, new Guid("11111111-1111-1111-1111-111111111110"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000011-0000-0000-0000-000000000003"), "Targetolog",        "Reklama targeting",   null, 0, new Guid("11111111-1111-1111-1111-111111111110"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000011-0000-0000-0000-000000000004"), "PR mutaxassis",     "Jamoatchilik aloqasi",null, 0, new Guid("11111111-1111-1111-1111-111111111110"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000011-0000-0000-0000-000000000005"), "Brand menejeri",    "Brend boshqaruv",     null, 0, new Guid("11111111-1111-1111-1111-111111111110"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Warehouse (11111111-1111-1111-1111-111111111111)
            { new Guid("b1000012-0000-0000-0000-000000000001"), "Omborchi",          "Ombor boshqaruv",     null, 0, new Guid("11111111-1111-1111-1111-111111111111"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000012-0000-0000-0000-000000000002"), "Yuk ko'taruvchi",   "Jismoniy ish",        null, 0, new Guid("11111111-1111-1111-1111-111111111111"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000012-0000-0000-0000-000000000003"), "Inventarizatsiya",  "Tovar hisobi",        null, 0, new Guid("11111111-1111-1111-1111-111111111111"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000012-0000-0000-0000-000000000004"), "Logist",            "Logistika",           null, 0, new Guid("11111111-1111-1111-1111-111111111111"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Daily (11111111-1111-1111-1111-111111111112)
            { new Guid("b1000013-0000-0000-0000-000000000001"), "Uy yumushi",        "Uy ishlari",          null, 0, new Guid("11111111-1111-1111-1111-111111111112"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000013-0000-0000-0000-000000000002"), "Ko'chirish",        "Uy ko'chirish",       null, 0, new Guid("11111111-1111-1111-1111-111111111112"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000013-0000-0000-0000-000000000003"), "Mayda ta'mir",      "Kichik ta'mir ishlari",null, 0, new Guid("11111111-1111-1111-1111-111111111112"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000013-0000-0000-0000-000000000004"), "Yuk tashish",       "Yuk ko'tarish yordami",null, 0, new Guid("11111111-1111-1111-1111-111111111112"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Trade (a1000001-0000-0000-0000-000000000001)
            { new Guid("b1000014-0000-0000-0000-000000000001"), "Sotuvchi",          "Do'kon sotuvchi",     null, 0, new Guid("a1000001-0000-0000-0000-000000000001"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000014-0000-0000-0000-000000000002"), "Savdo vakili",      "Savdo agenti",        null, 0, new Guid("a1000001-0000-0000-0000-000000000001"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000014-0000-0000-0000-000000000003"), "Kassir",            "Kassa xizmati",       null, 0, new Guid("a1000001-0000-0000-0000-000000000001"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000014-0000-0000-0000-000000000004"), "Online savdo",      "E-commerce",          null, 0, new Guid("a1000001-0000-0000-0000-000000000001"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000014-0000-0000-0000-000000000005"), "Do'kon menejeri",   "Do'kon boshqaruv",    null, 0, new Guid("a1000001-0000-0000-0000-000000000001"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Agriculture (a1000001-0000-0000-0000-000000000002)
            { new Guid("b1000015-0000-0000-0000-000000000001"), "Dehqon",            "Ekin yetishtirish",   null, 0, new Guid("a1000001-0000-0000-0000-000000000002"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000015-0000-0000-0000-000000000002"), "Chorvachilik",      "Mol-qo'y boqish",     null, 0, new Guid("a1000001-0000-0000-0000-000000000002"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000015-0000-0000-0000-000000000003"), "Bog'dorchilik",     "Meva-sabzavot",       null, 0, new Guid("a1000001-0000-0000-0000-000000000002"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000015-0000-0000-0000-000000000004"), "Parrandachilik",    "Parranda boqish",     null, 0, new Guid("a1000001-0000-0000-0000-000000000002"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000015-0000-0000-0000-000000000005"), "Traktorchi",        "Texnika boshqaruv",   null, 0, new Guid("a1000001-0000-0000-0000-000000000002"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Manufacturing (a1000001-0000-0000-0000-000000000003)
            { new Guid("b1000016-0000-0000-0000-000000000001"), "Zavod ishchisi",    "Ishlab chiqarish",    null, 0, new Guid("a1000001-0000-0000-0000-000000000003"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000016-0000-0000-0000-000000000002"), "Mashina operatori", "Uskunalar boshqaruv", null, 0, new Guid("a1000001-0000-0000-0000-000000000003"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000016-0000-0000-0000-000000000003"), "Sifat nazorati",    "QC mutaxassis",       null, 0, new Guid("a1000001-0000-0000-0000-000000000003"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000016-0000-0000-0000-000000000004"), "Elektr montajchi",  "Elektr ishlari",      null, 0, new Guid("a1000001-0000-0000-0000-000000000003"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Courier (a1000001-0000-0000-0000-000000000004)
            { new Guid("b1000017-0000-0000-0000-000000000001"), "Piyoda kuryer",     "Shahar ichida",       null, 0, new Guid("a1000001-0000-0000-0000-000000000004"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000017-0000-0000-0000-000000000002"), "Mototsikl kuryer",  "Tez yetkazish",       null, 0, new Guid("a1000001-0000-0000-0000-000000000004"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000017-0000-0000-0000-000000000003"), "Avtomobil kuryer",  "Uzoq masofa",         null, 0, new Guid("a1000001-0000-0000-0000-000000000004"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000017-0000-0000-0000-000000000004"), "Express yetkazish", "Tezkor xizmat",       null, 0, new Guid("a1000001-0000-0000-0000-000000000004"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Legal (a1000001-0000-0000-0000-000000000005)
            { new Guid("b1000018-0000-0000-0000-000000000001"), "Advokat",           "Huquqiy himoya",      null, 0, new Guid("a1000001-0000-0000-0000-000000000005"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000018-0000-0000-0000-000000000002"), "Yurist",            "Huquqiy maslahat",    null, 0, new Guid("a1000001-0000-0000-0000-000000000005"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000018-0000-0000-0000-000000000003"), "Notarius yordamchi","Notarial xizmat",     null, 0, new Guid("a1000001-0000-0000-0000-000000000005"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000018-0000-0000-0000-000000000004"), "HR-yurist",         "Mehnat huquqi",       null, 0, new Guid("a1000001-0000-0000-0000-000000000005"), DateTime.UtcNow, DateTime.UtcNow, false },

            // HR (a1000001-0000-0000-0000-000000000006)
            { new Guid("b1000019-0000-0000-0000-000000000001"), "HR menejeri",       "Kadrlar boshqaruvi",  null, 0, new Guid("a1000001-0000-0000-0000-000000000006"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000019-0000-0000-0000-000000000002"), "Rekruter",          "Xodim yollash",       null, 0, new Guid("a1000001-0000-0000-0000-000000000006"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000019-0000-0000-0000-000000000003"), "Kadrlar mutaxassisi","Mehnat munosabatlari",null, 0, new Guid("a1000001-0000-0000-0000-000000000006"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Real Estate (a1000001-0000-0000-0000-000000000007)
            { new Guid("b1000020-0000-0000-0000-000000000001"), "Rieltor",           "Ko'chmas mulk savdo", null, 0, new Guid("a1000001-0000-0000-0000-000000000007"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000020-0000-0000-0000-000000000002"), "Mulk bahovchisi",   "Baholash xizmati",    null, 0, new Guid("a1000001-0000-0000-0000-000000000007"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000020-0000-0000-0000-000000000003"), "Ijara menejeri",    "Ijara boshqaruv",     null, 0, new Guid("a1000001-0000-0000-0000-000000000007"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Beauty (a1000001-0000-0000-0000-000000000008)
            { new Guid("b1000021-0000-0000-0000-000000000001"), "Sartarosh",         "Soch kesish",         null, 0, new Guid("a1000001-0000-0000-0000-000000000008"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000021-0000-0000-0000-000000000002"), "Kosmetolog",        "Teri parvarish",      null, 0, new Guid("a1000001-0000-0000-0000-000000000008"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000021-0000-0000-0000-000000000003"), "Manikur/Pedikur",   "Tirnoq parvarish",    null, 0, new Guid("a1000001-0000-0000-0000-000000000008"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000021-0000-0000-0000-000000000004"), "Vizajist",          "Make-up xizmati",     null, 0, new Guid("a1000001-0000-0000-0000-000000000008"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000021-0000-0000-0000-000000000005"), "Massajchi",         "Massaj xizmati",      null, 0, new Guid("a1000001-0000-0000-0000-000000000008"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Auto Service (a1000001-0000-0000-0000-000000000009)
            { new Guid("b1000022-0000-0000-0000-000000000001"), "Avto mexanik",      "Dvigatel ta'mir",     null, 0, new Guid("a1000001-0000-0000-0000-000000000009"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000022-0000-0000-0000-000000000002"), "Shinachi",          "Shina xizmati",       null, 0, new Guid("a1000001-0000-0000-0000-000000000009"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000022-0000-0000-0000-000000000003"), "Avto elektrik",     "Avto elektr tizimi",  null, 0, new Guid("a1000001-0000-0000-0000-000000000009"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000022-0000-0000-0000-000000000004"), "Avto bo'yoqchi",    "Kuzov bo'yash",       null, 0, new Guid("a1000001-0000-0000-0000-000000000009"), DateTime.UtcNow, DateTime.UtcNow, false },

            // Textile (a1000001-0000-0000-0000-000000000010)
            { new Guid("b1000023-0000-0000-0000-000000000001"), "Tikuvchi",          "Kiyim tikish",        null, 0, new Guid("a1000001-0000-0000-0000-000000000010"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000023-0000-0000-0000-000000000002"), "Kashtachi",         "Kashta ishlari",      null, 0, new Guid("a1000001-0000-0000-0000-000000000010"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000023-0000-0000-0000-000000000003"), "Kiyim dizayneri",   "Moda dizayn",         null, 0, new Guid("a1000001-0000-0000-0000-000000000010"), DateTime.UtcNow, DateTime.UtcNow, false },
            { new Guid("b1000023-0000-0000-0000-000000000004"), "Fabrika ishchisi",  "To'qimachilik zavod", null, 0, new Guid("a1000001-0000-0000-0000-000000000010"), DateTime.UtcNow, DateTime.UtcNow, false },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"Categories\" WHERE \"Id\"::text LIKE 'b1000%'");
        }
    }
}
