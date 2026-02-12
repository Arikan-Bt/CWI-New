using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CWI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUsername",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_OrderedAt",
                table: "PurchaseOrders",
                column: "OrderedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedByUsername",
                table: "Orders",
                column: "CreatedByUsername");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_OrderedAt",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedByUsername",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUsername",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
