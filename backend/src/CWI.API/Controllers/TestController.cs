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

    public TestController(CWIDbContext context)
    {
        _context = context;
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
            "ErrorLogs", "ApplicationLogs", "ImportJobs",
            "PaymentNotifications", "PaymentTransactions", "Payments",
            "StockAdjustmentItems", "StockAdjustments", "InventoryItems",
            "VendorPayments", "VendorInvoices", "GoodsReceiptItems", "GoodsReceipts", "PurchaseOrderItems", "PurchaseOrders",
            "OrderErpSyncs", "OrderTaxDetails", "OrderPackageItems", "OrderPackages", "OrderDeliveryRequests", "OrderShippingInfos", "OrderItems", "Orders",
            "CustomerTransactions",
            "ProductPrices", "ProductImages", "ProductNotes", "ProductAttributes", "ProductTranslations", "Products",
            "AttributeTranslations", "AttributeTypeTranslations", "AttributeTypes", "ColorTranslations", "Colors",
            "Announcements", "Banners"
        };

        foreach (var table in tables)
        {
#pragma warning disable EF1002 // Tablo adları hardcoded listeden geliyor, kullanıcı girdisi kullanılmıyor.
            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]");
#pragma warning restore EF1002
        }

        return Ok(new { message = "Database reset successful. Kept: Users, Roles, Customers, Warehouses, Brands, Settings." });
    }
}
