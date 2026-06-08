using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkConditionRating",
                table: "Reviews",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkConditionRating",
                table: "Reviews");
        }
    }
}
