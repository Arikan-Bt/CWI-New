using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CWI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUsdCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET NOCOUNT ON;

                DECLARE @UsdCurrencyId INT;

                SELECT @UsdCurrencyId = [Id]
                FROM [Currencies]
                WHERE [Code] = 'USD';

                IF (@UsdCurrencyId IS NULL)
                BEGIN
                    INSERT INTO [Currencies] ([Code], [Name], [Symbol], [IsDefault], [IsActive])
                    VALUES ('USD', 'US Dollar', '$', 1, 1);

                    SET @UsdCurrencyId = CAST(SCOPE_IDENTITY() AS INT);
                END

                IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
                    UPDATE [Orders] SET [CurrencyId] = @UsdCurrencyId WHERE [CurrencyId] <> @UsdCurrencyId;

                IF OBJECT_ID(N'[Payments]', N'U') IS NOT NULL
                    UPDATE [Payments] SET [CurrencyId] = @UsdCurrencyId WHERE [CurrencyId] <> @UsdCurrencyId;

                IF OBJECT_ID(N'[VendorInvoices]', N'U') IS NOT NULL
                    UPDATE [VendorInvoices] SET [CurrencyId] = @UsdCurrencyId WHERE [CurrencyId] <> @UsdCurrencyId;

                IF OBJECT_ID(N'[VendorPayments]', N'U') IS NOT NULL
                    UPDATE [VendorPayments] SET [CurrencyId] = @UsdCurrencyId WHERE [CurrencyId] <> @UsdCurrencyId;

                IF OBJECT_ID(N'[ProductPrices]', N'U') IS NOT NULL
                    UPDATE [ProductPrices] SET [CurrencyId] = @UsdCurrencyId WHERE [CurrencyId] <> @UsdCurrencyId;

                IF OBJECT_ID(N'[ProductSalesPrices]', N'U') IS NOT NULL
                    UPDATE [ProductSalesPrices] SET [CurrencyId] = @UsdCurrencyId WHERE [CurrencyId] <> @UsdCurrencyId;

                IF OBJECT_ID(N'[ProductPurchasePrices]', N'U') IS NOT NULL
                    UPDATE [ProductPurchasePrices] SET [CurrencyId] = @UsdCurrencyId WHERE [CurrencyId] <> @UsdCurrencyId;

                IF OBJECT_ID(N'[StockAdjustmentItems]', N'U') IS NOT NULL
                    UPDATE [StockAdjustmentItems]
                    SET [Currency] = 'USD'
                    WHERE [Currency] IS NULL OR LTRIM(RTRIM([Currency])) = '' OR UPPER(LTRIM(RTRIM([Currency]))) <> 'USD';

                UPDATE [Currencies]
                SET
                    [IsDefault] = CASE WHEN [Id] = @UsdCurrencyId THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END,
                    [IsActive] = CASE WHEN [Id] = @UsdCurrencyId THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Veri dönüşümü geri alınamaz; Down adımı bilinçli olarak boş bırakıldı.
        }
    }
}
