using CWI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CWI.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly CWIDbContext _context;
    private readonly IWebHostEnvironment _env;

    public TestController(CWIDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    /// <summary>
    /// Veritabanını test için sıfırlar (Sadece Development ortamında çalışır)
    /// </summary>
    /// <returns></returns>
    [HttpPost("reset-db")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetDb()
    {
        var tables = new List<string>
        {
            // 1. Logs & Jobs
            "ErrorLogs", "ApplicationLogs", "ImportJobs", 
            
            // 2. Payments (Siparişe bağımlı olabilir, önce silinmeli)
            "PaymentNotifications", "PaymentTransactions", "Payments",
            
            // 3. Inventory Transactions
            "StockAdjustmentItems", "StockAdjustments", "InventoryItems",
            
            // 4. Purchasing Transactions
            "VendorPayments", "VendorInvoices", "GoodsReceiptItems", "GoodsReceipts", "PurchaseOrderItems", "PurchaseOrders",
            
            // 5. Sales Transactions
            "OrderErpSyncs", "OrderTaxDetails", "OrderPackageItems", "OrderPackages", "OrderDeliveryRequests", "OrderShippingInfos", "OrderItems", "Orders",
            
            // 6. Customer Transactions
            "CustomerTransactions",
            
            // 7. Products (Stoklara ve Sipariş kalemlerine bağlıdır, onlar yukarıda silindi)
            "ProductPrices", "ProductImages", "ProductNotes", "ProductAttributes", "ProductTranslations", "Products",
            
            // 8. Attributes & Colors
            "AttributeTranslations", "AttributeTypeTranslations", "AttributeTypes", "ColorTranslations", "Colors",
            
            // 9. Content
            "Announcements", "Banners"
        };

        foreach (var table in tables)
        {
            try
            {
                // Tablo mevcut olmayabilir veya silme sırası yanlış olabilir diye tek tek deniyoruz
                await _context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]");
            }
            catch (Exception ex)
            {
                // Log the error but continue if possible or return error
                return StatusCode(500, new { error = $"Error deleting from table {table}", detail = ex.Message });
            }
        }

        return Ok(new { message = "Database reset successful. Kept: Users, Roles, Customers, Warehouses, Brands, Settings." });
    }
}
