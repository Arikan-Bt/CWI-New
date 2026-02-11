# Arıkan CWI Projesi - Detaylı Akış Analizi ve Yazılımcı Kılavuzu

> **Versiyon**: 1.0  
> **Son Güncelleme**: 29 Aralık 2024  
> **Hedef**: .NET 9.0 + Hibrit CQRS + Stored Procedure Mimarisi

---

## 📋 1. Genel Bakış

Bu doküman, Arıkan firması için geliştirilecek **B2B E-Ticaret ve Sipariş Yönetim Sistemi**nin teknik kılavuzudur.

### 1.1 Projenin Amacı
- Bayilerin online sipariş vermesi
- Cari hesap ve bakiye takibi
- Stok yönetimi ve depo dağıtımı
- Ödeme ve fatura takibi
- Excel ile toplu işlemler

### 1.2 Hedef Teknoloji Stack
| Katman | Teknoloji |
|--------|-----------|
| Backend | .NET 9.0 Web API |
| ORM/Data | Dapper + Stored Procedures |
| Pattern | CQRS with MediatR |
| Auth | JWT Bearer Token |
| Cache | Redis (opsiyonel) |
| Database | SQL Server |
| Frontend | Ayrı proje (React/Vue) |

---

## 🏗️ 2. Mimari Yapı: Hibrit CQRS + Stored Procedure

### 2.1 Neden Hibrit Yaklaşım?

```
┌─────────────────────────────────────────────────────────────────┐
│                    MİMARİ KARAR                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  CQRS (MediatR) Katmanı:                                         │
│  ├── Validasyon kuralları                                        │
│  ├── Yetkilendirme kontrolleri                                   │
│  ├── Loglama ve audit trail                                      │
│  └── İş akışı orkestrasyonu                                      │
│                                                                  │
│  Stored Procedure Katmanı:                                       │
│  ├── Veri manipülasyonu (CRUD)                                   │
│  ├── Karmaşık sorgular ve raporlar                               │
│  ├── Transaction yönetimi                                        │
│  └── Performans kritik işlemler                                  │
│                                                                  │
│  AVANTAJLAR:                                                     │
│  ✓ SQL gücü korunur, performans maksimum                         │
│  ✓ İş mantığı test edilebilir C# sınıflarında                    │
│  ✓ Güvenlik: Tüm DB erişimi SP üzerinden                         │
│  ✓ Bakım kolaylığı: Mantık tek yerde                             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Proje Klasör Yapısı

```
Arikan.Solution/
│
├── src/
│   ├── Arikan.Domain/                    # Entity ve Interface'ler
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Customer.cs
│   │   │   ├── Product.cs
│   │   │   ├── ShoppingCart.cs
│   │   │   └── Order.cs
│   │   ├── Enums/
│   │   │   ├── UserType.cs
│   │   │   ├── OrderStatus.cs
│   │   │   └── CustomerType.cs
│   │   └── Interfaces/
│   │       ├── ICustomerRepository.cs
│   │       ├── ICartRepository.cs
│   │       └── IOrderRepository.cs
│   │
│   ├── Arikan.Application/               # CQRS Handlers
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── LoggingBehavior.cs
│   │   │   ├── Interfaces/
│   │   │   │   └── ICurrentUserService.cs
│   │   │   └── Models/
│   │   │       └── Result.cs
│   │   │
│   │   └── Features/
│   │       ├── Auth/
│   │       │   ├── Commands/
│   │       │   │   └── LoginCommand.cs
│   │       │   └── Queries/
│   │       │       └── GetCurrentUserQuery.cs
│   │       │
│   │       ├── Customers/
│   │       │   ├── Commands/
│   │       │   │   ├── CreateCustomerCommand.cs
│   │       │   │   ├── UpdateCustomerCommand.cs
│   │       │   │   └── DeleteCustomerCommand.cs
│   │       │   └── Queries/
│   │       │       ├── GetCustomersQuery.cs
│   │       │       └── GetCustomerByIdQuery.cs
│   │       │
│   │       ├── Cart/
│   │       │   ├── Commands/
│   │       │   │   ├── AddToCartCommand.cs
│   │       │   │   ├── RemoveFromCartCommand.cs
│   │       │   │   └── ClearCartCommand.cs
│   │       │   └── Queries/
│   │       │       └── GetCartQuery.cs
│   │       │
│   │       └── Orders/
│   │           ├── Commands/
│   │           │   ├── CreateOrderCommand.cs
│   │           │   ├── ApproveOrderCommand.cs
│   │           │   └── CancelOrderCommand.cs
│   │           └── Queries/
│   │               └── GetOrdersQuery.cs
│   │
│   ├── Arikan.Infrastructure/            # Data Access
│   │   ├── Data/
│   │   │   └── DapperContext.cs
│   │   ├── Repositories/
│   │   │   ├── CustomerRepository.cs
│   │   │   ├── CartRepository.cs
│   │   │   └── OrderRepository.cs
│   │   └── Services/
│   │       ├── EmailService.cs
│   │       └── FileStorageService.cs
│   │
│   └── Arikan.API/                       # Web API
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── CustomersController.cs
│       │   ├── CartController.cs
│       │   └── OrdersController.cs
│       ├── Middleware/
│       │   └── ExceptionMiddleware.cs
│       └── Program.cs
│
├── database/
│   └── StoredProcedures/
│       ├── Auth/
│       │   └── sp_Auth_Login.sql
│       ├── Customers/
│       │   ├── sp_Customer_GetAll.sql
│       │   ├── sp_Customer_GetById.sql
│       │   ├── sp_Customer_Insert.sql
│       │   ├── sp_Customer_Update.sql
│       │   └── sp_Customer_Delete.sql
│       ├── Cart/
│       │   ├── sp_Cart_AddItem.sql
│       │   └── sp_Cart_GetByUser.sql
│       └── Orders/
│           └── sp_Order_Create.sql
│
└── tests/
    └── Arikan.Tests/
```

---

## 👤 3. Kullanıcı Tipleri ve Yetkilendirme

### 3.1 Kullanıcı Rolleri

| Kod | Rol | Yetkiler |
|-----|-----|----------|
| 0 | Admin | Tüm yetkiler, kullanıcı yönetimi |
| 1 | Office | Sipariş onaylama, cari yönetimi |
| 2 | Salesman | Kendi müşterilerinin siparişleri |
| 3 | Dealer | Sipariş verme, kendi bakiyesini görme |
| 4 | Excel | Toplu sipariş yükleme |

### 3.2 Yetki Matrisi

| İşlem | Admin | Office | Salesman | Dealer |
|-------|:-----:|:------:|:--------:|:------:|
| Müşteri Listele | ✅ Tümü | ✅ Tümü | ✅ Kendi | ❌ |
| Müşteri Ekle | ✅ | ✅ | ❌ | ❌ |
| Müşteri Güncelle | ✅ | ✅ | ❌ | ❌ |
| Müşteri Sil | ✅ | ❌ | ❌ | ❌ |
| Sipariş Ver | ✅ | ✅ | ✅ | ✅ |
| Sipariş Onayla | ✅ | ✅ | ❌ | ❌ |
| Bakiye Gör | ✅ Tümü | ✅ Tümü | ✅ Kendi | ✅ Kendi |

---

## 🔐 4. Kimlik Doğrulama Akışı (Authentication)

### 4.1 Login İş Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│  ADIM 1: Kullanıcı Login Formu Doldurur                         │
│  POST /api/auth/login                                           │
│  Body: { username: "xxx", password: "xxx" }                     │
├─────────────────────────────────────────────────────────────────┤
│  ADIM 2: AuthController → LoginCommand gönderir                 │
│                                                                  │
│  [HttpPost("login")]                                             │
│  public async Task<IActionResult> Login(LoginRequest request)   │
│  {                                                               │
│      var command = new LoginCommand(request.Username,           │
│                                      request.Password);          │
│      var result = await _mediator.Send(command);                │
│      return Ok(result);                                          │
│  }                                                               │
├─────────────────────────────────────────────────────────────────┤
│  ADIM 3: LoginCommandHandler işlemi yürütür                     │
│                                                                  │
│  1. Validasyon: Username/Password boş mu?                       │
│  2. Repository.LoginAsync() → SP çağırır                        │
│  3. Başarılı ise JWT Token üret                                  │
│  4. Giriş logunu kaydet                                          │
│  5. Response dön                                                 │
├─────────────────────────────────────────────────────────────────┤
│  ADIM 4: Stored Procedure                                       │
│                                                                  │
│  EXEC sp_Auth_Login @Username, @PasswordHash                    │
│                                                                  │
│  SP İçeriği:                                                     │
│  - cdUser tablosundan kullanıcı çek                             │
│  - Şifre hash karşılaştır                                       │
│  - Kullanıcı bilgilerini döndür veya NULL                       │
├─────────────────────────────────────────────────────────────────┤
│  ADIM 5: Response                                               │
│                                                                  │
│  {                                                               │
│    "success": true,                                              │
│    "token": "eyJhbGciOiJIUzI1NiIs...",                          │
│    "user": {                                                     │
│      "userCode": "USR001",                                      │
│      "userName": "Ahmet",                                       │
│      "userType": 3,                                              │
│      "currAccCode": "MUS001"                                    │
│    }                                                             │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 Örnek Kod: LoginCommand

```csharp
// Application/Features/Auth/Commands/LoginCommand.cs

public record LoginCommand(string Username, string Password) 
    : IRequest<Result<LoginResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthRepository _authRepo;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAuthRepository authRepo, 
        IJwtService jwtService,
        ILogger<LoginCommandHandler> logger)
    {
        _authRepo = authRepo;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Validasyon
        if (string.IsNullOrEmpty(request.Username))
            return Result<LoginResponse>.Failure("Kullanıcı adı boş olamaz");

        // 2. Şifreyi hashle
        var passwordHash = HashPassword(request.Password);

        // 3. SP çağır
        var user = await _authRepo.LoginAsync(request.Username, passwordHash);

        if (user == null)
        {
            _logger.LogWarning("Başarısız giriş: {Username}", request.Username);
            return Result<LoginResponse>.Failure("Kullanıcı adı veya şifre hatalı");
        }

        // 4. JWT Token üret
        var token = _jwtService.GenerateToken(user);

        // 5. Login log kaydet
        await _authRepo.LogLoginAsync(user.UserCode, GetClientIp());

        // 6. Response dön
        return Result<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            User = user.ToDto()
        });
    }
}
```

---

## 🏢 5. Cari Hesap (Müşteri) Yönetimi

### 5.1 Entity Yapısı

```csharp
// Domain/Entities/Customer.cs

public class Customer
{
    public string CurrAccCode { get; set; }        // Primary Key
    public string CurrAccDescription { get; set; } // Firma Adı
    public int CurrAccTypeCode { get; set; }       // 1=Müşteri, 2=Tedarikçi
    public bool IsActive { get; set; }
    
    // İletişim
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Phone1 { get; set; }
    public string Email { get; set; }
    
    // Vergi Bilgileri
    public string TaxNumber { get; set; }
    public string TaxOffice { get; set; }
    public string IdentityNumber { get; set; }  // TC Kimlik
    
    // Ticari Bilgiler
    public string CurrencyCode { get; set; }    // TRY, USD, EUR
    public decimal CreditLimit { get; set; }
    public decimal DiscountPercent { get; set; }
    public string SalesmanCode { get; set; }
    
    // Audit
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string UpdatedBy { get; set; }
}
```

### 5.2 Müşteri Listeleme Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│  GET /api/customers?search=abc&page=1&size=20                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Controller → GetCustomersQuery gönderir                     │
│                                                                  │
│  2. Handler:                                                     │
│     - Yetki kontrolü (Admin/Office tümünü, Salesman kendisini)  │
│     - Repository.GetAllAsync(filters)                           │
│                                                                  │
│  3. SP: sp_Customer_GetAll                                       │
│     @SearchTerm VARCHAR(100),                                   │
│     @TypeCode INT = NULL,                                        │
│     @SalesmanCode VARCHAR(50) = NULL,                           │
│     @IsActive BIT = NULL,                                        │
│     @Page INT = 1,                                               │
│     @PageSize INT = 20                                          │
│                                                                  │
│  4. Dönüş: PagedResult<CustomerDto>                              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 5.3 Müşteri Ekleme Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│  POST /api/customers                                            │
│  Body: { currAccDescription: "ABC Ltd", taxNumber: "123..." }   │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ADIM 1: Validasyonlar (Handler'da)                             │
│  ├── CurrAccCode benzersiz mi?                                  │
│  ├── TaxNumber 10 veya 11 hane mi?                              │
│  ├── TC Kimlik algoritma kontrolü                               │
│  ├── Email format kontrolü                                      │
│  └── CreditLimit >= 0 mı?                                       │
│                                                                  │
│  ADIM 2: SP Çağrısı                                             │
│  EXEC sp_Customer_Insert                                         │
│      @CurrAccCode,                                              │
│      @CurrAccDescription,                                       │
│      @TaxNumber,                                                │
│      @Email,                                                    │
│      @CreditLimit,                                              │
│      @CreatedBy,                                                │
│      ...                                                        │
│                                                                  │
│  ADIM 3: Audit Log                                              │
│  INSERT INTO AuditLog (Table, Action, RecordId, User, Date)    │
│  VALUES ('cdCurrAcc', 'INSERT', @CurrAccCode, @User, GETDATE()) │
│                                                                  │
│  ADIM 4: Response                                               │
│  { success: true, data: { currAccCode: "MUS001", ... } }        │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 5.4 Örnek Kod: CreateCustomerCommand

```csharp
// Application/Features/Customers/Commands/CreateCustomerCommand.cs

public record CreateCustomerCommand(
    string CurrAccCode,
    string CurrAccDescription,
    int CurrAccTypeCode,
    string TaxNumber,
    string Email,
    string Phone1,
    decimal CreditLimit
) : IRequest<Result<CustomerDto>>;

public class CreateCustomerCommandValidator 
    : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CurrAccCode)
            .NotEmpty().WithMessage("Cari kodu zorunludur")
            .MaximumLength(50);

        RuleFor(x => x.CurrAccDescription)
            .NotEmpty().WithMessage("Firma adı zorunludur");

        RuleFor(x => x.TaxNumber)
            .Must(BeValidTaxNumber).WithMessage("Geçersiz vergi numarası");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0);
    }

    private bool BeValidTaxNumber(string taxNumber)
    {
        if (string.IsNullOrEmpty(taxNumber)) return true;
        return taxNumber.Length == 10 || taxNumber.Length == 11;
    }
}

public class CreateCustomerCommandHandler 
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public async Task<Result<CustomerDto>> Handle(
        CreateCustomerCommand request, 
        CancellationToken ct)
    {
        // 1. Kod benzersizlik kontrolü
        var exists = await _repo.ExistsByCodeAsync(request.CurrAccCode);
        if (exists)
            return Result<CustomerDto>.Failure("Bu cari kodu zaten kullanılıyor");

        // 2. Entity oluştur
        var customer = new Customer
        {
            CurrAccCode = request.CurrAccCode,
            CurrAccDescription = request.CurrAccDescription,
            CurrAccTypeCode = request.CurrAccTypeCode,
            TaxNumber = request.TaxNumber,
            Email = request.Email,
            Phone1 = request.Phone1,
            CreditLimit = request.CreditLimit,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = _currentUser.UserCode
        };

        // 3. Kaydet (SP çağırır)
        await _repo.CreateAsync(customer);

        // 4. DTO olarak dön
        return Result<CustomerDto>.Success(customer.ToDto());
    }
}
```

---

## 🛒 6. Sepet Yönetimi (Shopping Cart)

### 6.1 Sepete Ürün Ekleme - KRİTİK AKIŞ

```
┌─────────────────────────────────────────────────────────────────┐
│  POST /api/cart/items                                           │
│  Body: { itemCode: "PRD001", quantity: 5 }                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ⚠️ DİKKAT: Bu akış DEPO DAĞITIMI içerir!                       │
│                                                                  │
│  ADIM 1: AddToCartCommand                                       │
│  ├── UserCode (JWT'den)                                         │
│  ├── ItemCode                                                   │
│  └── Quantity                                                   │
│                                                                  │
│  ADIM 2: Handler                                                │
│  ├── Ürün var mı kontrolü                                       │
│  ├── Stok kontrolü                                              │
│  └── SP çağır                                                   │
│                                                                  │
│  ADIM 3: SP - sp_Cart_AddItem (Transactional)                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ BEGIN TRANSACTION                                          │  │
│  │                                                             │  │
│  │ 1. Toplam stok kontrolü:                                   │  │
│  │    SELECT SUM(Qty) FROM cdItemWarehouse                    │  │
│  │    WHERE ItemCode = @ItemCode                               │  │
│  │                                                             │  │
│  │ 2. Yeterli stok yoksa → ROLLBACK + Hata                    │  │
│  │                                                             │  │
│  │ 3. Depo dağıtımı (Warehouse Splitting):                    │  │
│  │    - Depolar stok miktarına göre sıralanır                 │  │
│  │    - İstenen miktar depolardan alınarak bölünür            │  │
│  │    WHILE @RemainingQty > 0                                  │  │
│  │    BEGIN                                                    │  │
│  │      -- En çok stoklu depodan al                           │  │
│  │      -- trShopCartLine'a INSERT                            │  │
│  │    END                                                      │  │
│  │                                                             │  │
│  │ 4. Header yoksa oluştur                                     │  │
│  │    INSERT INTO trShopCartHeader                            │  │
│  │                                                             │  │
│  │ 5. Sepet toplamlarını hesapla                              │  │
│  │    UPDATE trShopCartHeader SET TotalQty, TotalAmount       │  │
│  │                                                             │  │
│  │ COMMIT TRANSACTION                                          │  │
│  │                                                             │  │
│  │ -- Güncel sepeti döndür                                    │  │
│  │ SELECT * FROM trShopCartHeader WHERE UserCode = @User      │  │
│  │ SELECT * FROM trShopCartLine WHERE MasterLineId = @Id      │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ADIM 4: Response                                               │
│  {                                                               │
│    "success": true,                                              │
│    "cart": {                                                     │
│      "totalItems": 3,                                           │
│      "totalAmount": 1500.00,                                    │
│      "items": [...]                                             │
│    }                                                             │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📦 7. Sipariş Akışı (Order Flow)

### 7.1 Sipariş Durumları

| Kod | Durum | Açıklama | Sonraki Durumlar |
|-----|-------|----------|------------------|
| 0 | Pending | Yeni sipariş | 1, 3 |
| 1 | Approved | Onaylandı | 2, 3 |
| 2 | Shipped | Kargoya verildi | 4 |
| 3 | Cancelled | İptal edildi | - |
| 4 | Delivered | Teslim edildi | - |

### 7.2 Sipariş Oluşturma Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│  POST /api/orders (Sepeti siparişe çevir)                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. CreateOrderCommand                                           │
│     ├── UserCode                                                │
│     ├── CartId                                                  │
│     └── Notes                                                   │
│                                                                  │
│  2. Handler Kontrolleri:                                         │
│     ├── Sepet boş mu?                                           │
│     ├── Stoklar hala yeterli mi?                                │
│     └── Müşteri kredi limiti aşılmış mı?                        │
│                                                                  │
│  3. SP: sp_Order_CreateFromCart                                  │
│     ├── trShopCartHeader → UPDATE IsCompleted = 1               │
│     ├── Sipariş numarası üret                                   │
│     ├── Stok rezerve et (opsiyonel)                             │
│     └── Bildirim için flag set et                               │
│                                                                  │
│  4. E-posta bildirimi gönder (async)                            │
│                                                                  │
│  5. Response: { orderId: 12345, orderNumber: "SIP-2024-0001" }  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🛠️ 8. Yazılımcı Rehberi

### 8.1 Yeni Özellik Ekleme Adımları

```
┌─────────────────────────────────────────────────────────────────┐
│  ÖRNEK: "Müşteriye Adres Ekleme" özelliği                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ADIM 1: Veritabanı (10 dk)                                     │
│  ├── Tablo: cdCurrAccAddress                                    │
│  └── SP: sp_CustomerAddress_Insert                              │
│                                                                  │
│  ADIM 2: Domain (5 dk)                                          │
│  └── Entity: CustomerAddress.cs                                  │
│                                                                  │
│  ADIM 3: Application (20 dk)                                    │
│  ├── Command: CreateCustomerAddressCommand.cs                   │
│  ├── Validator: CreateCustomerAddressCommandValidator.cs        │
│  └── Handler: CreateCustomerAddressCommandHandler.cs            │
│                                                                  │
│  ADIM 4: Infrastructure (10 dk)                                 │
│  └── Repository: CustomerAddressRepository.cs                   │
│                                                                  │
│  ADIM 5: API (5 dk)                                             │
│  └── Endpoint: POST /api/customers/{code}/addresses             │
│                                                                  │
│  ADIM 6: Test (15 dk)                                           │
│  └── Unit test yaz                                               │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 8.2 Altın Kurallar

| ❌ YAPMA | ✅ YAP |
|----------|--------|
| Controller'da iş mantığı | Handler'da iş mantığı |
| Raw SQL string | Stored Procedure |
| Try-catch yutma | Global exception handler |
| Magic string | Enum veya constant |
| Console.WriteLine | ILogger kullan |

### 8.3 SP İsimlendirme Standardı

```
sp_[Modül]_[Eylem]

Örnekler:
├── sp_Auth_Login
├── sp_Customer_GetAll
├── sp_Customer_Insert
├── sp_Customer_Update
├── sp_Customer_Delete
├── sp_Cart_AddItem
├── sp_Cart_RemoveItem
├── sp_Order_Create
├── sp_Order_Approve
└── sp_Order_Cancel
```

---

## 📝 9. API Endpoint Listesi

```
Authentication:
  POST   /api/auth/login
  POST   /api/auth/refresh-token
  POST   /api/auth/logout

Customers (Cari Hesap):
  GET    /api/customers                    # Liste
  GET    /api/customers/{code}             # Detay
  POST   /api/customers                    # Ekle
  PUT    /api/customers/{code}             # Güncelle
  DELETE /api/customers/{code}             # Sil
  GET    /api/customers/{code}/balance     # Bakiye
  GET    /api/customers/{code}/addresses   # Adresler
  POST   /api/customers/{code}/addresses   # Adres ekle

Shopping Cart:
  GET    /api/cart                         # Sepeti getir
  POST   /api/cart/items                   # Ürün ekle
  PUT    /api/cart/items/{id}              # Miktar güncelle
  DELETE /api/cart/items/{id}              # Ürün sil
  DELETE /api/cart                         # Sepeti temizle

Orders:
  GET    /api/orders                       # Liste
  GET    /api/orders/{id}                  # Detay
  POST   /api/orders                       # Sepetten sipariş oluştur
  PUT    /api/orders/{id}/approve          # Onayla
  PUT    /api/orders/{id}/cancel           # İptal et
```

---

## 📊 10. Stored Procedure Listesi

| Modül | SP Adı | Açıklama |
|-------|--------|----------|
| Auth | sp_Auth_Login | Kullanıcı girişi |
| Auth | sp_Auth_LogLogin | Giriş logu kaydet |
| Customer | sp_Customer_GetAll | Müşteri listesi |
| Customer | sp_Customer_GetById | Müşteri detay |
| Customer | sp_Customer_Insert | Müşteri ekle |
| Customer | sp_Customer_Update | Müşteri güncelle |
| Customer | sp_Customer_Delete | Müşteri sil (soft) |
| Customer | sp_Customer_GetBalance | Bakiye sorgula |
| Cart | sp_Cart_GetByUser | Kullanıcı sepeti |
| Cart | sp_Cart_AddItem | Sepete ürün ekle |
| Cart | sp_Cart_UpdateItem | Miktar güncelle |
| Cart | sp_Cart_RemoveItem | Ürün sil |
| Cart | sp_Cart_Clear | Sepeti temizle |
| Order | sp_Order_Create | Sipariş oluştur |
| Order | sp_Order_Approve | Sipariş onayla |
| Order | sp_Order_Cancel | Sipariş iptal |
| Order | sp_Order_GetAll | Sipariş listesi |

---

## 🚀 11. Sonraki Adımlar

1. **Faz 1**: Solution yapısını oluştur
2. **Faz 2**: Auth modülünü tamamla (Login/JWT)
3. **Faz 3**: Customer CRUD işlemleri
4. **Faz 4**: Cart modülü
5. **Faz 5**: Order modülü
6. **Faz 6**: Raporlar ve Excel export
