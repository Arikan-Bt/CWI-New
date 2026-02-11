# Packing List Yeniden Yapılandırma Planı
## sales-detail.component.ts → Packing List Modal

---

## MEVCUT DURUM

### Frontend Modal (satır 942-1088)
- **Section 1 - ORDER DETAILS tablosu:** 4 kolon (Item Code, ItemDesc, QTY, Carton NO input)
- **Section 2 - Cartons tablosu:** Manuel carton ekleme formu (CartonNo, Net Weight, Gross Weight, Measurements)
- Shipping details, invoice bilgisi, side mark **YOK**
- Otomatik 100'lük gruplama **YOK**
- Tüm carton/koli yönetimi tamamen **manuel**

### Backend
- `GetPackingListQuery.cs` → OrderItem'ları ve OrderPackage'ları çekiyor, düz liste döndürüyor
- `SavePackingListCommand.cs` → Carton CRUD + item-carton ilişkilendirme
- `OrderPackage` entity: PackageNumber, NetWeight, GrossWeight, Length, Width, Height
- `OrderPackageItem` entity: OrderPackageId, OrderItemId, Quantity

### Veri Modelleri
- `PackingListDto` → items[] + cartons[]
- `PackingListItemDto` → orderItemId, productCode, productName, qty, cartonNo
- `PackingListCartonDto` → id, cartonNo, netWeight, grossWeight, measurements

---

## HEDEF

PDF şablonuna uygun, 100'lük otomatik gruplama yapan packing list sistemi.

---

## DETAYLI PLAN

### ADIM 1: Backend — `OrderPackage` Entity'sine `SideMark` Alanı Ekleme

**Dosya:** `backend/src/CWI.Domain/Entities/Orders/OrderPackage.cs`

Yeni alan:
```csharp
public string? SideMark { get; set; }
```

**Dosya:** Yeni migration dosyası

```bash
dotnet ef migrations add AddSideMarkToOrderPackage
```

> **Neden:** Side mark bilgisi sipariş-paket seviyesinde saklanmalı. Aynı siparişin tüm kolilerinde ortak side mark gösterilecek. Alternatif olarak Order entity'sine de eklenebilir (tüm koliler için tek bir side mark). **Karar noktası: Side mark Order seviyesinde mi yoksa OrderPackage seviyesinde mi?** Muhtemelen Order seviyesinde daha mantıklı çünkü tek bir metin kutusu isteniyor.

→ **Öneri:** `Order` entity'sine `SideMark` (string, nullable) ekle.

---

### ADIM 2: Backend — `GetPackingListQuery` Güncelleme

**Dosya:** `backend/src/CWI.Application/Features/Reports/Queries/GetPackingListQuery.cs`

Değişiklikler:

1. **DTO genişletme:**
```csharp
public class PackingListDto
{
    // YENİ: Shipping / Header bilgileri
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? SideMark { get; set; }

    // MEVCUT
    public List<PackingListItemDto> Items { get; set; } = new();
    public List<PackingListCartonDto> Cartons { get; set; } = new();
}
```

2. **PackingListItemDto genişletme** — 100'lük bölünmüş satırlar:
```csharp
public class PackingListItemDto
{
    public long OrderItemId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Qty { get; set; }
    public string CartonNo { get; set; } = string.Empty;

    // YENİ: Otomatik gruplama alanları
    public int BoxQty { get; set; }          // Bu kutudaki adet (max 100)
    public int BoxIndex { get; set; }         // Kaçıncı kutu (1, 2, 3...)
    public int TotalBoxes { get; set; }       // Toplam kutu sayısı
    public bool IsManualOverride { get; set; } // Gift set/otomatik saat için manual flag
}
```

3. **Handler'da 100'lük bölme mantığı:**
```csharp
// Her ürün için 100'lük kutulara böl
foreach (var item in orderItems)
{
    int remaining = item.Quantity;
    int boxIndex = 1;
    int totalBoxes = (int)Math.Ceiling(remaining / 100.0);

    while (remaining > 0)
    {
        int boxQty = Math.Min(remaining, 100);
        result.Items.Add(new PackingListItemDto
        {
            OrderItemId = item.Id,
            ProductCode = item.Product.Sku,
            ProductName = item.ProductName,
            Qty = item.Quantity,        // Orijinal toplam
            BoxQty = boxQty,            // Bu kutudaki adet
            BoxIndex = boxIndex,
            TotalBoxes = totalBoxes,
            CartonNo = cartonMap...,     // Varsa mevcut carton eşleşmesi
            IsManualOverride = false
        });
        remaining -= boxQty;
        boxIndex++;
    }
}
```

4. **Order bilgilerini çek:**
```csharp
var order = await _unitOfWork.Repository<Order, long>().AsQueryable()
    .Include(x => x.Customer)
    .Include(x => x.ShippingInfo)
    .FirstOrDefaultAsync(x => x.Id == request.OrderId);

result.OrderNumber = order.OrderNumber;
result.OrderDate = order.OrderedAt;
result.CustomerName = order.Customer?.Name ?? "";
result.ShippingAddress = order.ShippingInfo?.ShippingAddress;
result.SideMark = order.SideMark;
```

---

### ADIM 3: Backend — `SavePackingListCommand` Güncelleme

**Dosya:** `backend/src/CWI.Application/Features/Reports/Commands/SavePackingListCommand.cs`

Değişiklikler:

1. **Command'e SideMark ekle:**
```csharp
public class SavePackingListCommand : IRequest<bool>
{
    public long OrderId { get; set; }
    public string? SideMark { get; set; }   // YENİ
    public List<SavePackingListItemDto> Items { get; set; } = new();
    public List<SavePackingListCartonDto> Cartons { get; set; } = new();
}
```

2. **Handler'da SideMark'ı Order'a kaydet:**
```csharp
var order = await _unitOfWork.Repository<Order, long>().GetByIdAsync(request.OrderId);
order.SideMark = request.SideMark;
```

3. **SavePackingListCartonDto'ya Kolon 7-8 alanları:**
```csharp
public class SavePackingListCartonDto
{
    // MEVCUT
    public long Id { get; set; }
    public string CartonNo { get; set; } = string.Empty;
    public decimal? NetWeight { get; set; }
    public decimal? GrossWeight { get; set; }
    public string Measurements { get; set; } = string.Empty;

    // YENİ: Depo tarafından güncellenecek (Kolon 7 & 8)
    // Bunlar zaten mevcut alanlara map ediliyor, ek alan gerekirse buraya eklenir
}
```

---

### ADIM 4: Frontend — Model Güncellemeleri

**Dosya:** `frontend/src/app/core/models/orders-report.models.ts`

```typescript
export interface PackingListDto {
  // YENİ: Header bilgileri
  orderNumber: string;
  orderDate: string;
  customerName: string;
  shippingAddress?: string;
  sideMark?: string;

  // MEVCUT
  items: PackingListItemDto[];
  cartons: PackingListCartonDto[];
}

export interface PackingListItemDto {
  orderItemId: number;
  productCode: string;
  productName: string;
  qty: number;
  cartonNo: string;

  // YENİ
  boxQty: number;           // Bu kutudaki adet
  boxIndex: number;          // Kaçıncı kutu
  totalBoxes: number;        // Toplam kutu sayısı
  isManualOverride: boolean; // Manuel düzenleme flag'i
}

export interface SavePackingListCommand {
  orderId: number;
  sideMark?: string;         // YENİ
  items: { orderItemId: number; cartonNo: string; qty: number }[];
  cartons: PackingListCartonDto[];
}
```

---

### ADIM 5: Frontend — Packing List Modal Yeniden Tasarımı

**Dosya:** `frontend/src/app/pages/sales/sales-detail/sales-detail.component.ts`

Mevcut modal (satır 942-1088) tamamen yeniden yapılandırılacak:

#### Section 0: HEADER — Shipping Details
```
┌──────────────────────────────────────────────────────────┐
│  Invoice #: [ORDER-2025-001]    Date: [10.02.2026]       │
│  Customer: [ABC Trading Co.]                             │
│  Shipping Address: [123 Main St, Dubai, UAE]             │
└──────────────────────────────────────────────────────────┘
```
- Tüm alanlar **read-only** (B2B'den otomatik)
- `selectedOrder()` ve yeni `packingListHeader` signal'inden beslenecek

#### Section 1: SIDE MARK — Textarea (Text Box)
```
┌──────────────────────────────────────────────────────────┐
│  SIDE MARK:                                              │
│  ┌────────────────────────────────────────────────────┐  │
│  │ (Textarea - çok satırlı metin kutusu)              │  │
│  │ Müşterinin side mark bilgisi...                    │  │
│  │                                                     │  │
│  └────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```
- `<textarea pTextarea>` kullanılacak (input değil)
- Kaydedilecek (`SideMark` alanı)

#### Section 2: ANA TABLO — 8 Kolon
```
┌────┬──────────┬───────────┬─────┬─────┬─────┬──────────────┬─────────┐
│ #  │ Carton   │ Product   │ Desc│ QTY │ QTY │ Net Weight   │ Gross   │
│    │ No       │ Code      │     │(PCS)│/Box │ (Manual-Col7)│Weight   │
│    │ (Auto)   │ (Auto)    │(Auto│(Auto│(Auto│              │(Man-C8) │
├────┼──────────┼───────────┼─────┼─────┼─────┼──────────────┼─────────┤
│ 1  │ CTN-001  │ SKU-100   │Watch│ 350 │ 100 │ [input]      │ [input] │
│ 2  │ CTN-002  │ SKU-100   │Watch│ 350 │ 100 │ [input]      │ [input] │
│ 3  │ CTN-003  │ SKU-100   │Watch│ 350 │ 100 │ [input]      │ [input] │
│ 4  │ CTN-004  │ SKU-100   │Watch│ 350 │  50 │ [input]      │ [input] │
│ 5  │ CTN-005  │ SKU-200   │Gift │  30 │  30 │ [input]      │ [input] │
└────┴──────────┴───────────┴─────┴─────┴─────┴──────────────┴─────────┘
```

**8 Kolon Detayı:**
| Kolon | Alan | Kaynak | Düzenlenebilir? |
|-------|------|--------|-----------------|
| 1 | # (Sıra No) | Otomatik | Hayır |
| 2 | Carton No | Otomatik (CTN-001, CTN-002...) | Hayır* |
| 3 | Product Code | B2B sipariş verisi | Hayır |
| 4 | Description | B2B sipariş verisi | Hayır |
| 5 | QTY (PCS) - Orijinal toplam | B2B sipariş verisi | Hayır |
| 6 | QTY / Box (100'lük bölünmüş) | Otomatik hesaplama | Hayır* |
| 7 | Net Weight | Depo tarafından | **EVET** |
| 8 | Gross Weight | Depo tarafından | **EVET** |

> *Gift set/otomatik saat ürünleri için kolon 6 (QTY/Box) düzenlenebilir olacak (isManualOverride = true)

#### 100'lük Gruplama Mantığı (Frontend computed signal):
```typescript
packingListRows = computed(() => {
  const items = this.packingListItems();
  const rows: PackingListRow[] = [];
  let cartonCounter = 1;

  for (const item of items) {
    if (item.isManualOverride) {
      // Gift set / otomatik saat → tek satır, QTY düzenlenebilir
      rows.push({
        ...item,
        cartonNo: `CTN-${String(cartonCounter++).padStart(3, '0')}`,
        boxQty: item.qty,
        editable: true
      });
    } else {
      // Normal ürün → 100'lük grupla
      let remaining = item.qty;
      while (remaining > 0) {
        const boxQty = Math.min(remaining, 100);
        rows.push({
          ...item,
          cartonNo: `CTN-${String(cartonCounter++).padStart(3, '0')}`,
          boxQty,
          editable: false
        });
        remaining -= boxQty;
      }
    }
  }
  return rows;
});
```

---

### ADIM 6: Frontend — Component Logic Güncellemeleri

**Dosya:** `frontend/src/app/pages/sales/sales-detail/sales-detail.component.ts`

Değişecek signal'ler ve metotlar:

```typescript
// YENİ signal'ler
packingListHeader = signal<{
  orderNumber: string;
  orderDate: string;
  customerName: string;
  shippingAddress: string;
}>({ orderNumber: '', orderDate: '', customerName: '', shippingAddress: '' });

sideMark = signal('');

// GÜNCELLENMİŞ onPackingList()
onPackingList() {
  // API'den gelen veriye header + sideMark ata
  this.packingListHeader.set({
    orderNumber: res.data.orderNumber,
    orderDate: res.data.orderDate,
    customerName: res.data.customerName,
    shippingAddress: res.data.shippingAddress || ''
  });
  this.sideMark.set(res.data.sideMark || '');
  this.packingListItems.set(res.data.items);
  // ...
}

// GÜNCELLENMİŞ onSavePackingList()
onSavePackingList() {
  const command: SavePackingListCommand = {
    orderId: order.orderId,
    sideMark: this.sideMark(),
    items: ...,
    cartons: ...
  };
}
```

---

### ADIM 7: Migration & Test

1. `Order` entity'sine `SideMark` alanı ekle
2. EF Core migration oluştur
3. Backend build & test
4. Frontend build & test

---

## DOSYA DEĞİŞİKLİK ÖZETİ

| Dosya | İşlem |
|-------|-------|
| `backend/.../Orders/Order.cs` | `SideMark` alanı ekle |
| `backend/.../Queries/GetPackingListQuery.cs` | DTO genişlet + 100'lük bölme + header bilgileri |
| `backend/.../Commands/SavePackingListCommand.cs` | `SideMark` kaydetme ekle |
| `frontend/.../models/orders-report.models.ts` | Interface'lere yeni alanlar |
| `frontend/.../services/report.service.ts` | Değişiklik yok (aynı endpoint) |
| `frontend/.../sales-detail/sales-detail.component.ts` | Modal UI + logic yeniden yapılandır |
| Yeni migration dosyası | `AddSideMarkToOrder` |

**Toplam:** 6 dosya güncelleme + 1 migration

---

## SORULAR / KARAR NOKTALARI

1. **8 kolonun tam isimleri nedir?** PDF şablonunu göremediğim için yukarıdaki kolon isimlerini varsaydım. PDF'i tarayıcıda açıp doğrulayabilir misiniz?
2. **Side Mark** Order seviyesinde mi (tüm koliler için tek) yoksa koli başına mı?
3. **Gift set / otomatik saat** ürünlerini nasıl ayırt edeceğiz? Ürün kategorisi mi, manuel flag mi?
4. **Measurements (L*W*H)** kolonu 8 kolonun içinde mi yoksa ayrı mı?
