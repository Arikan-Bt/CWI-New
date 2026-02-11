using CWI.Application.Common.Models;
using CWI.Application.DTOs.Reports;
using CWI.Application.Features.Reports.Queries;
using CWI.Application.Features.Reports.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IMediator mediator, ILogger<ReportsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("orders")]
    public async Task<ActionResult<Result<OrdersReportResponse>>> GetOrdersReport([FromBody] OrdersReportRequest request)
    {
        var result = await _mediator.Send(new GetOrdersReportQuery { Request = request });
        return Ok(Result<OrdersReportResponse>.Succeed(result));
    }

    [HttpGet("orders/export")]
    public async Task<IActionResult> ExportOrdersReport([FromQuery] OrdersReportRequest request)
    {
        var fileContent = await _mediator.Send(new ExportOrdersReportQuery { Request = request });
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"OrdersReport_{DateTime.Now:yyyyMMddHHmm}.xlsx");
    }

    [HttpGet("orders/{orderId}/details")]
    public async Task<ActionResult<Result<OrderDetailResponse>>> GetOrderDetails(long orderId, [FromQuery] string? brand, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetOrderDetailQuery { 
            OrderId = orderId, 
            Brand = brand,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return Ok(Result<OrderDetailResponse>.Succeed(result));
    }

    [HttpGet("customers")]
    public async Task<ActionResult<Result<List<CustomerLookupDto>>>> GetCustomers([FromQuery] bool onlyVendors = false)
    {
        var result = await _mediator.Send(new GetCustomersLookupQuery { OnlyVendors = onlyVendors });
        return Ok(Result<List<CustomerLookupDto>>.Succeed(result));
    }

    [HttpGet("order-statuses")]
    public ActionResult<Result<List<object>>> GetOrderStatuses()
    {
        var statuses = new List<object>
        {
            new { label = "Load All", value = (string?)null },
            new { label = "Pre Order", value = CWI.Domain.Enums.OrderStatus.PreOrder.ToString() },
            new { label = "Pending", value = CWI.Domain.Enums.OrderStatus.Pending.ToString() },
            new { label = "Packed & Waiting Shipment", value = CWI.Domain.Enums.OrderStatus.PackedAndWaitingShipment.ToString() },
            new { label = "Shipped", value = CWI.Domain.Enums.OrderStatus.Shipped.ToString() },
            new { label = "Canceled", value = CWI.Domain.Enums.OrderStatus.Canceled.ToString() },
            new { label = "Draft", value = CWI.Domain.Enums.OrderStatus.Draft.ToString() }
        };
        return Ok(Result<List<object>>.Succeed(statuses));
    }

    [HttpGet("products")]
    public async Task<ActionResult<Result<List<ProductLookupDto>>>> GetProducts()
    {
        var result = await _mediator.Send(new GetProductsLookupQuery());
        return Ok(Result<List<ProductLookupDto>>.Succeed(result));
    }

    [HttpGet("payment-methods")]
    public async Task<ActionResult<Result<List<object>>>> GetPaymentMethods()
    {
        var result = await _mediator.Send(new GetPaymentMethodsLookupQuery());
        return Ok(Result<List<object>>.Succeed(result));
    }

    [HttpGet("shipment-terms")]
    public async Task<ActionResult<Result<List<object>>>> GetShipmentTerms()
    {
        var result = await _mediator.Send(new GetShipmentTermsLookupQuery());
        return Ok(Result<List<object>>.Succeed(result));
    }

    [HttpPost("stock")]
    public async Task<ActionResult<Result<StockReportResponse>>> GetStockReport([FromBody] StockReportRequest request)
    {
        var result = await _mediator.Send(new GetStockReportQuery { Request = request });
        return Ok(Result<StockReportResponse>.Succeed(result));
    }

    [HttpGet("stock/export")]
    public async Task<IActionResult> ExportStockReport([FromQuery] StockReportRequest request)
    {
        var fileContent = await _mediator.Send(new ExportStockReportQuery { Request = request });
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"StockReport_{DateTime.Now:yyyyMMddHHmm}.xlsx");
    }

    [HttpPost("stock/update-note")]
    public async Task<ActionResult<Result<bool>>> UpdateStockNote([FromBody] UpdateStockNoteRequest request)
    {
        var result = await _mediator.Send(new UpdateStockNoteCommand { Request = request });
        return Ok(Result<bool>.Succeed(result));
    }

    [HttpGet("brands")]
    public async Task<ActionResult<Result<List<BrandLookupDto>>>> GetBrands()
    {
        var result = await _mediator.Send(new GetBrandsQuery());
        return Ok(Result<List<BrandLookupDto>>.Succeed(result));
    }

    [HttpPost("purchase-order-invoices")]
    public async Task<ActionResult<Result<PurchaseOrderInvoiceReportResponse>>> GetPurchaseOrderInvoicesReport([FromBody] PurchaseOrderInvoiceReportRequest request)
    {
        var result = await _mediator.Send(new GetPurchaseOrderInvoiceReportQuery { Request = request });
        return Ok(Result<PurchaseOrderInvoiceReportResponse>.Succeed(result));
    }



    [HttpPost("customer-balance")]
    public async Task<ActionResult<Result<CustomerBalanceReportResponse>>> GetCustomerBalanceReport([FromBody] CustomerBalanceReportRequest request)
    {
        var result = await _mediator.Send(new GetCustomerBalanceReportQuery { Request = request });
        return Ok(Result<CustomerBalanceReportResponse>.Succeed(result));
    }

    [HttpGet("customer-balance/export")]
    public async Task<IActionResult> ExportCustomerBalanceReport([FromQuery] CustomerBalanceReportRequest request)
    {
        var fileContent = await _mediator.Send(new ExportCustomerBalanceReportQuery { Request = request });
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CustomerBalanceReport_{DateTime.Now:yyyyMMddHHmm}.xlsx");
    }

    [HttpPost("payment-details")]
    public async Task<ActionResult<Result<CustomerPaymentDetailReportResponse>>> GetCustomerPaymentDetailsReport([FromBody] CustomerPaymentDetailReportRequest request)
    {
        var result = await _mediator.Send(new GetCustomerPaymentDetailsQuery { Request = request });
        return Ok(Result<CustomerPaymentDetailReportResponse>.Succeed(result));
    }

    /// <summary>
    /// Müşteriye ait balance > 0 olan referansları getirir
    /// Add Payment modal'ında Reference Code seçimi için kullanılır
    /// </summary>
    /// <param name="customerCode">Müşteri kodu</param>
    /// <returns>Referans listesi</returns>
    [HttpGet("customer-references/{customerCode}")]
    public async Task<ActionResult<Result<CustomerReferencesResponse>>> GetCustomerReferences(string customerCode)
    {
        var result = await _mediator.Send(new GetCustomerReferencesQuery { CustomerCode = customerCode });
        return Ok(Result<CustomerReferencesResponse>.Succeed(result));
    }

    [HttpPost("item-order-check")]
    public async Task<ActionResult<Result<ItemOrderCheckResponse>>> GetItemOrderCheck([FromBody] ItemOrderCheckRequest request)
    {
        var result = await _mediator.Send(new GetItemOrderCheckQuery { Request = request });
        return Ok(Result<ItemOrderCheckResponse>.Succeed(result));
    }

    /// <summary>
    /// Sipariş bilgilerini günceller
    /// </summary>
    /// <param name="request">Güncellenecek sipariş verileri</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPost("orders/update")]
    public async Task<ActionResult<Result<bool>>> UpdateOrder([FromBody] UpdateOrderRequest request)
    {
        var result = await _mediator.Send(new UpdateOrderCommand { Request = request });
        if (!result)
        {
            return Ok(Result<bool>.Failure("Order could not be updated. The order may not exist."));
        }

        return Ok(Result<bool>.Succeed(true));
    }

    /// <summary>
    /// Sipariş kalemini günceller veya yeni kalem ekler
    /// </summary>
    /// <param name="request">Güncellenecek kalem verileri</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPost("orders/items/update")]
    public async Task<ActionResult<Result<bool>>> UpdateOrderItem([FromBody] UpdateOrderItemRequest request)
    {
        var result = await _mediator.Send(new UpdateOrderItemCommand { Request = request });
        return Ok(Result<bool>.Succeed(result));
    }

    /// <summary>
    /// Siparişten kalem siler
    /// </summary>
    /// <param name="request">Silinecek kalem verileri</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPost("orders/items/remove")]
    public async Task<ActionResult<Result<bool>>> RemoveOrderItem([FromBody] RemoveOrderItemRequest request)
    {
        var result = await _mediator.Send(new RemoveOrderItemCommand { Request = request });
        return Ok(Result<bool>.Succeed(result));
    }

    /// <summary>
    /// Müşteri özet raporunu getirir
    /// </summary>
    /// <param name="request">Filtreleme kriterleri</param>
    /// <returns>Özet müşteri rapor verileri</returns>
    [HttpPost("summary-customer")]
    public async Task<ActionResult<Result<SummaryCustomerReportResponse>>> GetSummaryCustomerReport([FromBody] SummaryCustomerReportRequest request)
    {
        var result = await _mediator.Send(new GetSummaryCustomerReportQuery { Request = request });
        return Ok(Result<SummaryCustomerReportResponse>.Succeed(result));
    }

    /// <summary>
    /// Sipariş için Proforma Invoice Excel dosyasını üretir ve döner
    /// </summary>
    /// <param name="orderId">Sipariş Id</param>
    /// <returns>Excel dosyası</returns>
    [HttpGet("orders/{orderId}/proforma-invoice")]
    public async Task<IActionResult> GetProformaInvoice(long orderId)
    {
        var fileContent = await _mediator.Send(new GetProformaInvoiceQuery(orderId));
        if (fileContent == null || fileContent.Length == 0) return NotFound();

        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Proforma_Invoice_{orderId}.xlsx");
    }

    /// <summary>
    /// Sipariş için Invoice Excel dosyasını üretir ve döner
    /// </summary>
    /// <param name="orderId">Sipariş Id</param>
    /// <returns>Excel dosyası</returns>
    [HttpGet("orders/{orderId}/invoice")]
    public async Task<IActionResult> GetInvoice(long orderId)
    {
        var fileContent = await _mediator.Send(new GetInvoiceQuery(orderId));
        if (fileContent == null || fileContent.Length == 0) return NotFound();

        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Invoice_{orderId}.xlsx");
    }
    /// <summary>
    /// Packing List verilerini getirir
    /// </summary>
    /// <param name="orderId">Sipariş Id</param>
    /// <returns>Packing List detayları</returns>
    [HttpGet("orders/{orderId}/packing-list")]
    public async Task<ActionResult<Result<PackingListDto>>> GetPackingList(long orderId)
    {
        var result = await _mediator.Send(new GetPackingListQuery(orderId));
        return Ok(Result<PackingListDto>.Succeed(result));
    }

    /// <summary>
    /// Packing List verilerini kaydeder
    /// </summary>
    /// <param name="request">Packing List dataları</param>
    /// <returns>Başarılı mı?</returns>
    [HttpPost("orders/packing-list")]
    public async Task<ActionResult<Result<bool>>> SavePackingList([FromBody] SavePackingListCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(Result<bool>.Succeed(result));
    }
}
