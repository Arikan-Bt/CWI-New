using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CWI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVendorPaymentStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VendorInvoiceId",
                table: "VendorPayments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "VendorPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "VendorPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VendorPayments_VendorId",
                table: "VendorPayments",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorPayments_Customers_VendorId",
                table: "VendorPayments",
                column: "VendorId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendorPayments_Customers_VendorId",
                table: "VendorPayments");

            migrationBuilder.DropIndex(
                name: "IX_VendorPayments_VendorId",
                table: "VendorPayments");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "VendorPayments");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "VendorPayments");

            migrationBuilder.AlterColumn<int>(
                name: "VendorInvoiceId",
                table: "VendorPayments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
