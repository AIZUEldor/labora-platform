using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategoryNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubCategoryId",
                table: "Jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SubCategoryId",
                table: "Jobs",
                column: "SubCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Categories_SubCategoryId",
                table: "Jobs",
                column: "SubCategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Categories_SubCategoryId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_SubCategoryId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "Jobs");
        }
    }
}
