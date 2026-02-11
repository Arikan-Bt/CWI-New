# PROJE TEKNİK ANALİZİ VE MİGRASYON REHBERİ

Bu belge, mevcut ASP.NET MVC projesinin her bir modülünü, veri akışını ve iş kurallarını satır satır analiz ederek, yeni .NET 9.0 mimarisine ve "Tamamen Stored Procedure (SP)" tabanlı yapıya geçiş için yol haritası sunar.

---

## 1. Sistemin Genel Yapısı ve Kullanımdaki Roller

Sistem, `MainController.Index` metodunda görüldüğü üzere `LoggUserType` değişkeni üzerinden belirlenen 5 farklı kullanıcı rolüne göre şekillenmektedir:

| Kullanıcı Tipi (ID) | Tanım | Yönlendirilen Ekran | Ana İşlevler |
| :--- | :--- | :--- | :--- |
| **0** | **Admin / Yönetici** | `Settings/Orders` | Tüm siparişleri görme, raporlama, cari listeleme. |
| **1** | **Ofis Kullanıcısı** | `OfficeView` | Onay bekleyen siparişleri (`UnApprovedOrders`) yönetme. |
| **2** | **Plasiyer / Satış Temsilcisi** | `Settings/Orders` | Kendine bağlı carilerin siparişlerini yönetme. |
| **3** | **Bayi (Dealer)** | `DealerView` | **(Ana Odak)** Ürün gezme, sepete ekleme, sipariş verme. |
| **4** | **Excel Entegratörü** | `Excel` | Toplu sipariş yükleme modülü. |

---

## 2. DETAYLI İŞ AKIŞLARI VE TEKNİK ANALİZ (TRACE)

Aşağıdaki akışlar, mevcut kodun çalıştırılma sırasına göre (Execution Trace) çıkarılmıştır. Her adımda mevcut durumun teknik karşılığı ve yeni sistemde yapılması gerekenler belirtilmiştir.

### Senaryo A: Sisteme Giriş (Authentication Flow)

**Amaç**: Kullanıcının doğrulanması ve oturum bağlamının (Context) yüklenmesi.

1.  **Kullanıcı**: `Account/Index` sayfasına girer.
2.  **Frontend**: Kullanıcı Adı ve Şifre girer -> `Account/CheckLogin` (AJAX/POST)
3.  **Backend**: `AccountController.CheckLogin(UserName, Password)`
    *   **Adım 3.1 (Sorgu)**: Kullanıcıyı veritabanında arar.
        *   *Mevcut*: Inline SQL (`SELECT * FROM cdUser WHERE ...`).
        *   *Yapılacak*: `sp_Auth_CheckUser` prosedürü oluşturulacak.
    *   **Adım 3.2 (Şifre Kontrolü)**:
        *   Gelen şifre `AppHelper.DecodeString` (AES Decryption) ile çözülen veritabanı şifresiyle kıyaslanır.
        *   **Kritik Güvenlik Notu**: "11223344" backdoor şifresi kaldırılmalı.
    *   **Adım 3.3 (Loglama)**:
        *   *Mevcut*: Inline SQL (`INSERT prUserLogin ...`).
        *   *Yapılacak*: `sp_Log_UserLogin` prosedürü.
    *   **Adım 3.4 (Bağlam Yükleme)**:
        *   `SetCurrAcc(UserCurrAccCode)`: Kullanıcının Cari Hesabı Session'a yüklenir.
        *   `UserShopCartSet()`:
            *   *Mevcut*: `trShopCartHeader` tablosundan `IsCompleated=0` olan kayıt, `GetActivePreOrder` ve `GetActive` metotlarıyla sorgulanır. Bulunursa `trShopCartLine` detayları çekilir.
            *   *Kullanılan SQL*: Inline SQL (`SELECT * FROM trShopCartLine WHERE ...`).
            *   *Yapılacak*: `sp_Cart_GetActiveSession` (Header ve Detayları tek seferde veya result set olarak dönmeli).

---

### Senaryo B: Ürün Listeleme ve Vitrin (Dealer Dashboard)

**Amaç**: Bayinin ürünleri görmesi ve kategorilerde gezmesi.

1.  **Kullanıcı**: Giriş yaptıktan sonra `Main/Index` yönlenir (Tip 3).
2.  **Backend**: `MainController.Index`
    *   **Adım 2.1 (Vitrin)**: `GetCaruselDate` çağrılır.
        *   *Mevcut*: `MainController` içinde method. Muhtemelen `cdItem` ve stok tablolarına JOIN atıyor.
        *   *Yapılacak*: `sp_Catalog_GetFeaturedItems`.
    *   **Adım 2.2 (Menü)**: `GetMainCategory` çağrılır.
        *   *Mevcut*: Kategorileri (`MenuItem`) listeler.
        *   *Yapılacak*: `sp_Catalog_GetCategories`.
3.  **Frontend**: `Views/Main/DealerView.cshtml` render edilir.

---

### Senaryo C: Sepete Ürün Ekleme (Core Commerce Flow) - EN KRİTİK

**Amaç**: Bayinin bir ürünü, stok kontrolü yapılarak sepete eklemesi. Mevcut sistemde en karmaşık mantık buradadır.

1.  **Kullanıcı**: Ürün detayında veya listede "Sepete Ekle" butonuna basar.
2.  **Frontend**: `Main/AddProductCart` (AJAX) -> Parametreler: `itemCode`, `qty`.
3.  **Backend**: `MainController.AddProductCart`
    *   **Adım 3.1 (Stok Durumu)**: `GetProductWareHouseStatus(itemCode)` çağrılır.
        *   *Mevcut*: Ürünün hangi depoda ne kadar stoğu olduğunu bir List olarak döner.
        *   *Yapılacak*: Bu sorgu ana SP'nin içine gömülecek.
    *   **Adım 3.2 (Sepet Kontrolü)**: `GlobalVariables.UserCart` kontrol edilir. Header yoksa oluşturulur (`ShopCartHeader...SetActive()`).
    *   **Adım 3.3 (Mevcut Ürün Temizliği)**:
        *   *Mevcut Logic*: Eğer sepette ürün zaten varsa, önce **silinir** (`trShopCartLine` delete), sonra yeni adetle tekrar eklenir.
    *   **Adım 3.4 (Warehouse Splitting / Depo Bölme Mantığı)**:
        *   *Mevcut C# Logic*: `WareHouseStatus` listesi `Available` stoğa göre çoktan aza sıralanır.
        *   Döngü kurulur: İstenen `qty` karşılanana kadar depolar gezilir.
            *   Depo A (Stok 5): İstenen 10 -> 5 al, kalanı 5. (Insert Depo A)
            *   Depo B (Stok 8): Kalan 5 -> 5 al, kalanı 0. (Insert Depo B)
    *   **Adım 3.5 (Kayıt)**:
        *   Hesaplanan her parçalı satır için `ShopCartLine.Save()` çağrılır.
        *   *Mevcut*: `ShopCartLine.Save` içinde Inline SQL (`IF EXISTS UPDATE ELSE INSERT`).
    
    #### **MİGRASYON PLANI (YENİ SİSTEM)**
    Bu tüm mantık tek bir Stored Procedure içine taşınmalıdır: **`sp_Cart_AddItem`**.
    *   **Parametreler**: `@UserCode`, `@ItemCode`, `@Qty`, `@SessionId`
    *   **SP İçeriği**:
        1.  Bir `ActiveCart` var mı bak, yoksa oluştur (`IF NOT EXISTS INSERT Header`).
        2.  Ürünün stok durumunu Temp table veya CTE'ye al.
        3.  Cursor veya While döngüsü ile stoğu depolara dağıt.
        4.  `trShopCartLine` tablosuna gerekli `INSERT` işlemlerini transactional olarak yap.
        5.  Sonuç olarak güncel sepet özetini dön.

---

### Senaryo D: Sipariş Onaylama (Checkout)

**Amaç**: Sepetin siparişe dönüştürülmesi.

1.  **Kullanıcı**: Sepet detayında "Siparişi Onayla" der.
2.  **Frontend**: `Main/ApproveCart` (AJAX) -> Parametreler: `MasterLine` (Sepet ID), `LongDesc1` (Notlar).
3.  **Backend**: `MainController.ApproveCart`
    *   **Adım 3.1**: `IsCompleated` Flag güncellemesi.
        *   *Mevcut*: Inline SQL (`UPDATE trShopCartHeader SET [IsCompleated]=1 ...`).
        *   *Mevcut*: Inline SQL (`INSERT trShopCartDetail ...` notlar için).
    *   **Adım 3.2**: Toplamların Hesaplanması.
        *   *Mevcut*: Inline SQL (`UPDATE trShopCartHeader ... FROM (Select Sum(Qty)...)`) başlık tablosundaki toplamları satırlardan toplayarak günceller.
    *   **Adım 3.3**: Session temizliği (`UserShopCartSet`).
    
    #### **MİGRASYON PLANI**
    Yeni SP: **`sp_Order_Checkout`**.
    *   **İşlem**: Transaction içinde Header status güncelle, notları kaydet, stokları düş (eğer rezervasyon yapılıyorsa), toplamları doğrula ve hesapla.

---

### Senaryo E: Excel Sipariş Yükleme (Custom Integration)

1.  **Kullanıcı**: Excel dosyasını yükler.
2.  **Backend**: `AppHelper.CreatePurchaseOrder` veya türevleri.
    *   *Mevcut*: `sp_ExcelItemCheckPurchasePreOrder` ile ürün kodlarını denetler. `sp_CreatePurchaseOrder` ile siparişi oluşturur.
    *   *Not*: Burası zaten SP kullanıyor, ancak yeni yapıdaki tablo isimlerine ve mantığa göre SP'lerin güncellenmesi gerekecek.

---

## 3. SQL Nesneleri Envanteri (Dönüştürülecekler)

Mevcut projede tespit edilen ve yeni sisteme taşınarak standardize edilmesi gereken SQL işlemleri:

1.  **Tablolar** (Prefix değişebilir, Clean Architecture'da Domain Entity olacaklar):
    *   `cdUser`, `cdUserGroup` -> `Users`, `UserGroups`
    *   `trShopCartHeader`, `trShopCartLine`, `trShopCartDetail` -> `Orders`, `OrderItems`, `OrderDetails`
    *   `ProductWareHouseStatus` (View veya Tablo?) -> `Inventory`
2.  **Mevcut SP'ler** (Hepsi korunup revize edilecek):
    *   `sp_ExcelItemCheck...`
    *   `sp_CreatePurchaseOrder`
    *   `sp_ItemCheckStatus`
3.  **Oluşturulacak Yeni SP'ler (Inline SQL yerine)**:
    *   `sp_Auth_Login`
    *   `sp_Cart_GetActive`
    *   `sp_Cart_AddItem` (Logic içeren kompleks SP)
    *   `sp_Cart_RemoveItem`
    *   `sp_Order_Checkout`
    *   `sp_Account_GetBalance`

---

## 4. Yeni Proje Kurulumu İçin Checklist

1.  **Solution**: .NET 9.0 Web API (Core).
2.  **Architecture**: Onion / Clean Architecture (Domain, Application, Infrastructure, Presentation).
3.  **Data Access**: Dapper (Performans ve SP odaklı olduğu için Dapper, EF Core'dan daha uygun olabilir; ancak kullanıcı EF Core istiyorsa Raw SQL/SP desteği kullanılacak).
4.  **CQRS**: MediatR ile `EvaluateOrderCommand`, `GetProductListQuery` gibi ayrıştırmalar yapılacak.
