using CWI.Domain.Common;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Identity;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Lookups;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Payments;
using CWI.Domain.Entities.Products;
using CWI.Domain.Entities.Purchasing;
using CWI.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace CWI.Infrastructure.Persistence;

/// <summary>
/// Ana veritabanı context sınıfı
/// </summary>
public class CWIDbContext : DbContext
{
    public CWIDbContext(DbContextOptions<CWIDbContext> options) : base(options)
    {
    }
    
    #region Customers Module
    
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerTransaction> CustomerTransactions => Set<CustomerTransaction>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<CustomerPricing> CustomerPricings => Set<CustomerPricing>();
    public DbSet<CustomerDiscount> CustomerDiscounts => Set<CustomerDiscount>();
    
    #endregion
    
    #region Products Module
    
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<ColorTranslation> ColorTranslations => Set<ColorTranslation>();
    public DbSet<AttributeType> AttributeTypes => Set<AttributeType>();
    public DbSet<AttributeTypeTranslation> AttributeTypeTranslations => Set<AttributeTypeTranslation>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<AttributeTranslation> AttributeTranslations => Set<AttributeTranslation>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductNote> ProductNotes => Set<ProductNote>();
    
    #endregion
    
    #region Orders Module
    
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderShippingInfo> OrderShippingInfos => Set<OrderShippingInfo>();
    public DbSet<OrderDeliveryRequest> OrderDeliveryRequests => Set<OrderDeliveryRequest>();
    public DbSet<OrderPackage> OrderPackages => Set<OrderPackage>();
    public DbSet<OrderPackageItem> OrderPackageItems => Set<OrderPackageItem>();
    public DbSet<OrderTaxDetail> OrderTaxDetails => Set<OrderTaxDetail>();
    public DbSet<OrderErpSync> OrderErpSyncs => Set<OrderErpSync>();
    
    #endregion
    
    #region Inventory Module
    
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<WarehouseBrand> WarehouseBrands => Set<WarehouseBrand>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentItem> StockAdjustmentItems => Set<StockAdjustmentItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    
    #endregion
    
    #region Payments Module
    
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<PaymentNotification> PaymentNotifications => Set<PaymentNotification>();
    public DbSet<BankConfiguration> BankConfigurations => Set<BankConfiguration>();
    public DbSet<BankBinCode> BankBinCodes => Set<BankBinCode>();
    
    #endregion
    
    #region Purchasing Module
    
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();
    public DbSet<VendorInvoice> VendorInvoices => Set<VendorInvoice>();
    public DbSet<VendorPayment> VendorPayments => Set<VendorPayment>();
    
    #endregion
    
    #region Identity Module
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserBrandAccess> UserBrandAccesses => Set<UserBrandAccess>();
    public DbSet<UserRegionAccess> UserRegionAccesses => Set<UserRegionAccess>();
    public DbSet<UserWarehouseAccess> UserWarehouseAccesses => Set<UserWarehouseAccess>();
    public DbSet<SalesTarget> SalesTargets => Set<SalesTarget>();
    public DbSet<UserLoginHistory> UserLoginHistories => Set<UserLoginHistory>();
    
    #endregion
    
    #region Lookups Module
    
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<LocalizedString> LocalizedStrings => Set<LocalizedString>();
    public DbSet<ShipmentTerm> ShipmentTerms => Set<ShipmentTerm>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Banner> Banners => Set<Banner>();
    
    #endregion
    
    #region System Module
    
    public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    
    #endregion
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Tüm konfigürasyonları otomatik olarak uygula
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CWIDbContext).Assembly);
        
        // Global query filter: Soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDeletable.IsActive));
                var filter = System.Linq.Expressions.Expression.Lambda(property, parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        // Decimal alanlar için hassasiyet (Precision) ayarı
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Global: Cascade delete engelleme (SQL Server döngü hatalarını önlemek için)
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit bilgilerini otomatik olarak güncelle
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}
