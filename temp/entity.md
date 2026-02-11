# CWI Entity Dizayn Önerisi

**Proje:** CWI → Modern .NET Core Migration  
**Tarih:** 8 Ocak 2026  
**Yaklaşım:** Clean Architecture, Domain-Driven Design (DDD) prensipleri

---

## 1. GENEL DİZAYN PRENSİPLERİ

### 1.1. İsimlendirme Kuralları

| Kural           | Eski Sistem                      | Yeni Sistem             |
| --------------- | -------------------------------- | ----------------------- |
| Entity İsimleri | cdCurrAcc, trShopCartHeader      | Customer, Order         |
| Kolon İsimleri  | CurrAccCode, CurrAccDescription  | Code, Name              |
| Primary Key     | LineID, CurrAccCode              | Id (int/long)           |
| Foreign Key     | MasterLineID, CurrAccCode        | OrderId, CustomerId     |
| Boolean         | IsCompleated (typo), IsCancelled | IsCompleted, IsCanceled |
| Tarih           | CreatedDate, OrderDate           | CreatedAt, OrderedAt    |

### 1.2. Veri Tipi Dönüşümleri

| Eski Tip             | Yeni Tip           | Açıklama                  |
| -------------------- | ------------------ | ------------------------- |
| money                | decimal(18,4)      | Para birimleri için       |
| varchar(30) PK       | int/long           | Surrogate key             |
| varchar(30)          | string + MaxLength | Business key olarak kalır |
| bit                  | bool               | -                         |
| smalldatetime        | DateTime           | -                         |
| varbinary (password) | string (hashed)    | Identity framework        |

### 1.3. Modül Yapısı

```
CWI.Domain/
├── Entities/
│   ├── Customers/
│   ├── Products/
│   ├── Orders/
│   ├── Inventory/
│   ├── Payments/
│   ├── Purchasing/
│   └── Identity/
├── Enums/
├── ValueObjects/
└── Common/
```

---

## 2. ENTITY MAPPING TABLOSU

### 2.1. MÜŞTERİ MODÜLÜ (Customers)

| Eski Tablo          | Yeni Entity             | Açıklama                     |
| ------------------- | ----------------------- | ---------------------------- |
| cdCurrAcc           | **Customer**            | Ana müşteri entity'si        |
| cdCurrAccBalance    | **CustomerTransaction** | Cari hareket                 |
| cdCurrAccReports    | ❌ Kaldırıldı           | View olarak kalacak          |
| cdCrm               | **CustomerContact**     | CRM iletişim bilgisi         |
| AWC_MusteriCurrency | **CustomerPricing**     | Müşteri-Marka fiyat ilişkisi |
| AWC_MusteriIskonto  | **CustomerDiscount**    | Müşteri iskonto tanımı       |
| prCurrAccBalance    | ❌ Kaldırıldı           | Computed column olacak       |

#### Customer Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ Customer                                                             │
├─────────────────────────────────────────────────────────────────────┤
│ ESKİ KOLON                  → YENİ KOLON                  TİP       │
├─────────────────────────────────────────────────────────────────────┤
│ (yeni)                      → Id                          int PK    │
│ CurrAccCode                 → Code                        string(30)│
│ CurrAccDescription          → Name                        string(100)│
│ TaxOffice                   → TaxOfficeName               string(100)│
│ TaxNumber                   → TaxNumber                   string(20)│
│ CurrAccRegionCode           → RegionCode                  string(20)│
│ CurrAccRegionName           → RegionName                  string(100)│
│ StreetName1                 → AddressLine1                string(200)│
│ StreetName2                 → AddressLine2                string(200)│
│ Block                       → District                    string(200)│
│ DistrictName                → Town                        string(200)│
│ City                        → City                        string(200)│
│ Country                     → Country                     string(200)│
│ Phone1                      → PrimaryPhone                string(50)│
│ Phone2                      → SecondaryPhone              string(50)│
│ (yeni)                      → Email                       string(200)│
│ (yeni)                      → IsActive                    bool      │
│ (yeni)                      → CreatedAt                   DateTime  │
│ (yeni)                      → UpdatedAt                   DateTime? │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation Properties:                                               │
│   → Orders (ICollection<Order>)                                     │
│   → Transactions (ICollection<CustomerTransaction>)                 │
│   → Contacts (ICollection<CustomerContact>)                         │
│   → PricingRules (ICollection<CustomerPricing>)                     │
│   → Discounts (ICollection<CustomerDiscount>)                       │
└─────────────────────────────────────────────────────────────────────┘
```

#### CustomerTransaction Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ CustomerTransaction (eski: cdCurrAccBalance)                        │
├─────────────────────────────────────────────────────────────────────┤
│ (yeni)                      → Id                          long PK   │
│ RecCurrAccCode              → CustomerId                  int FK    │
│ RecType                     → TransactionType             enum      │
│ RecDate                     → TransactionDate             DateTime  │
│ RecRefNo                    → ReferenceNumber             string(50)│
│ RecDescription              → Description                 string(200)│
│ RecTransType                → DocumentType                string(50)│
│ RecAppRefNo                 → ApplicationReference        string(50)│
│ RecDebit                    → DebitAmount                 decimal   │
│ RecCredit                   → CreditAmount                decimal   │
│ RecBalance                  → Balance                     decimal   │
│ (yeni)                      → CreatedAt                   DateTime  │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Customer                                                │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 2.2. ÜRÜN MODÜLÜ (Products)

| Eski Tablo                        | Yeni Entity                  | Açıklama                          |
| --------------------------------- | ---------------------------- | --------------------------------- |
| cdItem                            | **Product**                  | Ana ürün entity'si                |
| cdItemPreOrder                    | **Product**                  | IsPreOrder flag ile birleştirildi |
| cdItemDesc                        | **ProductTranslation**       | Çok dilli açıklama                |
| cdBrand                           | **Brand**                    | Marka tanımı                      |
| cdColor                           | **Color**                    | Renk tanımı                       |
| cdColorDescription                | **ColorTranslation**         | Çok dilli renk                    |
| cdProductAttribute                | **ProductAttribute**         | Özellik tanımı                    |
| cdProductAttributeType            | **AttributeType**            | Özellik tipi                      |
| cdProductAttributeDescription     | **AttributeTranslation**     | Çok dilli özellik                 |
| cdProductAttributeTypeDescription | **AttributeTypeTranslation** | Çok dilli tip                     |
| cdStockNote                       | **ProductNote**              | Ürün notu                         |
| cdImageUrl                        | **ProductImage**             | Ürün görseli                      |
| PriceList + PriceListEUR          | **ProductPrice**             | Birleşik fiyat tablosu            |

#### Product Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ Product (eski: cdItem + cdItemPreOrder birleşimi)                   │
├─────────────────────────────────────────────────────────────────────┤
│ (yeni)                      → Id                          int PK    │
│ ItemCode                    → Sku                         string(30)│
│ ItemDescription             → Name                        string(100)│
│ ColorCode                   → ColorId                     int? FK   │
│ ItemAttribute6              → BrandId                     int? FK   │
│ ItemAttribute1              → CategoryId                  int? FK   │
│ ItemAttribute2              → SubCategoryId               int? FK   │
│ ItemAttribute3-5, 7-15      → Attributes (JSON)           string    │
│ (cdItemPreOrder.AvailableQty) → PreOrderQuantity          int?      │
│ (yeni)                      → IsPreOrder                  bool      │
│ (yeni)                      → IsActive                    bool      │
│ (yeni)                      → CreatedAt                   DateTime  │
│ (yeni)                      → UpdatedAt                   DateTime? │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation Properties:                                               │
│   → Brand                                                           │
│   → Color                                                           │
│   → Category (AttributeType)                                        │
│   → SubCategory (AttributeType)                                     │
│   → Translations (ICollection<ProductTranslation>)                  │
│   → Prices (ICollection<ProductPrice>)                              │
│   → Images (ICollection<ProductImage>)                              │
│   → Notes (ICollection<ProductNote>)                                │
│   → InventoryItems (ICollection<InventoryItem>)                     │
└─────────────────────────────────────────────────────────────────────┘
```

#### ProductPrice Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ ProductPrice (eski: PriceList + PriceListEUR birleşimi)             │
├─────────────────────────────────────────────────────────────────────┤
│ (yeni)                      → Id                          int PK    │
│ ItemCode                    → ProductId                   int FK    │
│ (yeni)                      → BrandId                     int? FK   │
│ Price                       → UnitPrice                   decimal   │
│ Currency                    → CurrencyId                  int FK    │
│ (yeni)                      → ValidFrom                   DateTime  │
│ (yeni)                      → ValidTo                     DateTime? │
│ (yeni)                      → IsActive                    bool      │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Product, Currency, Brand                                │
└─────────────────────────────────────────────────────────────────────┘
```

#### Brand Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ Brand (eski: cdBrand)                                               │
├─────────────────────────────────────────────────────────────────────┤
│ (yeni)                      → Id                          int PK    │
│ BrandCode                   → Code                        string(10)│
│ BrandDescription            → Name                        string(100)│
│ (yeni)                      → LogoUrl                     string(500)│
│ (yeni)                      → IsActive                    bool      │
│ (yeni)                      → SortOrder                   int       │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Products (ICollection<Product>)                         │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 2.3. SİPARİŞ MODÜLÜ (Orders)

| Eski Tablo             | Yeni Entity              | Açıklama             |
| ---------------------- | ------------------------ | -------------------- |
| trShopCartHeader       | **Order**                | Sipariş başlık       |
| trShopCartLine         | **OrderItem**            | Sipariş satırı       |
| trShopCartDetail       | **OrderShippingInfo**    | Teslimat bilgisi     |
| trShopCartAddition     | **OrderDeliveryRequest** | Talep edilen tarih   |
| trShopCartoon          | **OrderPackage**         | Koli başlık          |
| trShopCartoonLine      | **OrderPackageItem**     | Koli içerik          |
| cdOrderStatus          | **OrderStatus** (Enum)   | Durum enum'a dönüştü |
| PreOrderShopCartHeader | **Order**                | IsPreOrder flag ile  |
| PreOrderShopCartLine   | **OrderItem**            | Birleştirildi        |
| CaniasOrders           | **OrderErpSync**         | ERP senkron durumu   |

#### Order Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ Order (eski: trShopCartHeader + PreOrderShopCartHeader)             │
├─────────────────────────────────────────────────────────────────────┤
│ LineID                      → Id                          long PK   │
│ OrderRefNo                  → OrderNumber                 string(50)│
│ OrderDate                   → OrderedAt                   DateTime  │
│ CurrAccCode                 → CustomerId                  int FK    │
│ SalesManPerson              → SalesRepresentative         string(100)│
│ TotalQty                    → TotalQuantity               decimal   │
│ TotalAmount                 → SubTotal                    decimal   │
│ TotalDicount                → TotalDiscount               decimal   │
│ TaxBase                     → TaxableAmount               decimal   │
│ NetAmount                   → GrandTotal                  decimal   │
│ VatPercent1-5               → (OrderTaxDetail'e taşındı)            │
│ VatBase1-5                  → (OrderTaxDetail'e taşındı)            │
│ StatusCode                  → Status                      enum      │
│ IsCompleated                → IsCompleted                 bool      │
│ IsApproved                  → IsApproved                  bool      │
│ IsCancelled                 → IsCanceled                  bool      │
│ CancelReason                → CancellationReason          string(500)│
│ OrderNote                   → Notes                       string(1000)│
│ (yeni)                      → IsPreOrder                  bool      │
│ CreatedGroupCode            → CreatedByGroupCode          string(50)│
│ CreatedUserName             → CreatedByUsername           string(50)│
│ CreatedDate                 → CreatedAt                   DateTime  │
│ ShippedDate                 → ShippedAt                   DateTime? │
│ (yeni)                      → UpdatedAt                   DateTime? │
│ (yeni)                      → CurrencyId                  int FK    │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation Properties:                                               │
│   → Customer                                                        │
│   → Currency                                                        │
│   → Items (ICollection<OrderItem>)                                  │
│   → ShippingInfo (OrderShippingInfo)                                │
│   → DeliveryRequest (OrderDeliveryRequest)                          │
│   → Packages (ICollection<OrderPackage>)                            │
│   → TaxDetails (ICollection<OrderTaxDetail>)                        │
│   → Payments (ICollection<Payment>)                                 │
│   → ErpSync (OrderErpSync)                                          │
└─────────────────────────────────────────────────────────────────────┘
```

#### OrderItem Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ OrderItem (eski: trShopCartLine)                                    │
├─────────────────────────────────────────────────────────────────────┤
│ LineID                      → Id                          long PK   │
│ MasterLineID                → OrderId                     long FK   │
│ ItemCode                    → ProductId                   int FK    │
│ ItemDescription             → ProductName                 string(100)│
│ Qty                         → Quantity                    int       │
│ Price                       → UnitPrice                   decimal   │
│ Discount                    → DiscountAmount              decimal   │
│ Amount                      → LineTotal                   decimal   │
│ VatPercent                  → TaxRate                     decimal   │
│ VatBase                     → TaxAmount                   decimal   │
│ TaxBase                     → TaxableAmount               decimal   │
│ NetAmount                   → NetTotal                    decimal   │
│ WareHouseCode               → WarehouseId                 int FK    │
│ LineNote                    → Notes                       string(500)│
│ CreatedUserName             → CreatedByUsername           string(50)│
│ CreatedDate                 → CreatedAt                   DateTime  │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Order, Product, Warehouse                               │
└─────────────────────────────────────────────────────────────────────┘
```

#### OrderShippingInfo Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ OrderShippingInfo (eski: trShopCartDetail)                          │
├─────────────────────────────────────────────────────────────────────┤
│ LineID                      → Id                          long PK   │
│ MasterLineID                → OrderId                     long FK   │
│ OrderAddress                → ShippingAddress             string(500)│
│ OrderPaymentMethod          → PaymentMethod               string(250)│
│ OrderShipmentTerms          → ShipmentTerms               string(100)│
│ ExtraDiscount               → AdditionalDiscount          decimal   │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Order                                                   │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 2.4. ENVANTER MODÜLÜ (Inventory)

| Eski Tablo       | Yeni Entity        | Açıklama            |
| ---------------- | ------------------ | ------------------- |
| cdWareHouse      | **Warehouse**      | Depo tanımı         |
| trWareHouseItems | **InventoryItem**  | Stok kartı          |
| trWareHouseStock | ❌ Kaldırıldı      | View olarak         |
| trBrands         | **WarehouseBrand** | Depo-marka ilişkisi |

#### Warehouse Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ Warehouse (eski: cdWareHouse)                                       │
├─────────────────────────────────────────────────────────────────────┤
│ WareHouseCode               → Id                          int PK    │
│ WareHouseDescription        → Name                        string(200)│
│ (yeni)                      → Code                        string(20)│
│ (yeni)                      → Address                     string(500)│
│ (yeni)                      → IsActive                    bool      │
│ (yeni)                      → IsDefault                   bool      │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: InventoryItems (ICollection<InventoryItem>)             │
└─────────────────────────────────────────────────────────────────────┘
```

#### InventoryItem Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ InventoryItem (eski: trWareHouseItems)                              │
├─────────────────────────────────────────────────────────────────────┤
│ (yeni)                      → Id                          long PK   │
│ WareHouseCode               → WarehouseId                 int FK    │
│ ItemCode                    → ProductId                   int FK    │
│ Qty                         → QuantityOnHand              int       │
│ (yeni)                      → QuantityReserved            int       │
│ (yeni)                      → QuantityAvailable           int (computed)│
│ (yeni)                      → ReorderLevel                int?      │
│ (yeni)                      → LastStockTakeAt             DateTime? │
│ (yeni)                      → UpdatedAt                   DateTime  │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Warehouse, Product                                      │
│ Unique Index: (WarehouseId, ProductId)                              │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 2.5. ÖDEME MODÜLÜ (Payments)

| Eski Tablo               | Yeni Entity             | Açıklama              |
| ------------------------ | ----------------------- | --------------------- |
| cdPayment                | **Payment**             | Müşteri ödemesi       |
| cdPaymentHistory         | **PaymentAuditLog**     | Ödeme geçmişi (audit) |
| cdPaymentMethod          | **PaymentMethod**       | Ödeme yöntemi         |
| cdPaymentNotificationLog | **PaymentNotification** | Ödeme bildirimi       |
| cdCurrency               | **Currency**            | Para birimi           |
| cdTransactionLog         | **PaymentTransaction**  | Banka işlem log       |
| cdBankSettings           | **BankConfiguration**   | POS ayarları          |
| cdBankBinCodes           | **BankBinCode**         | Kart BIN kodları      |

#### Payment Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ Payment (eski: cdPayment)                                           │
├─────────────────────────────────────────────────────────────────────┤
│ PaymentID                   → Id                          long PK   │
│ PaymentCurrAccCode          → CustomerId                  int FK    │
│ LineId                      → OrderId                     long? FK  │
│ PaymentTotal                → Amount                      decimal   │
│ PaymentCurrencyId           → CurrencyId                  int FK    │
│ ReceiptNumber               → ReceiptNumber               string(50)│
│ PaymentDate                 → PaidAt                      DateTime  │
│ (yeni)                      → PaymentMethodId             int FK    │
│ (yeni)                      → Status                      enum      │
│ (yeni)                      → Notes                       string(500)│
│ (yeni)                      → CreatedAt                   DateTime  │
│ (yeni)                      → CreatedByUsername           string(50)│
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Customer, Order, Currency, PaymentMethod                │
└─────────────────────────────────────────────────────────────────────┘
```

#### Currency Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ Currency (eski: cdCurrency)                                         │
├─────────────────────────────────────────────────────────────────────┤
│ Id                          → Id                          int PK    │
│ Currency                    → Code                        string(5) │
│ CurrencyName                → Name                        string(50)│
│ (yeni)                      → Symbol                      string(5) │
│ (yeni)                      → IsDefault                   bool      │
│ (yeni)                      → IsActive                    bool      │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Payments, ProductPrices, Orders                         │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 2.6. SATIN ALMA MODÜLÜ (Purchasing)

| Eski Tablo            | Yeni Entity           | Açıklama            |
| --------------------- | --------------------- | ------------------- |
| cdCustomerOrderHeader | **PurchaseOrder**     | Satın alma siparişi |
| cdCustomerOrderLine   | **PurchaseOrderItem** | Satın alma satırı   |
| cdPurchase            | **GoodsReceipt**      | Mal alım fişi       |
| cdPurchaseLine        | **GoodsReceiptItem**  | Mal alım satırı     |
| cdPurchaseCanias      | **PurchaseErpSync**   | ERP senkron         |
| cdVendorInvoice       | **VendorInvoice**     | Tedarikçi faturası  |
| cdPaymentVendor       | **VendorPayment**     | Tedarikçi ödemesi   |

#### PurchaseOrder Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ PurchaseOrder (eski: cdCustomerOrderHeader - satın alma perspektifi)│
├─────────────────────────────────────────────────────────────────────┤
│ (yeni)                      → Id                          long PK   │
│ RecID                       → OrderNumber                 string(50)│
│ RecSeriNo                   → SerialNumber                string(15)│
│ RecDocumentNo               → DocumentNumber              int       │
│ RecDate                     → OrderedAt                   DateTime  │
│ RecQty                      → TotalQuantity               int       │
│ RecAmount                   → TotalAmount                 decimal   │
│ CurrAccDesc                 → SupplierName                string(150)│
│ DocumentNumber              → ExternalReference           string(50)│
│ Status                      → IsReceived                  bool      │
│ (yeni)                      → SupplierId                  int? FK   │
│ (yeni)                      → CreatedAt                   DateTime  │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Items (ICollection<PurchaseOrderItem>)                  │
└─────────────────────────────────────────────────────────────────────┘
```

#### VendorInvoice Entity

```
┌─────────────────────────────────────────────────────────────────────┐
│ VendorInvoice (eski: cdVendorInvoice)                               │
├─────────────────────────────────────────────────────────────────────┤
│ Id                          → Id                          int PK    │
│ CurrAccCode                 → VendorId                    int FK    │
│ InvoiceNo                   → InvoiceNumber               string(50)│
│ InvoiceDate                 → InvoicedAt                  DateTime  │
│ TotalAmount                 → TotalAmount                 decimal   │
│ Currency                    → CurrencyId                  int FK    │
│ Description                 → Description                 string    │
│ (yeni)                      → DueDate                     DateTime? │
│ (yeni)                      → IsPaid                      bool      │
│ (yeni)                      → PaidAmount                  decimal   │
│ (yeni)                      → Balance (computed)          decimal   │
│ (yeni)                      → CreatedAt                   DateTime  │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation: Vendor (Customer), Currency, Payments                   │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 2.7. KULLANICI & YETKİ MODÜLÜ (Identity)

| Eski Tablo              | Yeni Entity                 | Açıklama                |
| ----------------------- | --------------------------- | ----------------------- |
| cdUser                  | **User** (ASP.NET Identity) | Kullanıcı               |
| cdUserGroup             | **Role** (ASP.NET Identity) | Rol/Grup                |
| cdUserBrand             | **UserBrandAccess**         | Kullanıcı-marka yetkisi |
| cdUserBrands            | **UserBrandAccess**         | Birleştirildi           |
| cdSalesPersonRegion     | **UserRegionAccess**        | Satıcı-bölge yetkisi    |
| cdSalesPersonTargets    | **SalesTarget**             | Satış hedefi            |
| cdSalesPersonWareHouses | **UserWarehouseAccess**     | Satıcı-depo yetkisi     |
| prUserLogin             | **UserLoginHistory**        | Giriş log               |

#### User Entity (ASP.NET Identity Extends)

```
┌─────────────────────────────────────────────────────────────────────┐
│ User : IdentityUser<int> (eski: cdUser)                             │
├─────────────────────────────────────────────────────────────────────┤
│ (Identity)                  → Id                          int PK    │
│ (Identity)                  → UserName                    string    │
│ (Identity)                  → Email                       string    │
│ (Identity)                  → PasswordHash                string    │
│ UserCode                    → EmployeeCode                string(20)│
│ GroupCode                   → (Role olarak)               -         │
│ UserName                    → FirstName                   string(60)│
│ UserSurName                 → LastName                    string(60)│
│ UserCellNumber              → PhoneNumber                 string    │
│ UserPersonOfficeCode        → OfficeCode                  string(30)│
│ UserSalsmanCode             → SalesRepCode                string(30)│
│ UserCurrAccCode             → LinkedCustomerId            int? FK   │
│ IsGroupAdmin                → IsAdministrator             bool      │
│ CreateDate                  → CreatedAt                   DateTime  │
│ LastUpdatedDate             → UpdatedAt                   DateTime? │
├─────────────────────────────────────────────────────────────────────┤
│ Navigation Properties:                                               │
│   → LinkedCustomer (Customer)                                       │
│   → BrandAccess (ICollection<UserBrandAccess>)                      │
│   → RegionAccess (ICollection<UserRegionAccess>)                    │
│   → WarehouseAccess (ICollection<UserWarehouseAccess>)              │
│   → SalesTargets (ICollection<SalesTarget>)                         │
│   → LoginHistory (ICollection<UserLoginHistory>)                    │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 2.8. LOOKUP & REFERANS TABLOLARI

| Eski Tablo        | Yeni Entity         | Açıklama            |
| ----------------- | ------------------- | ------------------- |
| cdAppLanguage     | **Language**        | Dil tanımı          |
| cdAppFields       | **LocalizedString** | Uygulama çevirileri |
| cdShipmentTerms   | **ShipmentTerm**    | Sevkiyat koşulu     |
| cdCompanyNews     | **Announcement**    | Duyuru              |
| cdBannerManagment | **Banner**          | Banner yönetimi     |

---

### 2.9. SİSTEM & LOG TABLOLARI

| Eski Tablo        | Yeni Entity        | Açıklama                       |
| ----------------- | ------------------ | ------------------------------ |
| Logs              | **ApplicationLog** | Uygulama log                   |
| DB_Errors         | **ErrorLog**       | Hata log                       |
| Testlogs          | ❌ Kaldırıldı      | Development artifact           |
| XMLImportData     | **ImportJob**      | Import log                     |
| AspNet_SqlCache\* | ❌ Kaldırıldı      | Distributed cache kullanılacak |

---

## 3. ENUM TANIMLARI

```csharp
public enum OrderStatus
{
    Draft = -1,          // Eski: StatusCode = -1 (PreOrder)
    Pending = 0,         // Eski: StatusCode = 0
    Approved = 1,        // Eski: StatusCode = 1
    Shipped = 2,         // Eski: StatusCode = 2
    Canceled = 3         // Eski: IsCancelled = true
}

public enum TransactionType
{
    Invoice = 1,
    Payment = 2,
    CreditNote = 3,
    DebitNote = 4,
    OpeningBalance = 5
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}
```

---

## 4. VALUE OBJECTS

```csharp
public record Address
{
    public string Line1 { get; init; }
    public string Line2 { get; init; }
    public string District { get; init; }
    public string City { get; init; }
    public string Country { get; init; }
    public string PostalCode { get; init; }
}

public record Money
{
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; }
}

public record DateRange
{
    public DateTime Start { get; init; }
    public DateTime? End { get; init; }
}
```

---

## 5. MİGRASYON HARİÇ TUTULAN TABLOLAR

| Tablo                                      | Sebep                  |
| ------------------------------------------ | ---------------------- |
| cdCurrAcc_yedek                            | Yedek tablo            |
| cdItemPreOrderYedek_20250213               | Yedek tablo            |
| trWareHouseItems_20240918                  | Yedek tablo            |
| trShopCartDetail_yedek_20240905            | Yedek tablo            |
| Testlogs                                   | Test verisi            |
| AspNet_SqlCacheTablesForChangeNotification | Eski cache mekanizması |
| tabBankList                                | Geçici import tablosu  |
| AWC_Siparisler                             | Geçici sipariş tablosu |
| ImportExcelData                            | Geçici import tablosu  |

---

## 6. ÖZET ENTITY LİSTESİ

### Toplam: 45 Entity

| Modül          | Entity Sayısı | Entity'ler                                                                                                                                |
| -------------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| **Customers**  | 5             | Customer, CustomerTransaction, CustomerContact, CustomerPricing, CustomerDiscount                                                         |
| **Products**   | 10            | Product, ProductTranslation, ProductPrice, ProductImage, ProductNote, Brand, Color, ColorTranslation, AttributeType, AttributeTranslation |
| **Orders**     | 8             | Order, OrderItem, OrderShippingInfo, OrderDeliveryRequest, OrderPackage, OrderPackageItem, OrderTaxDetail, OrderErpSync                   |
| **Inventory**  | 3             | Warehouse, InventoryItem, WarehouseBrand                                                                                                  |
| **Payments**   | 7             | Payment, PaymentMethod, PaymentTransaction, PaymentNotification, Currency, BankConfiguration, BankBinCode                                 |
| **Purchasing** | 6             | PurchaseOrder, PurchaseOrderItem, GoodsReceipt, GoodsReceiptItem, VendorInvoice, VendorPayment                                            |
| **Identity**   | 6             | User, Role, UserBrandAccess, UserRegionAccess, UserWarehouseAccess, SalesTarget, UserLoginHistory                                         |
| **Lookups**    | 4             | Language, LocalizedString, ShipmentTerm, Announcement, Banner                                                                             |
| **System**     | 3             | ApplicationLog, ErrorLog, ImportJob                                                                                                       |

---

## 7. ONAY BEKLİYOR

Bu entity dizaynı hakkında görüşlerinizi almak istiyorum:

1. ✅ / ❌ **Genel yapı uygun mu?**
2. ✅ / ❌ **Entity isimleri anlaşılır mı?**
3. ✅ / ❌ **Kolon isimleri tutarlı mı?**
4. ✅ / ❌ **Modül ayrımı mantıklı mı?**
5. ✅ / ❌ **Hariç tutulan tablolar doğru mu?**
6. 🔄 **Eklemek/çıkarmak istediğiniz entity var mı?**
7. 🔄 **Farklı isimlendirme tercihiniz var mı?**

Onayınızın ardından geliştirme aşamasına geçebiliriz.

---

_Dizayn Dokümanı Sonu_
