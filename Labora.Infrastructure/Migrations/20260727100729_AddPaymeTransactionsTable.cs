using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymeTransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymeTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymeTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaymeTransactionTime = table.Column<long>(type: "bigint", nullable: false),
                    RequestedAmountTiyin = table.Column<long>(type: "bigint", nullable: false),
                    MerchantCreateTime = table.Column<long>(type: "bigint", nullable: true),
                    MerchantPerformTime = table.Column<long>(type: "bigint", nullable: true),
                    MerchantCancelTime = table.Column<long>(type: "bigint", nullable: true),
                    InternalStatus = table.Column<int>(type: "integer", nullable: false),
                    PaymeStateCode = table.Column<int>(type: "integer", nullable: true),
                    CancelReason = table.Column<int>(type: "integer", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymeTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymeTransactions_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymeTransactions_PaymentOrderId",
                table: "PaymeTransactions",
                column: "PaymentOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymeTransactions_PaymeTransactionId",
                table: "PaymeTransactions",
                column: "PaymeTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymeTransactions_PaymeTransactionTime",
                table: "PaymeTransactions",
                column: "PaymeTransactionTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymeTransactions");
        }
    }
}
