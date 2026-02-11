using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace MigrationRunner
{
    class Program
    {
        private const string ConnectionString = "Server=localhost;Database=ArikanCWIDB;User ID=sa;Password=GuchluBirSifre123!;Encrypt=False;TrustServerCertificate=True;Integrated Security=False;";
        private const string SourceServerIp = "10.50.137.2";
        private const string SourceUser = "awc";
        private const string SourcePassword = "Gh10*Ws!";

        static async Task Main(string[] args)
        {
            Console.WriteLine("CWI Veri Aktarım İşlemi Başlatılıyor...");
            Console.WriteLine("------------------------------------------");

                // ADIM 1: Mapping Tabloları
                await ExecuteStepAsync(GetStep1Sql(), "ADIM 1: Migration Mapping Tablolarını Oluşturma");

                // ADIM 2: Linked Server
                await ExecuteStepAsync(GetStep2Sql(), "ADIM 2: Linked Server Oluşturma", false);

                // ADIM 2.5: Languages
                await ExecuteStepAsync(GetStep2_5Sql(), "ADIM 2.5: Language (Dil) Aktarımı");

                // ADIM 3: Currencies
                await ExecuteStepAsync(GetStep3Sql(), "ADIM 3: Currency (Para Birimi) Aktarımı");

                // ADIM 4: Brands
                await ExecuteStepAsync(GetStep4Sql(), "ADIM 4: Brand (Marka) Aktarımı");

                // ADIM 5: Colors
                await ExecuteStepAsync(GetStep5Sql(), "ADIM 5: Color (Renk) Aktarımı");

                // ADIM 6: Warehouses
                await ExecuteStepAsync(GetStep6Sql(), "ADIM 6: Warehouse (Depo) Aktarımı");

                // ADIM 7: PaymentMethods
                await ExecuteStepAsync(GetStep7Sql(), "ADIM 7: PaymentMethods");

                // ADIM 8: Customers
                await ExecuteStepAsync(GetStep8Sql(), "ADIM 8: Customer (Müşteri) Aktarımı");

                // ADIM 9: Products
                await ExecuteStepAsync(GetStep9Sql(), "ADIM 9: Product (Ürün) Aktarımı");

                // ADIM 10: ProductTranslations
                await ExecuteStepAsync(GetStep10Sql(), "ADIM 10: ProductTranslation");

                // ADIM 11: ProductPrices
                await ExecuteStepAsync(GetStep11Sql(), "ADIM 11: ProductPrice");

                // ADIM 12: ProductImages
                await ExecuteStepAsync(GetStep12Sql(), "ADIM 12: ProductImage");

                // ADIM 13: Orders
                await ExecuteStepAsync(GetStep13Sql(), "ADIM 13: Order");

                // ADIM 14: OrderItems
                await ExecuteStepAsync(GetStep14Sql(), "ADIM 14: OrderItem");

                // ADIM 15: OrderShippingInfo
                await ExecuteStepAsync(GetStep15Sql(), "ADIM 15: OrderShippingInfo");

                // ADIM 16: InventoryItem
                await ExecuteStepAsync(GetStep16Sql(), "ADIM 16: InventoryItem");

                // ADIM 17: Payment
                await ExecuteStepAsync(GetStep17Sql(), "ADIM 17: Payment");

                // ADIM 18: CustomerTransaction
                await ExecuteStepAsync(GetStep18Sql(), "ADIM 18: CustomerTransaction");

                // ADIM 19: Role
                await ExecuteStepAsync(GetStep19Sql(), "ADIM 19: Role");

                // ADIM 20: User
                await ExecuteStepAsync(GetStep20Sql(), "ADIM 20: User");

                // ADIM 21: PurchaseOrder
                await ExecuteStepAsync(GetStep21Sql(), "ADIM 21: PurchaseOrder");

                // ADIM 22: PurchaseOrderItem
                await ExecuteStepAsync(GetStep22Sql(), "ADIM 22: PurchaseOrderItem");

                // ADIM 23: VendorInvoice
                await ExecuteStepAsync(GetStep23Sql(), "ADIM 23: VendorInvoice");

                // ADIM 24: VendorPayment
                await ExecuteStepAsync(GetStep24Sql(), "ADIM 24: VendorPayment");

                // ADIM 25: Update VendorInvoices
                await ExecuteStepAsync(GetStep25Sql(), "ADIM 25: Update VendorInvoices");

                Console.WriteLine("\n------------------------------------------");
                Console.WriteLine("TÜM AKTARIM İŞLEMLERİ BAŞARIYLA TAMAMLANDI.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n!!! KRİTİK HATA: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }

        private static async Task ExecuteStepAsync(string sql, string description, bool useTransaction = true)
        {
            Console.Write($"{description}...");
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                SqlTransaction? transaction = null;
                if (useTransaction)
                {
                    transaction = connection.BeginTransaction();
                }

                try
                {
                    var commands = sql.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var cmdText in commands)
                    {
                        if (string.IsNullOrWhiteSpace(cmdText)) continue;
                        using (var command = new SqlCommand(cmdText, connection, transaction))
                        {
                            command.CommandTimeout = 300; // 5 dakika
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    if (useTransaction && transaction != null)
                    {
                        transaction.Commit();
                    }
                    Console.WriteLine(" [OK]");
                }
                catch (Exception)
                {
                    if (useTransaction && transaction != null)
                    {
                        transaction.Rollback();
                    }
                    throw;
                }
            }
        }

        private static async Task QueryAndPrintAsync(string sql)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write($"{reader.GetName(i)}\t");
                    }
                    Console.WriteLine("\n" + new string('-', 50));

                    while (await reader.ReadAsync())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader.GetName(i)}: {reader.GetValue(i)}\t");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        #region SQL Script Helpers

        private static string GetStep1Sql() => @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_Customers]') AND type in (N'U'))
CREATE TABLE MigrationMapping_Customers (OldCode VARCHAR(30) PRIMARY KEY, NewId INT NOT NULL);

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_Products]') AND type in (N'U'))
CREATE TABLE MigrationMapping_Products (OldSku VARCHAR(30) PRIMARY KEY, NewId INT NOT NULL);

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_Orders]') AND type in (N'U'))
CREATE TABLE MigrationMapping_Orders (OldLineId INT PRIMARY KEY, NewId BIGINT NOT NULL);

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_Brands]') AND type in (N'U'))
CREATE TABLE MigrationMapping_Brands (OldCode VARCHAR(20) PRIMARY KEY, NewId INT NOT NULL);

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_Colors]') AND type in (N'U'))
CREATE TABLE MigrationMapping_Colors (OldCode VARCHAR(20) PRIMARY KEY, NewId INT NOT NULL);

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_Warehouses]') AND type in (N'U'))
CREATE TABLE MigrationMapping_Warehouses (OldCode INT PRIMARY KEY, NewId INT NOT NULL);

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_Users]') AND type in (N'U'))
CREATE TABLE MigrationMapping_Users (OldGroupCode VARCHAR(30), OldUserCode VARCHAR(20), NewId INT NOT NULL, PRIMARY KEY (OldGroupCode, OldUserCode));

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_PurchaseOrders]') AND type in (N'U'))
CREATE TABLE MigrationMapping_PurchaseOrders (OldRecId VARCHAR(50) PRIMARY KEY, NewId BIGINT NOT NULL);

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationMapping_VendorInvoices]') AND type in (N'U'))
CREATE TABLE MigrationMapping_VendorInvoices (OldId INT PRIMARY KEY, NewId INT NOT NULL);
";

        private static string GetStep2Sql() => $@"
IF EXISTS (SELECT srv.name FROM sys.servers srv WHERE srv.name = 'CWI_SOURCE')
    EXEC sp_dropserver 'CWI_SOURCE', 'droplogins';

EXEC sp_addlinkedserver @server = 'CWI_SOURCE', @srvproduct = '', @provider = 'SQLNCLI', @datasrc = '{SourceServerIp}';
EXEC sp_addlinkedsrvlogin @rmtsrvname = 'CWI_SOURCE', @useself = 'FALSE', @locallogin = NULL, @rmtuser = '{SourceUser}', @rmtpassword = '{SourcePassword}';
";

        private static string GetStep2_5Sql() => @"
INSERT INTO Languages (Code, Name, NativeName, IsDefault, SortOrder, IsActive)
SELECT LTRIM(RTRIM(LangCode)), LTRIM(RTRIM(LangDescription)), LTRIM(RTRIM(LangDescription)), 
    CASE WHEN LangCode = 'EN' THEN 1 ELSE 0 END, 1, 1
FROM [CWI_SOURCE].[CWI].[dbo].[cdAppLanguage]
WHERE NOT EXISTS (SELECT 1 FROM Languages WHERE Code = LTRIM(RTRIM(LangCode)) COLLATE DATABASE_DEFAULT);
";

        private static string GetStep3Sql() => @"
SET IDENTITY_INSERT Currencies ON;
INSERT INTO Currencies (Id, Code, Name, Symbol, IsDefault, IsActive)
SELECT Id, LTRIM(RTRIM(Currency)), LTRIM(RTRIM(CurrencyName)), 
    CASE WHEN Currency = 'TRY' THEN '₺' WHEN Currency = 'USD' THEN '$' WHEN Currency = 'EUR' THEN '€' WHEN Currency = 'GBP' THEN '£' ELSE Currency END,
    CASE WHEN Currency = 'TRY' THEN 1 ELSE 0 END, 1
FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrency] src
WHERE NOT EXISTS (SELECT 1 FROM Currencies WHERE Id = src.Id);
SET IDENTITY_INSERT Currencies OFF;
";

        private static string GetStep4Sql() => @"
INSERT INTO Brands (Code, Name, LogoUrl, IsActive, SortOrder, CreatedAt)
OUTPUT inserted.Id, INSERTED.Code INTO MigrationMapping_Brands(NewId, OldCode)
SELECT LTRIM(RTRIM(BrandCode)), LTRIM(RTRIM(BrandDescription)), NULL, 1, ROW_NUMBER() OVER (ORDER BY BrandCode), GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[cdBrand]
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_Brands WHERE OldCode = LTRIM(RTRIM(BrandCode)) COLLATE DATABASE_DEFAULT);
";

        private static string GetStep5Sql() => @"
INSERT INTO Colors (Code, Name, HexCode, IsActive, CreatedAt, SortOrder)
OUTPUT inserted.Id, INSERTED.Code INTO MigrationMapping_Colors(NewId, OldCode)
SELECT LTRIM(RTRIM(ColorCode)), LTRIM(RTRIM(ColorCode)), LTRIM(RTRIM(ColorHexCode)), 1, CreateDate, ROW_NUMBER() OVER (ORDER BY ColorCode)
FROM [CWI_SOURCE].[CWI].[dbo].[cdColor]
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_Colors WHERE OldCode = LTRIM(RTRIM(ColorCode)) COLLATE DATABASE_DEFAULT);

INSERT INTO ColorTranslations (ColorId, LanguageId, Name)
SELECT cm.NewId, lang.Id, LTRIM(RTRIM(cd.ColorDescription))
FROM [CWI_SOURCE].[CWI].[dbo].[cdColorDescription] cd
INNER JOIN MigrationMapping_Colors cm ON cd.ColorCode = cm.OldCode COLLATE DATABASE_DEFAULT
INNER JOIN Languages lang ON lang.Code = LTRIM(RTRIM(cd.LangCode)) COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM ColorTranslations WHERE ColorId = cm.NewId AND LanguageId = lang.Id);
";

        private static string GetStep6Sql() => @"
INSERT INTO Warehouses (Code, Name, Address, IsActive, IsDefault, CreatedAt)
OUTPUT inserted.Id, INSERTED.Code INTO MigrationMapping_Warehouses(NewId, OldCode)
SELECT CAST(WareHouseCode AS VARCHAR(20)), LTRIM(RTRIM(WareHouseDescription)), NULL, 1, CASE WHEN WareHouseCode = 1 THEN 1 ELSE 0 END, GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[cdWareHouse]
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_Warehouses WHERE OldCode = WareHouseCode);
";

        private static string GetStep7Sql() => @"
SET IDENTITY_INSERT PaymentMethods ON;
INSERT INTO PaymentMethods (Id, Code, Name, IsActive, CreatedAt, SortOrder)
SELECT LineID, LTRIM(RTRIM(PaymentDescription)), LTRIM(RTRIM(PaymentDescription)), 1, GETDATE(), ROW_NUMBER() OVER (ORDER BY LineID)
FROM [CWI_SOURCE].[CWI].[dbo].[cdPaymentMethod]
WHERE NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Id = LineID);
SET IDENTITY_INSERT PaymentMethods OFF;
";

        private static string GetStep8Sql() => @"
INSERT INTO Customers (Code, Name, TaxOfficeName, TaxNumber, RegionCode, RegionName, AddressLine1, AddressLine2, District, Town, City, Country, PrimaryPhone, SecondaryPhone, Email, IsActive, CreatedAt)
OUTPUT inserted.Id, INSERTED.Code INTO MigrationMapping_Customers(NewId, OldCode)
SELECT LTRIM(RTRIM(CurrAccCode)), LTRIM(RTRIM(CurrAccDescription)), LTRIM(RTRIM(TaxOffice)), LTRIM(RTRIM(TaxNumber)), LTRIM(RTRIM(CurrAccRegionCode)), LTRIM(RTRIM(CurrAccRegionName)), LTRIM(RTRIM(StreetName1)), LTRIM(RTRIM(StreetName2)), LTRIM(RTRIM(Block)), LTRIM(RTRIM(DistrictName)), LTRIM(RTRIM(City)), LTRIM(RTRIM(Country)), LTRIM(RTRIM(Phone1)), LTRIM(RTRIM(Phone2)), NULL, 1, GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAcc]
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_Customers WHERE OldCode = LTRIM(RTRIM(CurrAccCode)) COLLATE DATABASE_DEFAULT);
";

        private static string GetStep9Sql() => @"
INSERT INTO Products (Sku, Name, ColorId, BrandId, IsPreOrder, PreOrderQuantity, IsActive, CreatedAt, Attributes)
OUTPUT inserted.Id, INSERTED.Sku INTO MigrationMapping_Products(NewId, OldSku)
SELECT LTRIM(RTRIM(i.ItemCode)), COALESCE(LTRIM(RTRIM(d.ItemDescription)), LTRIM(RTRIM(i.ItemCode))), cm.NewId, bm.NewId, 0, NULL, 1, GETDATE(),
    (SELECT i.ItemAttribute3 AS Attr3, i.ItemAttribute4 AS Attr4, i.ItemAttribute5 AS Attr5, i.ItemAttribute7 AS Attr7, i.ItemAttribute8 AS Attr8, i.ItemAttribute9 AS Attr9, i.ItemAttribute10 AS Attr10, i.ItemAttribute11 AS Attr11, i.ItemAttribute12 AS Attr12, i.ItemAttribute13 AS Attr13, i.ItemAttribute14 AS Attr14, i.ItemAttribute15 AS Attr15 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
FROM [CWI_SOURCE].[CWI].[dbo].[cdItem] i
LEFT JOIN [CWI_SOURCE].[CWI].[dbo].[cdItemDesc] d ON i.ItemCode = d.ItemCode AND d.LangCode = 'EN' COLLATE DATABASE_DEFAULT
LEFT JOIN MigrationMapping_Brands bm ON i.ItemAttribute6 = bm.OldCode COLLATE DATABASE_DEFAULT
LEFT JOIN MigrationMapping_Colors cm ON i.ColorCode = cm.OldCode COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_Products WHERE OldSku = LTRIM(RTRIM(i.ItemCode)) COLLATE DATABASE_DEFAULT);

INSERT INTO Products (Sku, Name, ColorId, BrandId, IsPreOrder, PreOrderQuantity, IsActive, CreatedAt, Attributes)
OUTPUT inserted.Id, INSERTED.Sku INTO MigrationMapping_Products(NewId, OldSku)
SELECT LTRIM(RTRIM(p.ItemCode)), LTRIM(RTRIM(p.ItemDescription)), cm.NewId, bm.NewId, 1, p.AvailableQty, 1, GETDATE(),
    (SELECT p.ItemAttribute3 AS Attr3, p.ItemAttribute4 AS Attr4, p.ItemAttribute5 AS Attr5 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
FROM [CWI_SOURCE].[CWI].[dbo].[cdItemPreOrder] p
LEFT JOIN MigrationMapping_Brands bm ON p.ItemAttribute6 = bm.OldCode COLLATE DATABASE_DEFAULT
LEFT JOIN MigrationMapping_Colors cm ON p.ColorCode = cm.OldCode COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM [CWI_SOURCE].[CWI].[dbo].[cdItem] i WHERE i.ItemCode = p.ItemCode COLLATE DATABASE_DEFAULT)
AND NOT EXISTS (SELECT 1 FROM MigrationMapping_Products WHERE OldSku = LTRIM(RTRIM(p.ItemCode)) COLLATE DATABASE_DEFAULT);
";

        private static string GetStep10Sql() => @"
INSERT INTO ProductTranslations (ProductId, LanguageId, Name, Description)
SELECT pm.NewId, lang.Id, LTRIM(RTRIM(d.ItemDescription)), NULL
FROM [CWI_SOURCE].[CWI].[dbo].[cdItemDesc] d
INNER JOIN MigrationMapping_Products pm ON d.ItemCode = pm.OldSku COLLATE DATABASE_DEFAULT
INNER JOIN Languages lang ON lang.Code = LTRIM(RTRIM(d.LangCode)) COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM ProductTranslations WHERE ProductId = pm.NewId AND LanguageId = lang.Id);
";

        private static string GetStep11Sql() => @"
INSERT INTO ProductPrices (ProductId, BrandId, UnitPrice, CurrencyId, ValidFrom, ValidTo, IsActive, CreatedAt)
SELECT pm.NewId, NULL, pl.Price, c.Id, GETDATE(), NULL, 1, GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[PriceList] pl
INNER JOIN MigrationMapping_Products pm ON pl.ItemCode = pm.OldSku COLLATE DATABASE_DEFAULT
INNER JOIN Currencies c ON pl.Currency = c.Code COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM ProductPrices WHERE ProductId = pm.NewId AND CurrencyId = c.Id);
";

        private static string GetStep12Sql() => @"
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsPrimary, CreatedAt, IsActive)
SELECT pm.NewId, img.URL, ROW_NUMBER() OVER (PARTITION BY img.ItemCode ORDER BY img.ID), CASE WHEN ROW_NUMBER() OVER (PARTITION BY img.ItemCode ORDER BY img.ID) = 1 THEN 1 ELSE 0 END, GETDATE(), 1
FROM [CWI_SOURCE].[CWI].[dbo].[cdImageUrl] img
INNER JOIN MigrationMapping_Products pm ON img.ItemCode = pm.OldSku COLLATE DATABASE_DEFAULT
WHERE img.URL IS NOT NULL AND img.URL <> ''
AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = pm.NewId AND ImageUrl = img.URL COLLATE DATABASE_DEFAULT);
";

        private static string GetStep13Sql() => @"
-- 1. Siparişleri aktar
INSERT INTO Orders (OrderNumber, OrderedAt, CustomerId, SalesRepresentative, TotalQuantity, SubTotal, TotalDiscount, TaxableAmount, GrandTotal, Status, IsCompleted, IsApproved, IsCanceled, CancellationReason, Notes, IsPreOrder, CurrencyId, CreatedByGroupCode, CreatedByUsername, CreatedAt, ShippedAt)
SELECT h.OrderRefNo, h.OrderDate, cm.NewId, LTRIM(RTRIM(h.SalesManPerson)), h.TotalQty, h.TotalAmount, h.TotalDicount, h.TaxBase, h.NetAmount, 
    CASE WHEN h.IsCancelled = 1 THEN 3 WHEN h.StatusCode = -1 THEN -1 WHEN h.StatusCode = 0 THEN 0 WHEN h.StatusCode = 1 THEN 1 WHEN h.StatusCode = 2 THEN 2 ELSE h.StatusCode END,
    h.IsCompleated, h.IsApproved, h.IsCancelled, LTRIM(RTRIM(h.CancelReason)), LTRIM(RTRIM(h.OrderNote)), 0, 1, LTRIM(RTRIM(h.CreatedGroupCode)), LTRIM(RTRIM(h.CreatedUserName)), h.CreatedDate, CASE WHEN h.ShippedDate = '1900-01-01' THEN NULL ELSE h.ShippedDate END
FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader] h
INNER JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(h.CurrAccCode)) = cm.OldCode COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM Orders WHERE OrderNumber = h.OrderRefNo COLLATE DATABASE_DEFAULT);

-- 2. Mapping tablosunu doldur
INSERT INTO MigrationMapping_Orders (OldLineId, NewId)
SELECT h.LineID, o.Id
FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader] h
INNER JOIN Orders o ON h.OrderRefNo = o.OrderNumber COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_Orders WHERE OldLineId = h.LineID);
";

        private static string GetStep14Sql() => @"
INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice, DiscountAmount, LineTotal, TaxRate, TaxAmount, TaxableAmount, NetTotal, WarehouseId, Notes, CreatedByUsername, CreatedAt)
SELECT om.NewId, pm.NewId, LTRIM(RTRIM(l.ItemDescription)), l.Qty, l.Price, l.Discount, l.Amount, l.VatPercent, l.VatBase, l.TaxBase, l.NetAmount, 
    COALESCE(wm.NewId, (SELECT TOP 1 NewId FROM MigrationMapping_Warehouses WHERE OldCode = 1), (SELECT TOP 1 NewId FROM MigrationMapping_Warehouses)), 
    LTRIM(RTRIM(l.LineNote)), LTRIM(RTRIM(l.CreatedUserName)), l.CreatedDate
FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartLine] l
INNER JOIN MigrationMapping_Orders om ON l.MasterLineID = om.OldLineId
INNER JOIN MigrationMapping_Products pm ON LTRIM(RTRIM(l.ItemCode)) = pm.OldSku COLLATE DATABASE_DEFAULT
LEFT JOIN MigrationMapping_Warehouses wm ON TRY_CAST(l.WareHouseCode AS INT) = wm.OldCode
WHERE NOT EXISTS (SELECT 1 FROM OrderItems WHERE OrderId = om.NewId AND ProductId = pm.NewId AND CreatedAt = l.CreatedDate);
";

        private static string GetStep15Sql() => @"
INSERT INTO OrderShippingInfos (OrderId, ShippingAddress, PaymentMethod, ShipmentTerms, AdditionalDiscount)
SELECT OrderId, ShippingAddress, PaymentMethod, ShipmentTerms, AdditionalDiscount
FROM (
    SELECT om.NewId AS OrderId, LTRIM(RTRIM(d.OrderAddress)) AS ShippingAddress, LTRIM(RTRIM(d.OrderPaymentMethod)) AS PaymentMethod, LTRIM(RTRIM(d.OrderShipmentTerms)) AS ShipmentTerms, d.ExtraDiscount AS AdditionalDiscount,
    ROW_NUMBER() OVER (PARTITION BY om.NewId ORDER BY d.LineID DESC) as rn
    FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartDetail] d
    INNER JOIN MigrationMapping_Orders om ON d.MasterLineID = om.OldLineId
) t
WHERE rn = 1
AND NOT EXISTS (SELECT 1 FROM OrderShippingInfos WHERE OrderId = t.OrderId);
";

        private static string GetStep16Sql() => @"
INSERT INTO InventoryItems (WarehouseId, ProductId, QuantityOnHand, QuantityReserved, ReorderLevel, LastStockTakeAt, UpdatedAt)
SELECT wm.NewId, pm.NewId, w.Qty, 
    ISNULL((SELECT SUM(l.Qty) FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartLine] l INNER JOIN [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader] h ON l.MasterLineID = h.LineID WHERE LTRIM(RTRIM(l.ItemCode)) = LTRIM(RTRIM(w.ItemCode)) COLLATE DATABASE_DEFAULT AND h.StatusCode IN (-1, 0, 1)), 0),
    NULL, NULL, GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[trWareHouseItems] w
INNER JOIN MigrationMapping_Warehouses wm ON w.WareHouseCode = wm.OldCode
INNER JOIN MigrationMapping_Products pm ON LTRIM(RTRIM(w.ItemCode)) = pm.OldSku COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM InventoryItems WHERE WarehouseId = wm.NewId AND ProductId = pm.NewId);
";

        private static string GetStep17Sql() => @"
INSERT INTO Payments (CustomerId, OrderId, Amount, CurrencyId, ReceiptNumber, PaidAt, PaymentMethodId, Status, Notes, CreatedAt, CreatedByUsername)
SELECT cm.NewId, om.NewId, p.PaymentTotal, COALESCE(p.PaymentCurrencyId, 1), LTRIM(RTRIM(p.ReceiptNumber)), COALESCE(p.PaymentDate, GETDATE()), 
    (SELECT TOP 1 Id FROM PaymentMethods ORDER BY Id), 
    1, NULL, COALESCE(p.PaymentDate, GETDATE()), NULL
FROM [CWI_SOURCE].[CWI].[dbo].[cdPayment] p
LEFT JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(p.PaymentCurrAccCode)) = cm.OldCode COLLATE DATABASE_DEFAULT
LEFT JOIN MigrationMapping_Orders om ON TRY_CAST(p.LineId AS INT) = om.OldLineId
WHERE NOT EXISTS (SELECT 1 FROM Payments WHERE CustomerId = cm.NewId AND PaidAt = COALESCE(p.PaymentDate, GETDATE()) AND Amount = p.PaymentTotal);
";

        private static string GetStep18Sql() => @"
INSERT INTO CustomerTransactions (CustomerId, TransactionType, TransactionDate, ReferenceNumber, Description, DocumentType, ApplicationReference, DebitAmount, CreditAmount, Balance, CreatedAt)
SELECT cm.NewId, b.RecType, b.RecDate, LTRIM(RTRIM(b.RecRefNo)), LTRIM(RTRIM(b.RecDescription)), LTRIM(RTRIM(b.RecTransType)), LTRIM(RTRIM(b.RecAppRefNo)), CAST(b.RecDebit AS DECIMAL(18,4)), CAST(b.RecCredit AS DECIMAL(18,4)), b.RecBalance, GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAccBalance] b
INNER JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(b.RecCurrAccCode)) = cm.OldCode COLLATE DATABASE_DEFAULT
WHERE b.RecCurrAccCode IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM CustomerTransactions 
    WHERE CustomerId = cm.NewId 
    AND TransactionDate = b.RecDate 
    AND ISNULL(ReferenceNumber, '') = ISNULL(LTRIM(RTRIM(b.RecRefNo)), '') COLLATE DATABASE_DEFAULT
    AND TransactionType = b.RecType
    AND DebitAmount = CAST(b.RecDebit AS DECIMAL(18,4))
    AND CreditAmount = CAST(b.RecCredit AS DECIMAL(18,4))
);
";

        private static string GetStep19Sql() => @"
INSERT INTO Roles (Code, Name, IsActive, CreatedAt, IsAdmin)
SELECT DISTINCT LTRIM(RTRIM(GroupCode)), LTRIM(RTRIM(GroupCode)), 1, GETDATE(), 0
FROM [CWI_SOURCE].[CWI].[dbo].[cdUser]
WHERE GroupCode IS NOT NULL AND GroupCode <> ''
AND NOT EXISTS (SELECT 1 FROM Roles WHERE Code = LTRIM(RTRIM(GroupCode)) COLLATE DATABASE_DEFAULT);
";

        private static string GetStep20Sql() => @"
-- 1. Kullanıcıları aktar
INSERT INTO Users (UserName, Email, PasswordHash, EmployeeCode, FirstName, LastName, PhoneNumber, OfficeCode, SalesRepCode, LinkedCustomerId, IsAdministrator, RoleId, IsActive, CreatedAt)
SELECT 
    LTRIM(RTRIM(u.UserCode)), 
    CASE WHEN u.UserEMail IS NOT NULL AND u.UserEMail <> '' THEN LTRIM(RTRIM(u.UserEMail)) ELSE LTRIM(RTRIM(u.UserCode)) + '@temp.local' END,
    'MIGRATION_REQUIRED_RESET', 
    LTRIM(RTRIM(u.UserCode)), 
    LTRIM(RTRIM(u.UserName)), 
    LTRIM(RTRIM(u.UserSurName)), 
    LTRIM(RTRIM(u.UserCellNumber)), 
    LTRIM(RTRIM(u.UserPersonOfficeCode)), 
    LTRIM(RTRIM(u.UserSalsmanCode)), 
    cm.NewId, 
    u.IsGroupAdmin, 
    r.Id, 
    1, 
    u.CreateDate
FROM [CWI_SOURCE].[CWI].[dbo].[cdUser] u
LEFT JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(u.UserCurrAccCode)) = cm.OldCode COLLATE DATABASE_DEFAULT
INNER JOIN Roles r ON LTRIM(RTRIM(u.GroupCode)) = r.Code COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM Users WHERE UserName = LTRIM(RTRIM(u.UserCode)) COLLATE DATABASE_DEFAULT);

-- 2. Mapping tablosunu doldur
INSERT INTO MigrationMapping_Users(NewId, OldGroupCode, OldUserCode)
SELECT u.Id, LTRIM(RTRIM(src.GroupCode)), LTRIM(RTRIM(src.UserCode))
FROM [CWI_SOURCE].[CWI].[dbo].[cdUser] src
INNER JOIN Users u ON LTRIM(RTRIM(src.UserCode)) = u.UserName COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_Users WHERE OldUserCode = LTRIM(RTRIM(src.UserCode)) COLLATE DATABASE_DEFAULT AND OldGroupCode = LTRIM(RTRIM(src.GroupCode)) COLLATE DATABASE_DEFAULT);
";

        private static string GetStep21Sql() => @"
-- 1. Satın alma siparişlerini aktar
INSERT INTO PurchaseOrders (OrderNumber, SerialNumber, DocumentNumber, OrderedAt, TotalQuantity, TotalAmount, SupplierName, ExternalReference, IsReceived, SupplierId, CreatedAt)
SELECT 
    LTRIM(RTRIM(h.RecID)) COLLATE DATABASE_DEFAULT, 
    LTRIM(RTRIM(h.RecSeriNo)) COLLATE DATABASE_DEFAULT, 
    h.RecDocumentNo, 
    h.RecDate, 
    h.RecQty, 
    h.RecAmount, 
    LTRIM(RTRIM(h.CurrAccDesc)) COLLATE DATABASE_DEFAULT, 
    LTRIM(RTRIM(h.DocumentNumber)) COLLATE DATABASE_DEFAULT, 
    CASE WHEN h.Status = '1' OR h.Status = 'True' THEN 1 ELSE 0 END,
    cm.Id, 
    GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[cdCustomerOrderHeader] h
LEFT JOIN Customers cm ON LTRIM(RTRIM(h.CurrAccDesc)) COLLATE DATABASE_DEFAULT = cm.Name COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM PurchaseOrders WHERE OrderNumber = LTRIM(RTRIM(h.RecID)) COLLATE DATABASE_DEFAULT);

-- 2. Mapping tablosunu doldur
INSERT INTO MigrationMapping_PurchaseOrders (OldRecId, NewId)
SELECT h.RecID COLLATE DATABASE_DEFAULT, MIN(p.Id)
FROM [CWI_SOURCE].[CWI].[dbo].[cdCustomerOrderHeader] h
INNER JOIN PurchaseOrders p ON h.RecID COLLATE DATABASE_DEFAULT = p.OrderNumber COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_PurchaseOrders WHERE OldRecId = h.RecID COLLATE DATABASE_DEFAULT)
GROUP BY h.RecID COLLATE DATABASE_DEFAULT;
";

        private static string GetStep22Sql() => @"
INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, ProductCode, ProductName, Quantity, ReceivedQuantity, UnitPrice, LineTotal)
SELECT 
    pm.NewId, 
    prod.NewId, 
    LTRIM(RTRIM(l.RecItemCode)) COLLATE DATABASE_DEFAULT, 
    LTRIM(RTRIM(l.ItemDescription)) COLLATE DATABASE_DEFAULT, 
    l.RecQty, 
    l.Dispatch, 
    l.BPrice, 
    l.RecAmount
FROM [CWI_SOURCE].[CWI].[dbo].[cdCustomerOrderLine] l
INNER JOIN MigrationMapping_PurchaseOrders pm ON l.RecID COLLATE DATABASE_DEFAULT = pm.OldRecId COLLATE DATABASE_DEFAULT
INNER JOIN MigrationMapping_Products prod ON LTRIM(RTRIM(l.RecItemCode)) COLLATE DATABASE_DEFAULT = prod.OldSku COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM PurchaseOrderItems WHERE PurchaseOrderId = pm.NewId AND ProductId = prod.NewId AND Quantity = l.RecQty);
";

        private static string GetStep23Sql() => @"
-- 1. Faturayı aktar
INSERT INTO VendorInvoices (VendorId, InvoiceNumber, InvoicedAt, TotalAmount, CurrencyId, Description, IsPaid, PaidAmount, CreatedAt)
SELECT 
    cm.NewId, 
    LTRIM(RTRIM(src.InvoiceNo)), 
    src.InvoiceDate, 
    src.TotalAmount, 
    CAST(src.Currency AS INT), 
    LTRIM(RTRIM(src.Description)),
    0, 0, GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[cdVendorInvoice] src
INNER JOIN MigrationMapping_Customers cm ON src.CurrAccCode = cm.OldCode COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM VendorInvoices WHERE InvoiceNumber = src.InvoiceNo COLLATE DATABASE_DEFAULT);

-- 2. Mapping tablosunu doldur
INSERT INTO MigrationMapping_VendorInvoices (OldId, NewId)
SELECT src.Id, vi.Id
FROM [CWI_SOURCE].[CWI].[dbo].[cdVendorInvoice] src
INNER JOIN VendorInvoices vi ON LTRIM(RTRIM(src.InvoiceNo)) = vi.InvoiceNumber COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM MigrationMapping_VendorInvoices WHERE OldId = src.Id);
";

        private static string GetStep24Sql() => @"
INSERT INTO VendorPayments (VendorId, VendorInvoiceId, Amount, CurrencyId, PaidAt, ReferenceNumber, Description, CreatedAt)
SELECT 
    cm.NewId, 
    vi.Id,
    p.PaymentTotal, 
    COALESCE(p.PaymentCurrencyId, 1), 
    COALESCE(p.PaymentDate, GETDATE()), 
    LTRIM(RTRIM(p.ReceiptNumber)), 
    NULL, 
    GETDATE()
FROM [CWI_SOURCE].[CWI].[dbo].[cdPaymentVendor] p
LEFT JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(p.PaymentCurrAccCode)) = cm.OldCode COLLATE DATABASE_DEFAULT
LEFT JOIN VendorInvoices vi ON LTRIM(RTRIM(p.LineId)) = vi.InvoiceNumber COLLATE DATABASE_DEFAULT
WHERE NOT EXISTS (SELECT 1 FROM VendorPayments WHERE VendorId = cm.NewId AND Amount = p.PaymentTotal AND PaidAt = COALESCE(p.PaymentDate, GETDATE()));
";

        private static string GetStep25Sql() => @"
UPDATE vi
SET vi.PaidAmount = ISNULL(pay.TotalPaid, 0),
    vi.IsPaid = CASE WHEN ISNULL(pay.TotalPaid, 0) >= vi.TotalAmount THEN 1 ELSE 0 END
FROM VendorInvoices vi
LEFT JOIN (
    SELECT VendorInvoiceId, SUM(Amount) as TotalPaid
    FROM VendorPayments
    WHERE VendorInvoiceId IS NOT NULL
    GROUP BY VendorInvoiceId
) pay ON vi.Id = pay.VendorInvoiceId;
";

        #endregion
    }
}
