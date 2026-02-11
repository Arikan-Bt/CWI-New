# CWI Veri Aktarım Adımları

**Proje:** CWI Eski Sistem → Yeni .NET Core Sistem  
**Tarih:** 8 Ocak 2026  
**Hazırlayan:** Senior Backend Developer

---

## 📋 BAĞLANTI BİLGİLERİ

### Kaynak Veritabanı (Eski Sistem)

```
Server: 10.50.137.2
Database: CWI
User: awc
Password: Gh10*Ws!
```

### Hedef Veritabanı (Yeni Sistem)

```
Server: localhost
Database: ArikanCWIDB
User: sa
Password: GuchluBirSifre123!
```

---

## 🔄 AKTARIM SIRASI (ÖNEMLİ!)

Foreign key bağımlılıkları nedeniyle tablolar aşağıdaki sırayla aktarılmalıdır:

```
1. Lookup/Master Tablolar (bağımlılığı olmayan)
   ├── Languages
   ├── Currencies
   ├── Brands
   ├── Colors + ColorTranslations
   ├── AttributeTypes + AttributeTranslations
   ├── Warehouses
   ├── PaymentMethods
   └── ShipmentTerms

2. Ana Entity'ler (lookup'lara bağımlı)
   ├── Customers
   ├── Products + ProductTranslations
   ├── ProductPrices
   └── ProductImages

3. İşlem Verileri (ana entity'lere bağımlı)
   ├── Orders
   ├── OrderItems
   ├── OrderShippingInfo
   ├── InventoryItems
   ├── Payments
   └── CustomerTransactions

4. Kullanıcı & Yetki
   ├── Roles
   ├── Users
   ├── UserRoles
   ├── UserBrandAccess
   └── SalesTargets
```

---

## 📝 ADIM ADIM AKTARIM İŞLEMLERİ

---

### ADIM 1: Hedef Veritabanı Hazırlık

**Amaç:** Yeni veritabanında tabloları ve mapping tablolarını oluştur

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB (localhost)
-- =====================================================

-- 1.1. Migration Mapping Tablolarını Oluştur
-- Bu tablolar eski varchar PK'ları yeni int PK'lara eşleştirir

CREATE TABLE MigrationMapping_Customers (
    OldCode VARCHAR(30) PRIMARY KEY,
    NewId INT NOT NULL
);

CREATE TABLE MigrationMapping_Products (
    OldSku VARCHAR(30) PRIMARY KEY,
    NewId INT NOT NULL
);

CREATE TABLE MigrationMapping_Orders (
    OldLineId INT PRIMARY KEY,
    NewId BIGINT NOT NULL
);

CREATE TABLE MigrationMapping_Brands (
    OldCode VARCHAR(20) PRIMARY KEY,
    NewId INT NOT NULL
);

CREATE TABLE MigrationMapping_Colors (
    OldCode VARCHAR(20) PRIMARY KEY,
    NewId INT NOT NULL
);

CREATE TABLE MigrationMapping_Warehouses (
    OldCode INT PRIMARY KEY,
    NewId INT NOT NULL
);

CREATE TABLE MigrationMapping_Users (
    OldGroupCode VARCHAR(30),
    OldUserCode VARCHAR(20),
    NewId INT NOT NULL,
    PRIMARY KEY (OldGroupCode, OldUserCode)
);

PRINT 'Mapping tabloları oluşturuldu.';
```

---

### ADIM 2: Linked Server Oluştur (Opsiyonel)

**Amaç:** İki veritabanı arasında doğrudan veri transferi için

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB (localhost)
-- =====================================================

-- Linked Server oluştur (eğer farklı sunuculardaysa)
EXEC sp_addlinkedserver
    @server = 'CWI_SOURCE',
    @srvproduct = '',
    @provider = 'SQLNCLI',
    @datasrc = '10.50.137.2';

EXEC sp_addlinkedsrvlogin
    @rmtsrvname = 'CWI_SOURCE',
    @useself = 'FALSE',
    @locallogin = NULL,
    @rmtuser = 'awc',
    @rmtpassword = 'Gh10*Ws!';

-- Test et
SELECT TOP 1 * FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAcc];

PRINT 'Linked Server oluşturuldu ve test edildi.';
```

> **Not:** Eğer linked server kullanmıyorsanız, her adımda önce kaynak DB'den veriyi çekip, sonra hedef DB'ye insert edebilirsiniz.

---

### ADIM 3: Currency (Para Birimi) Aktarımı

**Kaynak Tablo:** `cdCurrency`  
**Hedef Tablo:** `Currencies`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

SET IDENTITY_INSERT Currencies ON;

INSERT INTO Currencies (Id, Code, Name, Symbol, IsDefault, IsActive)
SELECT
    Id,
    LTRIM(RTRIM(Currency)) AS Code,
    LTRIM(RTRIM(CurrencyName)) AS Name,
    CASE
        WHEN Currency = 'TRY' THEN '₺'
        WHEN Currency = 'USD' THEN '$'
        WHEN Currency = 'EUR' THEN '€'
        WHEN Currency = 'GBP' THEN '£'
        ELSE Currency
    END AS Symbol,
    CASE WHEN Currency = 'TRY' THEN 1 ELSE 0 END AS IsDefault,
    1 AS IsActive
FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrency];

SET IDENTITY_INSERT Currencies OFF;

-- Doğrulama
SELECT 'Currencies' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrency]) AS Kaynak,
       (SELECT COUNT(*) FROM Currencies) AS Hedef;
```

---

### ADIM 4: Brand (Marka) Aktarımı

**Kaynak Tablo:** `cdBrand`  
**Hedef Tablo:** `Brands`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO Brands (Code, Name, LogoUrl, IsActive, SortOrder)
OUTPUT inserted.Id, INSERTED.Code INTO MigrationMapping_Brands(NewId, OldCode)
SELECT
    LTRIM(RTRIM(BrandCode)) AS Code,
    LTRIM(RTRIM(BrandDescription)) AS Name,
    NULL AS LogoUrl,
    1 AS IsActive,
    ROW_NUMBER() OVER (ORDER BY BrandCode) AS SortOrder
FROM [CWI_SOURCE].[CWI].[dbo].[cdBrand];

-- Doğrulama
SELECT 'Brands' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdBrand]) AS Kaynak,
       (SELECT COUNT(*) FROM Brands) AS Hedef,
       (SELECT COUNT(*) FROM MigrationMapping_Brands) AS Mapping;
```

---

### ADIM 5: Color (Renk) Aktarımı

**Kaynak Tablolar:** `cdColor`, `cdColorDescription`  
**Hedef Tablolar:** `Colors`, `ColorTranslations`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

-- 5.1. Colors tablosunu aktar
INSERT INTO Colors (Code, HexCode, IsActive, CreatedAt)
OUTPUT inserted.Id, INSERTED.Code INTO MigrationMapping_Colors(NewId, OldCode)
SELECT
    LTRIM(RTRIM(ColorCode)) AS Code,
    LTRIM(RTRIM(ColorHexCode)) AS HexCode,
    1 AS IsActive,
    CreateDate AS CreatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[cdColor];

-- 5.2. ColorTranslations tablosunu aktar
INSERT INTO ColorTranslations (ColorId, LanguageCode, Name)
SELECT
    cm.NewId AS ColorId,
    LTRIM(RTRIM(cd.LangCode)) AS LanguageCode,
    LTRIM(RTRIM(cd.ColorDescription)) AS Name
FROM [CWI_SOURCE].[CWI].[dbo].[cdColorDescription] cd
INNER JOIN MigrationMapping_Colors cm ON cd.ColorCode = cm.OldCode;

-- Doğrulama
SELECT 'Colors' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdColor]) AS Kaynak,
       (SELECT COUNT(*) FROM Colors) AS Hedef;

SELECT 'ColorTranslations' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdColorDescription]) AS Kaynak,
       (SELECT COUNT(*) FROM ColorTranslations) AS Hedef;
```

---

### ADIM 6: Warehouse (Depo) Aktarımı

**Kaynak Tablo:** `cdWareHouse`  
**Hedef Tablo:** `Warehouses`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO Warehouses (Code, Name, Address, IsActive, IsDefault)
OUTPUT inserted.Id, INSERTED.Code INTO MigrationMapping_Warehouses(NewId, OldCode)
SELECT
    CAST(WareHouseCode AS VARCHAR(20)) AS Code,
    LTRIM(RTRIM(WareHouseDescription)) AS Name,
    NULL AS Address,
    1 AS IsActive,
    CASE WHEN WareHouseCode = 1 THEN 1 ELSE 0 END AS IsDefault
FROM [CWI_SOURCE].[CWI].[dbo].[cdWareHouse];

-- Doğrulama
SELECT 'Warehouses' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdWareHouse]) AS Kaynak,
       (SELECT COUNT(*) FROM Warehouses) AS Hedef;
```

---

### ADIM 7: PaymentMethod (Ödeme Yöntemi) Aktarımı

**Kaynak Tablo:** `cdPaymentMethod`  
**Hedef Tablo:** `PaymentMethods`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

SET IDENTITY_INSERT PaymentMethods ON;

INSERT INTO PaymentMethods (Id, Name, IsActive)
SELECT
    LineID AS Id,
    LTRIM(RTRIM(PaymentDescription)) AS Name,
    1 AS IsActive
FROM [CWI_SOURCE].[CWI].[dbo].[cdPaymentMethod];

SET IDENTITY_INSERT PaymentMethods OFF;

-- Doğrulama
SELECT 'PaymentMethods' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdPaymentMethod]) AS Kaynak,
       (SELECT COUNT(*) FROM PaymentMethods) AS Hedef;
```

---

### ADIM 8: Customer (Müşteri) Aktarımı

**Kaynak Tablo:** `cdCurrAcc`  
**Hedef Tablo:** `Customers`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO Customers (
    Code, Name, TaxOfficeName, TaxNumber,
    RegionCode, RegionName,
    AddressLine1, AddressLine2, District, Town, City, Country,
    PrimaryPhone, SecondaryPhone, Email,
    IsActive, CreatedAt, UpdatedAt
)
OUTPUT inserted.Id, INSERTED.Code INTO MigrationMapping_Customers(NewId, OldCode)
SELECT
    LTRIM(RTRIM(CurrAccCode)) AS Code,
    LTRIM(RTRIM(CurrAccDescription)) AS Name,
    LTRIM(RTRIM(TaxOffice)) AS TaxOfficeName,
    LTRIM(RTRIM(TaxNumber)) AS TaxNumber,
    LTRIM(RTRIM(CurrAccRegionCode)) AS RegionCode,
    LTRIM(RTRIM(CurrAccRegionName)) AS RegionName,
    LTRIM(RTRIM(StreetName1)) AS AddressLine1,
    LTRIM(RTRIM(StreetName2)) AS AddressLine2,
    LTRIM(RTRIM(Block)) AS District,
    LTRIM(RTRIM(DistrictName)) AS Town,
    LTRIM(RTRIM(City)) AS City,
    LTRIM(RTRIM(Country)) AS Country,
    LTRIM(RTRIM(Phone1)) AS PrimaryPhone,
    LTRIM(RTRIM(Phone2)) AS SecondaryPhone,
    NULL AS Email,  -- Eski sistemde email yok
    1 AS IsActive,
    GETDATE() AS CreatedAt,
    NULL AS UpdatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAcc];

-- Doğrulama
SELECT 'Customers' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAcc]) AS Kaynak,
       (SELECT COUNT(*) FROM Customers) AS Hedef,
       (SELECT COUNT(*) FROM MigrationMapping_Customers) AS Mapping;
```

---

### ADIM 9: Product (Ürün) Aktarımı

**Kaynak Tablolar:** `cdItem`, `cdItemPreOrder`, `cdItemDesc`  
**Hedef Tablo:** `Products`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

-- 9.1. Normal ürünleri aktar (IsPreOrder = 0)
INSERT INTO Products (
    Sku, Name, ColorId, BrandId,
    CategoryId, SubCategoryId, Attributes,
    IsPreOrder, PreOrderQuantity,
    IsActive, CreatedAt, UpdatedAt
)
OUTPUT inserted.Id, INSERTED.Sku INTO MigrationMapping_Products(NewId, OldSku)
SELECT
    LTRIM(RTRIM(i.ItemCode)) AS Sku,
    COALESCE(LTRIM(RTRIM(d.ItemDescription)), LTRIM(RTRIM(i.ItemCode))) AS Name,
    cm.NewId AS ColorId,
    bm.NewId AS BrandId,
    NULL AS CategoryId,      -- AttributeType mapping gerekirse sonra güncellenir
    NULL AS SubCategoryId,
    -- JSON formatında ek attribute'lar
    (SELECT
        i.ItemAttribute3 AS Attr3,
        i.ItemAttribute4 AS Attr4,
        i.ItemAttribute5 AS Attr5,
        i.ItemAttribute7 AS Attr7,
        i.ItemAttribute8 AS Attr8,
        i.ItemAttribute9 AS Attr9,
        i.ItemAttribute10 AS Attr10,
        i.ItemAttribute11 AS Attr11,
        i.ItemAttribute12 AS Attr12,
        i.ItemAttribute13 AS Attr13,
        i.ItemAttribute14 AS Attr14,
        i.ItemAttribute15 AS Attr15
     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS Attributes,
    0 AS IsPreOrder,
    NULL AS PreOrderQuantity,
    1 AS IsActive,
    GETDATE() AS CreatedAt,
    NULL AS UpdatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[cdItem] i
LEFT JOIN [CWI_SOURCE].[CWI].[dbo].[cdItemDesc] d
    ON i.ItemCode = d.ItemCode AND d.LangCode = 'EN'
LEFT JOIN MigrationMapping_Brands bm ON i.ItemAttribute6 = bm.OldCode
LEFT JOIN MigrationMapping_Colors cm ON i.ColorCode = cm.OldCode;

-- 9.2. PreOrder ürünleri aktar (IsPreOrder = 1)
-- Sadece cdItem'da olmayan ürünleri ekle
INSERT INTO Products (
    Sku, Name, ColorId, BrandId,
    Attributes, IsPreOrder, PreOrderQuantity,
    IsActive, CreatedAt, UpdatedAt
)
OUTPUT inserted.Id, INSERTED.Sku INTO MigrationMapping_Products(NewId, OldSku)
SELECT
    LTRIM(RTRIM(p.ItemCode)) AS Sku,
    LTRIM(RTRIM(p.ItemDescription)) AS Name,
    cm.NewId AS ColorId,
    bm.NewId AS BrandId,
    (SELECT
        p.ItemAttribute3 AS Attr3,
        p.ItemAttribute4 AS Attr4,
        p.ItemAttribute5 AS Attr5
     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS Attributes,
    1 AS IsPreOrder,
    p.AvailableQty AS PreOrderQuantity,
    1 AS IsActive,
    GETDATE() AS CreatedAt,
    NULL AS UpdatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[cdItemPreOrder] p
LEFT JOIN MigrationMapping_Brands bm ON p.ItemAttribute6 = bm.OldCode
LEFT JOIN MigrationMapping_Colors cm ON p.ColorCode = cm.OldCode
WHERE NOT EXISTS (
    SELECT 1 FROM [CWI_SOURCE].[CWI].[dbo].[cdItem] i
    WHERE i.ItemCode = p.ItemCode
);

-- Doğrulama
SELECT 'Products' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdItem]) AS 'Kaynak_cdItem',
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdItemPreOrder]) AS 'Kaynak_PreOrder',
       (SELECT COUNT(*) FROM Products) AS Hedef,
       (SELECT COUNT(*) FROM MigrationMapping_Products) AS Mapping;
```

---

### ADIM 10: ProductTranslation (Ürün Çevirisi) Aktarımı

**Kaynak Tablo:** `cdItemDesc`  
**Hedef Tablo:** `ProductTranslations`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO ProductTranslations (ProductId, LanguageCode, Name, Description)
SELECT
    pm.NewId AS ProductId,
    LTRIM(RTRIM(d.LangCode)) AS LanguageCode,
    LTRIM(RTRIM(d.ItemDescription)) AS Name,
    NULL AS Description
FROM [CWI_SOURCE].[CWI].[dbo].[cdItemDesc] d
INNER JOIN MigrationMapping_Products pm ON d.ItemCode = pm.OldSku;

-- Doğrulama
SELECT 'ProductTranslations' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdItemDesc]) AS Kaynak,
       (SELECT COUNT(*) FROM ProductTranslations) AS Hedef;
```

---

### ADIM 11: ProductPrice (Ürün Fiyatı) Aktarımı

**Kaynak Tablo:** `PriceList`  
**Hedef Tablo:** `ProductPrices`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO ProductPrices (ProductId, BrandId, UnitPrice, CurrencyId, ValidFrom, ValidTo, IsActive)
SELECT
    pm.NewId AS ProductId,
    NULL AS BrandId,
    pl.Price AS UnitPrice,
    c.Id AS CurrencyId,
    GETDATE() AS ValidFrom,
    NULL AS ValidTo,
    1 AS IsActive
FROM [CWI_SOURCE].[CWI].[dbo].[PriceList] pl
INNER JOIN MigrationMapping_Products pm ON pl.ItemCode = pm.OldSku
INNER JOIN Currencies c ON pl.Currency = c.Code;

-- Doğrulama
SELECT 'ProductPrices' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[PriceList]) AS Kaynak,
       (SELECT COUNT(*) FROM ProductPrices) AS Hedef;
```

---

### ADIM 12: ProductImage (Ürün Görseli) Aktarımı

**Kaynak Tablo:** `cdImageUrl`  
**Hedef Tablo:** `ProductImages`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO ProductImages (ProductId, Url, SortOrder, IsMain, CreatedAt)
SELECT
    pm.NewId AS ProductId,
    img.URL AS Url,
    ROW_NUMBER() OVER (PARTITION BY img.ItemCode ORDER BY img.ID) AS SortOrder,
    CASE WHEN ROW_NUMBER() OVER (PARTITION BY img.ItemCode ORDER BY img.ID) = 1 THEN 1 ELSE 0 END AS IsMain,
    GETDATE() AS CreatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[cdImageUrl] img
INNER JOIN MigrationMapping_Products pm ON img.ItemCode = pm.OldSku
WHERE img.URL IS NOT NULL AND img.URL <> '';

-- Doğrulama
SELECT 'ProductImages' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdImageUrl] WHERE URL IS NOT NULL) AS Kaynak,
       (SELECT COUNT(*) FROM ProductImages) AS Hedef;
```

---

### ADIM 13: Order (Sipariş) Aktarımı

**Kaynak Tablo:** `trShopCartHeader`  
**Hedef Tablo:** `Orders`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO Orders (
    OrderNumber, OrderedAt, CustomerId, SalesRepresentative,
    TotalQuantity, SubTotal, TotalDiscount, TaxableAmount, GrandTotal,
    Status, IsCompleted, IsApproved, IsCanceled, CancellationReason,
    Notes, IsPreOrder, CurrencyId,
    CreatedByGroupCode, CreatedByUsername, CreatedAt, ShippedAt, UpdatedAt
)
OUTPUT inserted.Id, INSERTED.OrderNumber INTO MigrationMapping_Orders(NewId, OldLineId)
SELECT
    h.OrderRefNo AS OrderNumber,
    h.OrderDate AS OrderedAt,
    cm.NewId AS CustomerId,
    LTRIM(RTRIM(h.SalesManPerson)) AS SalesRepresentative,
    h.TotalQty AS TotalQuantity,
    h.TotalAmount AS SubTotal,
    h.TotalDicount AS TotalDiscount,  -- Eski tablodaki typo korunuyor
    h.TaxBase AS TaxableAmount,
    h.NetAmount AS GrandTotal,
    -- Status Mapping
    CASE
        WHEN h.IsCancelled = 1 THEN 3  -- Canceled
        WHEN h.StatusCode = -1 THEN -1 -- Draft (PreOrder)
        WHEN h.StatusCode = 0 THEN 0   -- Pending
        WHEN h.StatusCode = 1 THEN 1   -- Approved
        WHEN h.StatusCode = 2 THEN 2   -- Shipped
        ELSE h.StatusCode
    END AS Status,
    h.IsCompleated AS IsCompleted,
    h.IsApproved AS IsApproved,
    h.IsCancelled AS IsCanceled,
    LTRIM(RTRIM(h.CancelReason)) AS CancellationReason,
    LTRIM(RTRIM(h.OrderNote)) AS Notes,
    0 AS IsPreOrder,
    1 AS CurrencyId,  -- Default TRY, gerekirse güncellenir
    LTRIM(RTRIM(h.CreatedGroupCode)) AS CreatedByGroupCode,
    LTRIM(RTRIM(h.CreatedUserName)) AS CreatedByUsername,
    h.CreatedDate AS CreatedAt,
    CASE WHEN h.ShippedDate = '1900-01-01' THEN NULL ELSE h.ShippedDate END AS ShippedAt,
    NULL AS UpdatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader] h
INNER JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(h.CurrAccCode)) = cm.OldCode;

-- NOT: Customer mapping'de eşleşmeyen siparişler için LEFT JOIN kullanılabilir
-- Ancak bu durumda CustomerId NULL olacaktır

-- Doğrulama
SELECT 'Orders' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader]) AS Kaynak,
       (SELECT COUNT(*) FROM Orders) AS Hedef,
       (SELECT COUNT(*) FROM MigrationMapping_Orders) AS Mapping;

-- Eşleşmeyen siparişleri kontrol et
SELECT COUNT(*) AS EslesemeyenSiparisler
FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader] h
WHERE NOT EXISTS (
    SELECT 1 FROM MigrationMapping_Customers cm
    WHERE LTRIM(RTRIM(h.CurrAccCode)) = cm.OldCode
);
```

---

### ADIM 14: OrderItem (Sipariş Satırı) Aktarımı

**Kaynak Tablo:** `trShopCartLine`  
**Hedef Tablo:** `OrderItems`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO OrderItems (
    OrderId, ProductId, ProductName, Quantity,
    UnitPrice, DiscountAmount, LineTotal,
    TaxRate, TaxAmount, TaxableAmount, NetTotal,
    WarehouseId, Notes, CreatedByUsername, CreatedAt
)
SELECT
    om.NewId AS OrderId,
    pm.NewId AS ProductId,
    LTRIM(RTRIM(l.ItemDescription)) AS ProductName,
    l.Qty AS Quantity,
    l.Price AS UnitPrice,
    l.Discount AS DiscountAmount,
    l.Amount AS LineTotal,
    l.VatPercent AS TaxRate,
    l.VatBase AS TaxAmount,
    l.TaxBase AS TaxableAmount,
    l.NetAmount AS NetTotal,
    wm.NewId AS WarehouseId,
    LTRIM(RTRIM(l.LineNote)) AS Notes,
    LTRIM(RTRIM(l.CreatedUserName)) AS CreatedByUsername,
    l.CreatedDate AS CreatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartLine] l
INNER JOIN MigrationMapping_Orders om ON l.MasterLineID = om.OldLineId
INNER JOIN MigrationMapping_Products pm ON LTRIM(RTRIM(l.ItemCode)) = pm.OldSku
LEFT JOIN MigrationMapping_Warehouses wm ON TRY_CAST(l.WareHouseCode AS INT) = wm.OldCode;

-- Doğrulama
SELECT 'OrderItems' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartLine]) AS Kaynak,
       (SELECT COUNT(*) FROM OrderItems) AS Hedef;

-- Eşleşmeyen satırları kontrol et
SELECT COUNT(*) AS EslesemeyenSatirlar
FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartLine] l
WHERE NOT EXISTS (
    SELECT 1 FROM MigrationMapping_Orders om WHERE l.MasterLineID = om.OldLineId
)
OR NOT EXISTS (
    SELECT 1 FROM MigrationMapping_Products pm WHERE LTRIM(RTRIM(l.ItemCode)) = pm.OldSku
);
```

---

### ADIM 15: OrderShippingInfo (Sipariş Teslimat Bilgisi) Aktarımı

**Kaynak Tablo:** `trShopCartDetail`  
**Hedef Tablo:** `OrderShippingInfos`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO OrderShippingInfos (
    OrderId, ShippingAddress, PaymentMethod,
    ShipmentTerms, AdditionalDiscount
)
SELECT
    om.NewId AS OrderId,
    LTRIM(RTRIM(d.OrderAddress)) AS ShippingAddress,
    LTRIM(RTRIM(d.OrderPaymentMethod)) AS PaymentMethod,
    LTRIM(RTRIM(d.OrderShipmentTerms)) AS ShipmentTerms,
    d.ExtraDiscount AS AdditionalDiscount
FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartDetail] d
INNER JOIN MigrationMapping_Orders om ON d.MasterLineID = om.OldLineId;

-- Doğrulama
SELECT 'OrderShippingInfos' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartDetail]) AS Kaynak,
       (SELECT COUNT(*) FROM OrderShippingInfos) AS Hedef;
```

---

### ADIM 16: InventoryItem (Stok) Aktarımı

**Kaynak Tablo:** `trWareHouseItems`  
**Hedef Tablo:** `InventoryItems`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO InventoryItems (
    WarehouseId, ProductId, QuantityOnHand,
    QuantityReserved, ReorderLevel, LastStockTakeAt, UpdatedAt
)
SELECT
    wm.NewId AS WarehouseId,
    pm.NewId AS ProductId,
    w.Qty AS QuantityOnHand,
    -- Bekleyen siparişlerdeki rezerv miktarı
    ISNULL((
        SELECT SUM(l.Qty)
        FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartLine] l
        INNER JOIN [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader] h ON l.MasterLineID = h.LineID
        WHERE LTRIM(RTRIM(l.ItemCode)) = LTRIM(RTRIM(w.ItemCode))
          AND h.StatusCode IN (-1, 0, 1)  -- Draft, Pending, Approved
    ), 0) AS QuantityReserved,
    NULL AS ReorderLevel,
    NULL AS LastStockTakeAt,
    GETDATE() AS UpdatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[trWareHouseItems] w
INNER JOIN MigrationMapping_Warehouses wm ON w.WareHouseCode = wm.OldCode
INNER JOIN MigrationMapping_Products pm ON LTRIM(RTRIM(w.ItemCode)) = pm.OldSku;

-- Doğrulama
SELECT 'InventoryItems' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[trWareHouseItems]) AS Kaynak,
       (SELECT COUNT(*) FROM InventoryItems) AS Hedef;
```

---

### ADIM 17: Payment (Ödeme) Aktarımı

**Kaynak Tablo:** `cdPayment`  
**Hedef Tablo:** `Payments`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO Payments (
    CustomerId, OrderId, Amount, CurrencyId,
    ReceiptNumber, PaidAt, PaymentMethodId,
    Status, Notes, CreatedAt, CreatedByUsername
)
SELECT
    cm.NewId AS CustomerId,
    om.NewId AS OrderId,
    p.PaymentTotal AS Amount,
    COALESCE(p.PaymentCurrencyId, 1) AS CurrencyId,
    LTRIM(RTRIM(p.ReceiptNumber)) AS ReceiptNumber,
    p.PaymentDate AS PaidAt,
    NULL AS PaymentMethodId,  -- Eski sistemde direkt bağlantı yok
    1 AS Status,  -- Completed
    NULL AS Notes,
    p.PaymentDate AS CreatedAt,
    NULL AS CreatedByUsername
FROM [CWI_SOURCE].[CWI].[dbo].[cdPayment] p
LEFT JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(p.PaymentCurrAccCode)) = cm.OldCode
LEFT JOIN MigrationMapping_Orders om ON TRY_CAST(p.LineId AS INT) = om.OldLineId;

-- Doğrulama
SELECT 'Payments' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdPayment]) AS Kaynak,
       (SELECT COUNT(*) FROM Payments) AS Hedef;
```

---

### ADIM 18: CustomerTransaction (Cari Hareket) Aktarımı

**Kaynak Tablo:** `cdCurrAccBalance`  
**Hedef Tablo:** `CustomerTransactions`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

INSERT INTO CustomerTransactions (
    CustomerId, TransactionType, TransactionDate,
    ReferenceNumber, Description, DocumentType,
    ApplicationReference, DebitAmount, CreditAmount, Balance,
    CreatedAt
)
SELECT
    cm.NewId AS CustomerId,
    b.RecType AS TransactionType,
    b.RecDate AS TransactionDate,
    LTRIM(RTRIM(b.RecRefNo)) AS ReferenceNumber,
    LTRIM(RTRIM(b.RecDescription)) AS Description,
    LTRIM(RTRIM(b.RecTransType)) AS DocumentType,
    LTRIM(RTRIM(b.RecAppRefNo)) AS ApplicationReference,
    CAST(b.RecDebit AS DECIMAL(18,4)) AS DebitAmount,
    CAST(b.RecCredit AS DECIMAL(18,4)) AS CreditAmount,
    b.RecBalance AS Balance,
    GETDATE() AS CreatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAccBalance] b
INNER JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(b.RecCurrAccCode)) = cm.OldCode
WHERE b.RecCurrAccCode IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM CustomerTransactions
    WHERE CustomerId = cm.NewId
    AND TransactionDate = b.RecDate
    AND ISNULL(ReferenceNumber, '') = ISNULL(LTRIM(RTRIM(b.RecRefNo)), '')
    AND TransactionType = b.RecType
    AND DebitAmount = CAST(b.RecDebit AS DECIMAL(18,4))
    AND CreditAmount = CAST(b.RecCredit AS DECIMAL(18,4))
);
-- (Opsiyonel) Tarih filtresi eklemek isterseniz: AND b.RecDate >= '2023-01-01'

-- Eşleşmeyen (Aktarılmayan) Kayıtları Kontrol Et
INSERT INTO MigrationSkipLog (TableName, OldId, Reason)
SELECT 'CustomerTransactions', b.RecRefNo, 'Customer Mapping Not Found'
FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAccBalance] b
LEFT JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(b.RecCurrAccCode)) = cm.OldCode
WHERE cm.NewId IS NULL;

-- Doğrulama
SELECT 'CustomerTransactions' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAccBalance]) AS Kaynak,
       (SELECT COUNT(*) FROM CustomerTransactions) AS Hedef;
```

---

### ADIM 19: User (Kullanıcı) Aktarımı

**Kaynak Tablo:** `cdUser`  
**Hedef Tablo:** `AspNetUsers` (Identity)

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

-- NOT: ASP.NET Identity tabloları özel yapıdadır
-- Password'ler hash'lenmeli, bu nedenle kullanıcılar şifre sıfırlama yapmalı

INSERT INTO AspNetUsers (
    UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
    AccessFailedCount,
    -- Custom alanlar (User entity'de tanımlı)
    EmployeeCode, FirstName, LastName, OfficeCode, SalesRepCode,
    LinkedCustomerId, IsAdministrator, CreatedAt, UpdatedAt
)
OUTPUT inserted.Id, u.GroupCode, u.UserCode
    INTO MigrationMapping_Users(NewId, OldGroupCode, OldUserCode)
SELECT
    LTRIM(RTRIM(u.UserCode)) AS UserName,
    UPPER(LTRIM(RTRIM(u.UserCode))) AS NormalizedUserName,
    CASE
        WHEN u.UserEMail IS NOT NULL AND u.UserEMail <> ''
        THEN LTRIM(RTRIM(u.UserEMail))
        ELSE LTRIM(RTRIM(u.UserCode)) + '@temp.local'
    END AS Email,
    CASE
        WHEN u.UserEMail IS NOT NULL AND u.UserEMail <> ''
        THEN UPPER(LTRIM(RTRIM(u.UserEMail)))
        ELSE UPPER(LTRIM(RTRIM(u.UserCode))) + '@TEMP.LOCAL'
    END AS NormalizedEmail,
    0 AS EmailConfirmed,
    -- Geçici password hash - kullanıcılar şifre sıfırlamalı
    'MIGRATION_REQUIRED_RESET' AS PasswordHash,
    NEWID() AS SecurityStamp,
    NEWID() AS ConcurrencyStamp,
    LTRIM(RTRIM(u.UserCellNumber)) AS PhoneNumber,
    0 AS PhoneNumberConfirmed,
    0 AS TwoFactorEnabled,
    1 AS LockoutEnabled,
    0 AS AccessFailedCount,
    -- Custom alanlar
    LTRIM(RTRIM(u.UserCode)) AS EmployeeCode,
    LTRIM(RTRIM(u.UserName)) AS FirstName,
    LTRIM(RTRIM(u.UserSurName)) AS LastName,
    LTRIM(RTRIM(u.UserPersonOfficeCode)) AS OfficeCode,
    LTRIM(RTRIM(u.UserSalsmanCode)) AS SalesRepCode,
    cm.NewId AS LinkedCustomerId,
    u.IsGroupAdmin AS IsAdministrator,
    u.CreateDate AS CreatedAt,
    u.LastUpdatedDate AS UpdatedAt
FROM [CWI_SOURCE].[CWI].[dbo].[cdUser] u
LEFT JOIN MigrationMapping_Customers cm ON LTRIM(RTRIM(u.UserCurrAccCode)) = cm.OldCode;

-- Doğrulama
SELECT 'Users' AS Tablo,
       (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdUser]) AS Kaynak,
       (SELECT COUNT(*) FROM AspNetUsers) AS Hedef;
```

---

### ADIM 20: Role ve UserRole Aktarımı

**Kaynak Tablo:** `cdUserGroup` (veya cdUser.GroupCode distinct)  
**Hedef Tablolar:** `AspNetRoles`, `AspNetUserRoles`

```sql
-- =====================================================
-- HEDEF DB'DE ÇALIŞTIR: ArikanCWIDB
-- =====================================================

-- 20.1. Rolleri oluştur (unique GroupCode'lardan)
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
SELECT
    NEWID() AS Id,
    GroupCode AS Name,
    UPPER(GroupCode) AS NormalizedName,
    NEWID() AS ConcurrencyStamp
FROM (
    SELECT DISTINCT LTRIM(RTRIM(GroupCode)) AS GroupCode
    FROM [CWI_SOURCE].[CWI].[dbo].[cdUser]
    WHERE GroupCode IS NOT NULL AND GroupCode <> ''
) AS DistinctGroups;

-- 20.2. User-Role ilişkilerini oluştur
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT
    um.NewId AS UserId,
    r.Id AS RoleId
FROM MigrationMapping_Users um
INNER JOIN AspNetRoles r ON UPPER(um.OldGroupCode) = r.NormalizedName;

-- Doğrulama
SELECT 'Roles' AS Tablo,
       (SELECT COUNT(DISTINCT GroupCode) FROM [CWI_SOURCE].[CWI].[dbo].[cdUser]) AS Kaynak,
       (SELECT COUNT(*) FROM AspNetRoles) AS Hedef;

SELECT 'UserRoles' AS Tablo,
       (SELECT COUNT(*) FROM MigrationMapping_Users) AS Kaynak,
       (SELECT COUNT(*) FROM AspNetUserRoles) AS Hedef;
```

---

## ✅ DOĞRULAMA KONTROL LİSTESİ

Her adımdan sonra aşağıdaki sorguları çalıştırarak doğrulama yapın:

```sql
-- =====================================================
-- KAPSAMLI DOĞRULAMA SORGUSU
-- =====================================================

SELECT 'MIGRATION DOĞRULAMA RAPORU' AS Rapor, GETDATE() AS Tarih;

SELECT '--- KAYIT SAYISI KARŞILAŞTIRMASI ---' AS Baslik;

SELECT
    'Currencies' AS Tablo,
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrency]) AS Kaynak,
    (SELECT COUNT(*) FROM Currencies) AS Hedef,
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrency]) = (SELECT COUNT(*) FROM Currencies)
         THEN '✓ OK' ELSE '✗ FARK VAR' END AS Durum
UNION ALL
SELECT 'Brands',
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdBrand]),
    (SELECT COUNT(*) FROM Brands),
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdBrand]) = (SELECT COUNT(*) FROM Brands)
         THEN '✓ OK' ELSE '✗ FARK VAR' END
UNION ALL
SELECT 'Colors',
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdColor]),
    (SELECT COUNT(*) FROM Colors),
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdColor]) = (SELECT COUNT(*) FROM Colors)
         THEN '✓ OK' ELSE '✗ FARK VAR' END
UNION ALL
SELECT 'Customers',
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAcc]),
    (SELECT COUNT(*) FROM Customers),
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdCurrAcc]) = (SELECT COUNT(*) FROM Customers)
         THEN '✓ OK' ELSE '✗ FARK VAR' END
UNION ALL
SELECT 'Products',
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdItem]) +
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdItemPreOrder] p
     WHERE NOT EXISTS (SELECT 1 FROM [CWI_SOURCE].[CWI].[dbo].[cdItem] i WHERE i.ItemCode = p.ItemCode)),
    (SELECT COUNT(*) FROM Products),
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdItem]) +
              (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdItemPreOrder] p
               WHERE NOT EXISTS (SELECT 1 FROM [CWI_SOURCE].[CWI].[dbo].[cdItem] i WHERE i.ItemCode = p.ItemCode))
              = (SELECT COUNT(*) FROM Products)
         THEN '✓ OK' ELSE '✗ FARK VAR' END
UNION ALL
SELECT 'Orders',
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader]),
    (SELECT COUNT(*) FROM Orders),
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartHeader]) = (SELECT COUNT(*) FROM Orders)
         THEN '✓ OK' ELSE '✗ FARK VAR' END
UNION ALL
SELECT 'OrderItems',
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartLine]),
    (SELECT COUNT(*) FROM OrderItems),
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[trShopCartLine]) = (SELECT COUNT(*) FROM OrderItems)
         THEN '✓ OK' ELSE '✗ FARK VAR' END
UNION ALL
SELECT 'Payments',
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdPayment]),
    (SELECT COUNT(*) FROM Payments),
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdPayment]) = (SELECT COUNT(*) FROM Payments)
         THEN '✓ OK' ELSE '✗ FARK VAR' END
UNION ALL
SELECT 'Users',
    (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdUser]),
    (SELECT COUNT(*) FROM AspNetUsers),
    CASE WHEN (SELECT COUNT(*) FROM [CWI_SOURCE].[CWI].[dbo].[cdUser]) = (SELECT COUNT(*) FROM AspNetUsers)
         THEN '✓ OK' ELSE '✗ FARK VAR' END;
```

---

## 🔄 GERİ ALMA (ROLLBACK)

Herhangi bir sorun durumunda:

```sql
-- =====================================================
-- ACİL GERİ ALMA
-- =====================================================

-- Tüm verileri temizle (sıralı - FK bağımlılıkları nedeniyle)
DELETE FROM AspNetUserRoles;
DELETE FROM AspNetUsers;
DELETE FROM AspNetRoles;
DELETE FROM CustomerTransactions;
DELETE FROM Payments;
DELETE FROM InventoryItems;
DELETE FROM OrderShippingInfos;
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM ProductImages;
DELETE FROM ProductPrices;
DELETE FROM ProductTranslations;
DELETE FROM Products;
DELETE FROM Customers;
DELETE FROM ColorTranslations;
DELETE FROM Colors;
DELETE FROM Brands;
DELETE FROM Warehouses;
DELETE FROM PaymentMethods;
DELETE FROM Currencies;

-- Mapping tablolarını temizle
DELETE FROM MigrationMapping_Users;
DELETE FROM MigrationMapping_Orders;
DELETE FROM MigrationMapping_Products;
DELETE FROM MigrationMapping_Warehouses;
DELETE FROM MigrationMapping_Colors;
DELETE FROM MigrationMapping_Brands;
DELETE FROM MigrationMapping_Customers;

-- Identity'leri sıfırla
DBCC CHECKIDENT ('Customers', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
-- ... diğer tablolar için de
```

---

## 📌 ÖNEMLİ NOTLAR

1. **Sıralama Kritik:** Tablolar mutlaka belirtilen sırada aktarılmalı
2. **Transaction Kullanın:** Her adımı BEGIN TRAN / COMMIT TRAN içinde çalıştırın
3. **Backup Alın:** Her adımdan önce hedef DB'nin yedeğini alın
4. **Test Ortamında Deneyin:** Önce test ortamında çalıştırın
5. **Parola Reset:** Tüm kullanıcılar için parola sıfırlama maili gönderin
6. **Linked Server:** Eğer sunucular farklıysa linked server gerekli

---

**Doküman Sonu**
