# CWI (Arıkan) – İş Akışları, Ekran Kılavuzu ve Yeniden Yapılandırma (To‑Be) Dokümanı

> **Kapsam**: Bu doküman; mevcut sistemin (ASP.NET MVC .NET Framework 4.5) ekranda görünen iş akışlarını “As‑Is” olarak çıkarır, ardından sıfırdan geliştirilecek **temiz arayüz + .NET 9.0 Web API + CQRS (MediatR) + (hibrit) Stored Procedure** hedef mimarisini “To‑Be” olarak tarif eder.  
> **Hedef okur**: Hem **normal kullanıcı** (ekran ekran ne yapılacağını anlamak için) hem de **geliştirici/analist** (yeni sistemi kurmak ve SP/CQRS tasarlamak için).

---

## 0) Terimler (Sözlük)

- **Cari / Cari Hesap (CurrAcc)**: Bayi/Müşteri hesabı. Kod: `CurrAccCode`, ad: `CurrAccDescription`.
- **Bayi (Dealer)**: Ürünleri görüntüleyip sepet/sipariş oluşturan kullanıcı tipi.
- **Plasiyer (Salesman)**: Kendisine bağlı cariler için siparişleri takip eden kullanıcı tipi.
- **Ofis (Office)**: Onay/işleme alınacak siparişleri görüntüleyen kullanıcı tipi.
- **Admin**: Tüm sipariş/rapor/yönetim ekranlarını görebilen kullanıcı tipi.
- **Sepet (Cart)**: Siparişe dönüşmemiş satırların bulunduğu alan. Mevcut sistemde DB tablosu: `trShopCartHeader` + `trShopCartLine`.
- **Ön Sipariş (Pre‑Order)**: Ayrı bir sepet/sipariş akışı. Mevcut sistemde ayrıca “PreOrder” header/line mantığı ve `StatusCode = -1`.
- **SP (Stored Procedure)**: SQL Server üzerinde prosedür.
- **TVP (Table‑Valued Parameter)**: Excel’den toplu satır aktarımında kullanılan tablo parametresi.

---

## 1) Mevcut Sistem (As‑Is) – Genel Yapı

### 1.1 Teknoloji
- Backend + UI: `Arikan` projesi (ASP.NET MVC, .NET Framework 4.5).
- Veri erişimi: Karışık; bazı yerlerde **SP**, bazı yerlerde **inline SQL string birleştirme**.
- Oturum: `Session` + bazı `Cookie`’ler.
- Dosya saklama (dekont/fatura): Ağ paylaşımı `\\arikancdn\\cwi\\Receipt` ve CDN URL’i `https://cdn.arikantime.com/cwi/Receipt/...`.

### 1.2 Rol Türleri (Mevcut)
Sistem, kullanıcı tipini `cdUserGroup.GroupUserType` üzerinden belirler ve `MainController.Index` içinde ekrana yönlendirir.

| Kod | Rol | Açılış ekranı | Ana amaç |
|---:|---|---|---|
| 0 | Admin | `Settings/Orders` | Tüm siparişler/raporlar + kullanıcı yönetimi |
| 1 | Office | `Main/OfficeView` | Onay bekleyen siparişleri takip etme |
| 2 | Salesman | `Settings/Orders` | Sipariş takibi (genelde kendi carileri) |
| 3 | Dealer | `Main/DealerView` | Ürün gezme + sepet + sipariş |
| 4 | Excel | `Main/Excel` | Excel ile toplu işlemler |

### 1.3 Ekran Haritası (Mevcut View’lar)
Kritik ekranlar (tam liste `Arikan/Views/Main/*.cshtml` içindedir):

- Giriş: `Views/Account/Index.cshtml`
- Bayi vitrin: `Views/Main/DealerView.cshtml`
- Kategori liste: `Views/Category/Index.cshtml`, `Views/Category/PreOrderCategory.cshtml`
- Sepet: `Views/Main/UserCart.cshtml`, `Views/Main/UserCartPreOrder.cshtml`
- Sipariş onayı: `Views/Main/Order.cshtml`, `Views/Main/OrderPreOrder.cshtml`
- Sipariş yönetimi: `Views/Settings/Orders.cshtml` (+ partial’lar)
- Bakiye: `Views/Main/CurrAccBalance*.cshtml`
- Ödeme / Dekont: `Views/Main/Payment*.cshtml`, `Views/Main/PaymentNotification*.cshtml`
- Tedarik/Satınalma: `Views/Main/Purchase*.cshtml`, `Views/Main/PurchaseOrderEntry.cshtml`
- Paketleme (Koli): `Views/Main/PackingList.cshtml`
- Kullanıcı yönetimi: `Views/Settings/UserLists.cshtml` (+ partial’lar)

### 1.4 Yetki Matrisi (As‑Is / Gözlenen)
Bu tablo, mevcut controller yönlendirmeleri ve ekranlardan gözlenen “fiili” davranışı özetler (iş birimiyle teyit edilmelidir).

| İşlem | Admin | Office | Salesman | Dealer | Excel |
|---|:---:|:---:|:---:|:---:|:---:|
| Giriş yapma | ✓ | ✓ | ✓ | ✓ | ✓ |
| Cari seçme/değiştirme | ✓ | ✓ | ✓ | (genelde yok) | ✓ |
| Ürün gezme/arama | ✓ | ✓ | ✓ | ✓ | ✓ |
| Sepete ürün ekleme | ✓ | ✓ | ✓ | ✓ | ✓ (toplu) |
| Sipariş onaylama | ✓ | ✓ | ✓ | ✓ | ✓ (toplu) |
| Onay bekleyen siparişleri görme | ✓ | ✓ | (kısıtlı olabilir) | ✗ | ✗ |
| Sipariş durumu güncelleme | ✓ | ✓ | (kısıtlı olabilir) | ✗ | ✗ |
| Bakiye/ekstre görme | ✓ (tümü) | ✓ (tümü) | ✓ (kendi) | ✓ (kendi) | ✓ |
| Ödeme/dekont ekleme | ✓ | ✓ | ✓ | (var) | ✓ |
| Vendor invoice ekleme | ✓ | ✓ | ✓ | (var) | ✓ |
| Satınalma/Purchase işlemleri | ✓ | ✓ | ✓ | ✗ | ✓ |
| Kullanıcı yönetimi | ✓ | (var) | ✗ | ✗ | ✗ |

### 1.5 Ekran Bazlı Kılavuz (As‑Is / Kullanıcı Gözüyle)
Aşağıdaki bölüm, kullanıcıların “hangi ekranda ne yapacağını” hızlıca anlaması için ekranları özetler.

| Ekran | Yol / View | Kim kullanır? | Amaç | Tipik girdiler/çıktılar |
|---|---|---|---|---|
| Giriş | `Account/Index` | Herkes | Sisteme giriş | Kullanıcı adı, şifre |
| Bayi vitrin | `Main/DealerView` | Dealer | Kategori + vitrin ürünleri | Ürün kartları, sepet göstergesi |
| Kategori liste | `Category/Index` | Dealer | Kategoride ürün listeleme | Arama filtresi, sayfalama |
| Ürün detay (modal) | `Main/ProductItemModal` | Herkes | Ürün detayını görmek | Fiyat/stock/özellik |
| Sepet | `Main/UserCart` | Dealer | Sepeti görmek/düzenlemek | Satır sil, sepet temizle |
| Sipariş | `Main/Order` | Dealer | Sepetten siparişe geçiş | Not, ödeme yöntemi vb. (ekrana göre) |
| Ön sipariş | `Main/OrderPreOrder` | Dealer | Ön sipariş oluşturma | Ön sipariş satırları |
| Siparişler | `Settings/Orders` | Admin/Office/Salesman | Sipariş listesi/filtre | Tarih, cari, durum |
| Sipariş detay | `Settings/OrdersDetail*` | Admin/Office/Salesman | Siparişi inceleme | Satırlar, durum, not |
| OfficeView | `Main/OfficeView` | Office | Onay bekleyen siparişler | Liste + hızlı aksiyon |
| Bakiye | `Main/CurrAccBalance*` | Herkes | Cari ekstre/bakiye | Tarih aralığı, export |
| Ödeme | `Main/Payment` | Admin/Office/Salesman/Dealer | Ödeme girme | Tutar, tarih, dekont |
| Ödeme bildirimi | `Main/PaymentNotification` | Herkes | Referans bazlı ödeme notu | Referans kodu, açıklama |
| Vendor invoice | `Main/VendorInvoice` | Admin/Office/Salesman/Dealer | Fatura dosyası ekleme | Fatura no, tarih, tutar |
| Satınalma giriş | `Main/PurchaseOrderEntry` | Admin/Office/Excel | Excel ile PO oluşturma | Excel dosyası, cari, tarih |
| Satınalma listeleri | `Main/Purchase*` | Admin/Office | Satınalma/irsaliye/fatura süreçleri | Kaydet, güncelle, sil |
| Packing list | `Main/PackingList` | Office/Depo | Koli/paketleme | Koli no, ölçü/ağırlık, export |
| Kullanıcı listesi | `Settings/UserLists` | Admin | Kullanıcı CRUD | Şifre, e‑posta, cari |
| Marka yetki | `Settings/UserBrandList` | Admin | Kullanıcı‑marka eşlemesi | Marka, kullanıcı seçimi |

---

## 2) Mevcut Sistem (As‑Is) – İş Akışları (Kullanıcı Odaklı)

Bu bölüm “normal kullanıcı” için yazılmıştır. Her senaryonun sonunda, “Sistemde Ne Oluyor?” kısmı teknik özet verir.

### Senaryo A — Giriş Yapma

**Kullanıcı adımları**
1. Giriş sayfasını açar (`/Account/Index`).
2. Kullanıcı adı ve şifreyi girer.
3. “Giriş”e basar.

**Sistemde ne oluyor? (özet)**
- `AccountController.CheckLogin(UserName, Password)` çalışır.
- Kullanıcı; `cdUser` + `cdUserGroup` join ile bulunur.
- Şifre kontrolü: DB’deki `UserPass` AES ile çözülür (`Apphelper.DecodeString`) ve girilen şifreyle karşılaştırılır.
- Başarılıysa:
  - `Session["LoginedUser"]` set edilir.
  - `prUserLogin` tablosuna log atılır.
  - `ARIKANUSER` cookie yazılır (20 yıl).
  - Kullanıcının cari hesabı oturuma yüklenir (`SetCurrAcc(UserCurrAccCode)`).
  - Aktif sepet(ler) oturuma alınır (`UserShopCartSet()`).

**Notlar / riskler**
- Kodda “backdoor” şifre kontrolü görülüyor: `11223344` (yeni sistemde kesinlikle kaldırılmalı).
- Inline SQL yoğun (yeni sistemde SP + parametreli çağrı şart).

### Senaryo B — Dil Değiştirme

**Kullanıcı adımları**
1. Dil seçer.

**Sistemde ne oluyor?**
- `ChangeLanguage(LangCode)` çağrılır; `AppCurrLanguage` cookie’si set edilir.

### Senaryo C — Cari Seçmek / Cari Değiştirmek (Özellikle Admin/Office/Salesman)

**Kullanıcı adımları**
1. Cari listesi olan ekranda cari seçer (ör. ödeme ekranı, rapor ekranı).

**Sistemde ne oluyor?**
- Cari listesi ya `SalesmenCurrAccounts.SelectAll()` ya da `GetSalesmenCurrAccounts` prosedürü ile gelir.
- Seçim sonrası `Account/SetCurrAcc` çalışır:
  - Seçilen cari `Session["SalesmenCurrAccounts"]` içine alınır.
  - Sepetler yeniden yüklenir (`UserShopCartSet()`).

### Senaryo D — Ürünleri Görüntüleme (Bayi Vitrini)

**Kullanıcı adımları**
1. Giriş sonrası bayi ana ekranına gelir.
2. Kategorileri görür, kategoriye girer.

**Sistemde ne oluyor?**
- `MainController.Index` (UserType=3) -> `DealerView`.
- Vitrin ürünleri: `sp_CaruselDate`.
- Kategoriler: `sp_GetMainCategory`.
- Kategori ağacı: `sp_GetMainCatSubTree`, `sp_GetSubCatTree` ve ürün listeleri: `sp_ProductTree*`.

### Senaryo E — Ürün Arama

**Kullanıcı adımları**
1. Arama kutusuna ürün kodu/açıklama yazar.
2. Sonuçları listeler.

**Sistemde ne oluyor?**
- Normal ürün: `sp_SearchProducts`
- Ön sipariş: `sp_SearchPreOrderProducts`

### Senaryo F — Sepete Ürün Ekleme (Normal Sepet) – “Depo Bölme” dahil

**Kullanıcı adımları**
1. Ürün kartında “Sepete Ekle” der.
2. Adet girer (veya +/‑ yapar).
3. Sepette satır(lar) oluşur.

**Sistemde ne oluyor?**
- `MainController.AddProductCart(itemCode, qty)` çalışır.
- Stok/depo durumu alınır: `sp_ProductWareHouseStatus`.
- Aktif sepet yoksa header açılır (`ShopCartHeader.SetActive()`).
- Aynı ürün sepette birden fazla satırsa önce temizlenir (delete/clear).
- **Depo bölme algoritması**:
  - Depolar “Available” stoğa göre büyükten küçüğe sıralanır.
  - İstenen `qty`, depoların stoklarıyla parçalara bölünerek `trShopCartLine`’a birden fazla satır olarak eklenir.
  - Örn: İstenen 10; DepoA=5 -> 5 satırı, DepoB=8 -> 5 satırı.
- Sonuç partial view ile sepet güncellenir.

**Kullanıcıya görünen uyarılar**
- Stok yoksa: “Inventory cannot be negative” benzeri mesaj.
- Stok kısmi ise: stok kadar eklenip bilgilendirme yapılır.

### Senaryo G — Sepetten Ürün Çıkarma / Sepeti Temizleme

**Kullanıcı adımları**
- Satır bazında “Sil” -> satır kalkar.
- “Sepeti Temizle” -> sepet tamamen sıfırlanır.

**Sistemde ne oluyor?**
- Satır sil: `RemoveProductCart(itemCode, wareHouseCode)` -> `ShopCartLine.DeleteItem()`.
- Sepeti temizle: `ClearCart()` -> `ShopCartHeader.ClearLines()` (line + header delete) + ilgili cache key’leri temizlenir.

### Senaryo H — Siparişi Onaylama (Checkout)

**Kullanıcı adımları**
1. Sepette “Siparişi Onayla” der.
2. Not alanlarını doldurur.

**Sistemde ne oluyor?**
- `ApproveCart(MasterLine, LongDesc1, LongDesc2)`
  - `trShopCartHeader.IsCompleated = 1`
  - `trShopCartDetail` içine notlar eklenir
  - Header toplamları line’lardan tekrar hesaplanır ve güncellenir (`TotalQty/TotalAmount/NetAmount/TaxBase`)
  - Oturum sepeti yeniden yüklenir.

### Senaryo I — Ön Sipariş (Pre‑Order) Sepeti ve Onayı

**Kullanıcı adımları**
1. Ön sipariş ekranından ürün ekler.
2. Ön siparişi onaylar.

**Sistemde ne oluyor?**
- Ön sipariş sepete ekleme: `AddProductCartPreOrder` / `AddProductCartOrderPreOrder`.
- Ön sipariş onayı: `ApproveCartPreOrder`:
  - `IsCompleated = 1`, `StatusCode = -1`
  - Notlar `trShopCartDetail`’e eklenir
  - Sepet sıfırlanır/yenilenir.

### Senaryo J — Siparişleri Görüntüleme / Durum Takibi (Admin/Salesman/Office)

**Kullanıcı adımları**
1. Siparişler ekranına gider.
2. Cari/durum/tarih filtreleri uygular.
3. Detaya girer.

**Sistemde ne oluyor?**
- `SettingsController.Orders`:
  - Durum listesi: `cdOrderStatus`
  - Cari listesi: `SalesmenCurrAccounts.SelectAll()`
  - Liste: `UserShopCart.LastOrders(...)` (DB’den header listesi)
- `SettingsController.OrdersDetail(RecNo)`:
  - Header + satırlar + ödeme/teslimat/ek bilgiler yüklenir.

### Senaryo K — Sipariş Üzerinde Düzenleme / Durum Güncelleme (Admin/Office)

**Kullanıcı adımları**
- Siparişe ekstra iskonto uygular.
- Teslim/Sevkiyat şartı seçer.
- Durumu değiştirir (onay, shipped, iptal vb.).

**Sistemde ne oluyor? (kritik noktalar)**
- `MainController.OrderDetail(id)` içinde:
  - `trShopCartLine` satırlarında indirim/vergisel alanlar güncellenebilir.
  - `trShopCartDetail.ExtraDiscount` set edilebilir.
  - `trShopCartAddition.RequestedDate` kaydedilir.
- Durum güncelleme farklı SQL bloklarıyla yapılır; shipped olduğunda `sp_InsertOrder` çağrısı tetiklenir.
- İptalde `IsCancelled=1`, `CancelReason` set edilir.

### Senaryo L — Bakiye Görüntüleme (Cari Ekstresi)

**Kullanıcı adımları**
1. Bakiye ekranına girer.
2. Tarih aralığı seçer.

**Sistemde ne oluyor?**
- Dealer: `sp_CurrAccBalance(startDate, endDate, VendorCurrAcc)`
- Admin: aynı SP ama seçilen cari ile; ayrıca “tüm cariler” için `sp_CurrAccBalanceAll`.

### Senaryo M — Ödeme Bildirimi / Ödeme Ekleme / Dekont Yükleme

**Kullanıcı adımları**
1. Ödeme ekranında cari seçer.
2. Tutar, makbuz no, tarih vb. girer.
3. Dekont dosyasını yükler.

**Sistemde ne oluyor?**
- Dosya `\\arikancdn\\cwi\\Receipt` altına kaydedilir (adı genelde cari + referans).
- `cdPayment` veya `cdPaymentVendor` tablolarına kayıt atılır (inline SQL insert).
- “Dekont görüntüle”de, dosya uzantılarını deneyerek CDN URL’i döndürülür (`GetReceiptFile`).

### Senaryo N — Vendor Invoice (Fatura) Yükleme ve Bakiye Takibi

**Kullanıcı adımları**
1. Vendor Invoice ekranında fatura ekler (dosya + meta).
2. Vendor invoice balance ekranında filtreleyerek bakar.

**Sistemde ne oluyor?**
- Dosya `\\arikancdn\\cwi\\Receipt` altına kaydedilir (`VI...`).
- `cdVendorInvoice` tablosuna insert atılır.
- Bakiye listesi: `sp_VendorInvoiceBalance`.

### Senaryo O — Excel ile Toplu İşlemler (Sipariş / Satınalma / Stok Düzeltme)

**Kullanıcı adımları**
1. Excel dosyası yükler.
2. Sistem satırları kontrol eder.
3. Hata yoksa toplu işlem oluşur.

**Sistemde ne oluyor?**
- Excel okuma: `OfficeOpenXml (EPPlus)` ile satırlar parse edilir.
- Kontrol ve import SP’leri:
  - Satınalma kontrol: `sp_ExcelItemCheckPurchasePreOrder` (TVP: `ItemCodeWith_Price`)
  - Satınalma oluştur: `sp_CreatePurchaseOrder`
  - Stok düzeltme kontrol: `sp_ExcelImportStockAdjustment` (TVP: `ItemForStockAdjustment`)
  - Stok güncelle: `sp_WareHouseUpdate`
  - Toplu sipariş kontrol: `sp_ExcelItemCheck`
  - Toplu sipariş import: `sp_ExcelImportOrderWithPrice`

### Senaryo P — Satınalma Faturası Kaydetme ve Depoya İşleme (Mikro Entegrasyonu)

**Kullanıcı adımları**
1. Satınalma ekranında sipariş seçer.
2. Fatura numarası girer, satırlara miktar/bedel yazar.
3. “Kaydet” der.

**Sistemde ne oluyor?**
- `SavePurchaseInvoice(...)`:
  - Header oluşturulur: `getPurchaseInvoiceHeader(...)`
  - Satırlar `cdPurchaseLine`’a kaydedilir.
  - Ardından depoya işleme için prosedür çağrısı yapılır: `sp_InsertItemintoWarehouse`.
- Ayrıca bazı güncellemeler doğrudan harici DB’ye atılır: `[MikroDB_V15_02]..STOK_HAREKETLERI`.

### Senaryo Q — Packing List (Koli/Paketleme) Yönetimi

**Kullanıcı adımları**
1. Sipariş detayından “Packing List”e gider.
2. Koli oluşturur, ölçü/ağırlık girer.
3. Satırları koliye bağlar, export alır.

**Sistemde ne oluyor?**
- `PackingList(id)`:
  - Sipariş + koli header/line listeleri yüklenir.
- `CartoonLines(...)` / `SaveShopCartoonLines(...)` ile koli satırları kaydedilir.
- Excel export üretilir.

### Senaryo R — POP Order (Özel Sipariş Akışı)

**Kullanıcı adımları**
1. Cari seçer, ürün listesinden miktar girer.
2. Siparişi kaydeder.

**Sistemde ne oluyor?**
- `SavePopOrder(...)`:
  - Pop header/line tablolarına kayıt
  - Sonrasında ERP/entegrasyon amaçlı: `MARIS.dbo.sp_InsertOrder` çağrısı

### Senaryo S — Kullanıcı Yönetimi (Admin)

**Kullanıcı adımları**
1. “Kullanıcı Listesi” ekranına gider.
2. Yeni kullanıcı ekler veya mevcut kullanıcıyı günceller/siler.
3. Kullanıcıya bir “cari” bağlar (kullanıcının çalışacağı cari).

**Sistemde ne oluyor?**
- Ekran: `Settings/UserLists`.
- Liste: `cdUser.SelectAll()` (join ile grup tipi dahil).
- CRUD:
  - Insert: `cdUser.InsertItem()` -> `cdUser` tablosuna insert (şifre uygulama içinde AES ile encode edilip byte[] yazılıyor).
  - Update: `cdUser.UpdateItem()`
  - Delete: `cdUser.DeleteItem()`
- Not: Bu akışta DB erişimi inline SQL ile (string birleştirme) yapılıyor; To‑Be’de SP’ye taşınmalı.

### Senaryo T — Kullanıcı‑Marka Yetkilendirme (Admin)

**Kullanıcı adımları**
1. “Marka Yetki” ekranına gider.
2. Marka seçer.
3. Kullanıcı(ları) markaya ekler veya çıkarır.

**Sistemde ne oluyor?**
- Ekran: `Settings/UserBrandList`.
- Kayıt: `cdUserBrands` tablosuna insert/delete (`UserBrand.InsertItem/DeleteItem`).

### Senaryo U — Eksik Görsel Listesi (Admin)

**Kullanıcı adımları**
1. “Image List Settings” ekranına gider.
2. Görseli eksik ürünleri listeler.

**Sistemde ne oluyor?**
- `SettingsController.ImageListSettings`
- Kaynak sorgu: `select * from view_AllItems where FileExists=0`

### Senaryo V — Excel/Export Raporları (Sipariş, Packing List, Ekstre)

**Kullanıcı adımları**
1. Rapor/ekstre ekranında “Excel’e Aktar” benzeri butona basar.
2. Excel dosyası indirir.

**Sistemde ne oluyor?**
- Bazı ekranlarda EPPlus ile Excel üretilip `Response.BinaryWrite(...)` ile indirilir.
- Örnekler:
  - Sipariş export: `OrderExportExcel`, `OrdersExportExcel` vb. view/action’lar
  - Packing list export: `ExportPackingList`
  - Ekstre export: `CurrAccBalanceExportExcel*`

---

## 3) Mevcut Sistem (As‑Is) – Teknik Envanter (Özet)

### 3.1 Oturumda Tutulan Kritik Nesneler
Kaynak: `Arikan/Controllers/BaseController.cs` -> `GlobalVariables`

- `GlobalVariables.loginedUser` (`Session["LoginedUser"]`)
- `GlobalVariables.VendorUser` (seçili cari; `Session["SalesmenCurrAccounts"]`)
- `GlobalVariables.UserCart` ve `UserCartPreOrder`

### 3.2 Tespit Edilen Prosedürler (Koddan Çıkarılanlar)
Bu liste kod içinde direkt referanslanan prosedürleri kapsar (rapor amaçlı).

- Katalog: `sp_CaruselDate`, `sp_GetMainCategory`, `sp_GetMainCatSubTree`, `sp_GetSubCatTree`, `sp_ProductTree*`, `sp_ProductDetail`, `sp_SearchProducts`, `sp_SearchPreOrderProducts`
- Stok/Depo: `sp_ProductWareHouseStatus`, `sp_ProductWareHouseStatusPreOrder`
- Cari bakiye: `sp_CurrAccBalance`, `sp_CurrAccBalanceAll`, `sp_CurrAccBalanceInvoiceDetails`
- Sipariş/entegrasyon: `sp_InsertOrder`, `sp_InsertItemintoWarehouse`, `MARIS.dbo.sp_InsertOrder`
- Satınalma/Excel: `sp_ExcelItemCheck`, `sp_ExcelImportOrderWithPrice`, `sp_ExcelItemCheckPurchasePreOrder`, `sp_CreatePurchaseOrder`, `sp_ExcelImportStockAdjustment`, `sp_WareHouseUpdate`
- Diğer: `GetSalesmenCurrAccounts`, `Get_CustomerOrders`, `Get_CustomerOrdersHeader`, `Get_CustomerPurchase`, `Get_PurchaseOrderInvoiveLine`, `Get_PurchaseOrderInvoiveHeader`

### 3.3 Mevcut DB Tablolarından Öne Çıkanlar (İsimler As‑Is)
- Kullanıcı: `cdUser`, `cdUserGroup`, `cdUserBrands`
- Cari: `cdCurrAcc`, `prCurrAccBalance`
- Sepet/Sipariş: `trShopCartHeader`, `trShopCartLine`, `trShopCartDetail`, `trShopCartAddition`
- Ödeme/Fatura: `cdPayment`, `cdPaymentVendor`, `cdVendorInvoice`
- Satınalma: `cdPurchase`, `cdPurchaseLine`

---

## 4) Hedef Sistem (To‑Be) – Ürün: “Sıfırdan Temiz UI + .NET 9.0 + CQRS/MediatR + Hibrit SP”

### 4.1 Üst Seviye Hedefler
- UI ve backend ayrışsın (temiz arayüz, API-first).
- Tüm DB erişimi **parametreli SP** üzerinden olsun (SQL injection riskini sıfırlamak için).
- İş akışları C# tarafında **CQRS + MediatR** ile yönetilsin (validasyon, yetki, loglama, audit).
- Performans kritik alanlarda (özellikle sepet/sipariş/rapor) SP odaklı tasarım.

### 4.2 Önerilen Katmanlı Yapı (Clean Architecture)

```
src/
  Cwi.Domain/                 # Entity, ValueObject, Enum, Interface
  Cwi.Application/            # CQRS: Commands/Queries + Handlers + Validation
  Cwi.Infrastructure/         # Dapper, SP çağrıları, repository implementasyonları
  Cwi.Api/                    # Web API (JWT, controllers, middleware)
  Cwi.Web/                    # Temiz UI (React/Vue/Blazor - proje kararı)
database/
  StoredProcedures/
  Types/                      # TVP type scriptleri
  Migrations/                 # İsteğe bağlı (schema versioning)
docs/
  CWI-Is-Akislari.md          # Bu dokümanın “master” kopyası
```

### 4.3 Kimlik Doğrulama (JWT) – To‑Be

**UI akışı**
1. Kullanıcı giriş olur.
2. API token verir; UI token’ı saklar.
3. Seçili cari değişince token içindeki “aktif cari” claim’i güncellenir (veya ayrı endpoint).

**Önerilen API**
- `POST /api/auth/login` -> `LoginCommand`
- `POST /api/auth/switch-customer` -> `SwitchCustomerCommand`

**Önerilen SP’ler**
- `sp_Auth_Login(@UserName, @Password)` -> user + rol + izinler
- `sp_Auth_LogLogin(@UserCode, @IpAddress, @LoginAt)` -> audit/log

### 4.4 Cari Yönetimi (Yeni Gereksinim) – To‑Be

Mevcut sistemde cari verisi büyük ölçüde “okunuyor” gibi (seçme/bakiye), fakat **cari CRUD** ekranı yok. Yeni sistemde cari yönetimi uygulama içinden yapılacak.

**Temel kullanıcı hikâyeleri**
- Admin/Office: Cari ekle, güncelle, pasife al, adres/iletişim yönet, salesman ataması yap.
- Salesman: Kendisine atanmış carileri görür.
- Dealer: Sadece kendi cari bilgisini ve bakiyesini görür.

**Önerilen API**
- `GET /api/customers?query=` -> `GetCustomersQuery`
- `GET /api/customers/{id}` -> `GetCustomerByIdQuery`
- `POST /api/customers` -> `CreateCustomerCommand`
- `PUT /api/customers/{id}` -> `UpdateCustomerCommand`
- `DELETE /api/customers/{id}` -> `DeactivateCustomerCommand` (hard delete yerine)
- `POST /api/customers/{id}/assign-salesman` -> `AssignSalesmanCommand`

**Önerilen SP’ler**
- `sp_Customer_GetAll(@Query, @SalesmanUserCode, @Page, @PageSize)`
- `sp_Customer_GetById(@CustomerId)`
- `sp_Customer_Insert(...)`
- `sp_Customer_Update(...)`
- `sp_Customer_Deactivate(@CustomerId, @Reason)`
- `sp_Customer_AssignSalesman(@CustomerId, @SalesmanUserCode)`
- (opsiyonel) `sp_Customer_GetBalance(@CurrAccCode, @StartDate, @EndDate)`

### 4.5 Sepet / Sipariş – To‑Be (En Kritik Alan)

**Tasarım kararı**
- “Depo bölme” mantığı **tek bir transaction** içinde çalışmalı.
- Aynı ürünün sepette “çoklu depo satırı” üretmesi desteklenmeli (mevcut davranış korunuyor).

**Önerilen CQRS**
- `AddCartItemCommand(UserId, CustomerId, ItemCode, Qty, Mode)`  
  - Mode: `Normal | PreOrder`
- `RemoveCartItemCommand(CartId, ItemCode, WarehouseCode)`
- `ClearCartCommand(CartId)`
- `CheckoutCommand(CartId, Notes, PaymentMethod, ShipmentTerms, RequestedDate)`

**Önerilen SP’ler (minimum set)**
- `sp_Cart_GetActive(@UserId, @CustomerId, @Mode)` -> header + lines
- `sp_Cart_AddItem(@UserId, @CustomerId, @ItemCode, @Qty, @Mode)`  
  - İçeride: stok/depoyu al, depo böl, satırları upsert et, toplamları güncelle.
- `sp_Cart_RemoveItem(@CartId, @ItemCode, @WarehouseCode)`
- `sp_Cart_Clear(@CartId)`
- `sp_Order_Checkout(@CartId, @Notes1, @Notes2, @PaymentMethodId, @ShipmentTermsId, @RequestedDate)`
- `sp_Order_RecalculateTotals(@OrderId)` (opsiyonel; tek noktadan toplam hesap)

### 4.6 Ödeme / Dekont / Fatura Dosyaları – To‑Be

**Dosya yönetimi önerisi**
- Ağ paylaşımı yerine: S3/Blob gibi obje depolama (veya kurum içi dosya servisi) + DB’de metadata.

**Önerilen API**
- `POST /api/payments` (multipart) -> `CreatePaymentCommand`
- `GET /api/payments/{id}/file` -> signed url / proxy download
- `POST /api/vendor-invoices` (multipart) -> `CreateVendorInvoiceCommand`

**Önerilen SP’ler**
- `sp_Payment_Insert(...)`
- `sp_PaymentVendor_Insert(...)`
- `sp_VendorInvoice_Insert(...)`
- `sp_VendorInvoiceBalance(@StartDate, @EndDate, @CurrAccCode?)`

### 4.7 Excel / Toplu İşlemler – To‑Be

**Tasarım ilkesi**
- Excel parse UI/API katmanında yapılır; doğrulama ve toplu insert/update **TVP + SP** ile yapılır.

**Önerilen API**
- `POST /api/imports/purchase-order` -> `ImportPurchaseOrderCommand`
- `POST /api/imports/stock-adjustment` -> `ImportStockAdjustmentCommand`
- `POST /api/imports/order` -> `ImportOrderCommand`

**Önerilen DB**
- TVP type’lar:
  - `dbo.ItemCodeWithPrice (ItemCode nvarchar(…), Qty int, Price decimal(…))`
  - `dbo.ItemForStockAdjustment (ItemCode nvarchar(…), Qty int, ...)`
- SP’ler:
  - `sp_Import_PurchaseOrder_Validate` / `sp_Import_PurchaseOrder_Apply`
  - `sp_Import_StockAdjustment_Validate` / `sp_Import_StockAdjustment_Apply`
  - `sp_Import_Order_Validate` / `sp_Import_Order_Apply`

### 4.8 Satınalma + Mikro Entegrasyonu – To‑Be

Bu alan mevcut projede “işin gerçeği” olarak duruyor: stok hareketlerinin bir kısmı harici DB’ye yazılıyor. Yeni sistemde bu entegrasyon netleştirilmeli:

- Mikro’ya yazım **doğrudan SQL** ile mi sürecek?
- Yoksa Mikro servisleri/connector ile mi yapılacak?

**Öneri**
- Mikro yazımını ayrı bir “Integration” bileşeni yapın (uygulama katmanından event/command ile tetiklenir).
- DB bağlantıları/şemaları environment bazlı konfigürasyondan gelsin.

### 4.9 Temiz Arayüz (UI) – Önerilen Menü ve Sayfalar (To‑Be)

Yeni UI; rol bazlı menü göstermeli ve kullanıcıyı “tek bakışta” yönlendirmelidir.

**Ortak (tüm roller)**
- Giriş / Şifre güncelleme
- Profil (ad‑soyad, e‑posta, telefon)
- Dil seçimi

**Dealer (Bayi)**
- Ana Sayfa (vitrin)
- Katalog
  - Kategori ağacı
  - Ürün arama
  - Ürün detay (modal/sayfa)
- Sepet
  - Satır düzenleme (adet, depo satırı)
  - Sepeti temizle
- Siparişlerim
  - Sipariş listesi (durum filtreli)
  - Sipariş detay
- Cari Bakiye / Ekstre (kendi cari)
- Ödeme (dekont yükleme) + Ödeme bildirimleri

**Salesman / Office / Admin**
- Sipariş Yönetimi
  - Filtreleme: tarih, cari, durum, marka
  - Sipariş detay: satırlar, notlar, kargo/teslim, ekstra iskonto
  - Aksiyonlar: durum güncelle, iptal, shipped, export
- Cari Yönetimi (yeni)
- Ödeme/Fatura Yönetimi
- Satınalma / Depo / Packing List
- Kullanıcı ve Yetki Yönetimi (Admin)

### 4.10 CQRS (MediatR) – Proje Standardı (To‑Be)

**Önerilen kurallar**
- Her use‑case için tek “Command/Query”:
  - Örn: `AddCartItemCommand`, `CheckoutCommand`, `CreateCustomerCommand`
- Validation: FluentValidation ile `Application` katmanında.
- Yetkilendirme: role/permission kontrolü `Application` pipeline behavior ile (MediatR Behavior).
- Loglama/Audit: yine Behavior ile; SP tarafında da kritik transaction’lar audit tablosuna yazabilir.
- Sonuç tipi: tek tip `Result<T>` veya `ApiResponse<T>` (hata kodu + mesaj + detay).

### 4.11 Stored Procedure Sözleşmesi – Şablon (To‑Be)

SP’ler; UI’nın değil **API/CQRS’ın** ihtiyacına göre tasarlanmalı ve mümkün olduğunca “tek çağrıda” ihtiyaç duyulan veriyi döndürmelidir.

**Önerilen genel prensipler**
- Tüm SP’ler parametreli olmalı (string birleştirme yok).
- Transaction gerektiren işlemler (sepet ekleme, checkout, stok düşme) SP içinde `BEGIN TRAN/COMMIT/ROLLBACK` ile kapsanmalı.
- Standart output:
  - Başarı/hata kodu (`@ResultCode`)
  - Mesaj (`@ResultMessage`)
  - Gerekirse dönen kimlik (`@Id`)
- Çoklu sonuç seti gerekiyorsa (header + lines) tek SP içinde 2 result set kullanılabilir.

**Örnek: `sp_Cart_AddItem` (depo bölme dahil)**
- Girdiler: `@UserId`, `@CustomerId`, `@ItemCode`, `@Qty`, `@Mode`
- İçerik: Aktif sepeti bul/oluştur → stok/depo dağıt → satırları upsert → header toplamlarını güncelle
- Çıktılar: güncel sepet header + sepet satırları (result set)

**Örnek: `sp_Order_Checkout`**
- Girdiler: `@CartId`, `@Notes1`, `@Notes2`, `@PaymentMethodId`, `@ShipmentTermsId`, `@RequestedDate`
- İçerik: sepeti siparişe çevir, durumu set et, toplamları doğrula
- Çıktı: `@OrderId` + sipariş referansı

---

## 5) “As‑Is → To‑Be” Eşleme Tablosu (Kısa)

| Mevcut MVC Action | Yeni API Endpoint | CQRS | Önerilen SP |
|---|---|---|---|
| `Account.CheckLogin` | `POST /api/auth/login` | `LoginCommand` | `sp_Auth_Login` |
| `Account.SetCurrAcc` | `POST /api/auth/switch-customer` | `SwitchCustomerCommand` | `sp_Customer_GetById` |
| `Main.AddProductCart` | `POST /api/cart/items` | `AddCartItemCommand` | `sp_Cart_AddItem` |
| `Main.RemoveProductCart` | `DELETE /api/cart/items` | `RemoveCartItemCommand` | `sp_Cart_RemoveItem` |
| `Main.ClearCart` | `DELETE /api/cart` | `ClearCartCommand` | `sp_Cart_Clear` |
| `Main.ApproveCart` | `POST /api/orders/checkout` | `CheckoutCommand` | `sp_Order_Checkout` |
| `Settings.Orders*` | `GET /api/orders` | `GetOrdersQuery` | `sp_Order_GetAll` (tasarlanacak) |
| `Main.CurrAccBalance*` | `GET /api/customers/{id}/balance` | `GetBalanceQuery` | `sp_CurrAccBalance` |

---

## 6) Dokümanın “Yapılacaklar” Listesi (Uygulama Başlatmadan Önce)

1. **Rol/yetki matrisi** son halini iş birimiyle onaylayın (özellikle cari CRUD kimin yetkisinde?).
2. **Sipariş durumları** (`cdOrderStatus` + özel kodlar) yeni sistemde enum/lookup olarak netleştirilsin.
3. “Depo bölme” kuralı yazılı hale getirilsin (kısmi stokta davranış, öncelik, depo öncelik sırası).
4. “Mikro entegrasyonu” için tek yaklaşım seçilsin (SQL mi, servis mi).
5. Dosya saklama/CDN stratejisi kararı (ağ paylaşımı devam mı?).

