using System.Drawing;
using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Identity;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace CWI.Application.Features.Reports.Queries;

public class ExportOrdersReportQuery : IRequest<byte[]>
{
    public OrdersReportRequest Request { get; set; } = null!;

    public class ExportOrdersReportQueryHandler : IRequestHandler<ExportOrdersReportQuery, byte[]>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ExportOrdersReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<byte[]> Handle(ExportOrdersReportQuery query, CancellationToken cancellationToken)
        {
            var filters = query.Request;
            var orderRepo = _unitOfWork.Repository<Order, long>();
            
            var queryable = orderRepo.AsQueryable()
                .Include(o => o.Customer)
                .Include(o => o.ShippingInfo)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Brand)
                .AsNoTracking();

            if (!_currentUserService.IsAdministrator)
            {
                if (_currentUserService.ProjectType.HasValue)
                {
                    var projectCode = _currentUserService.ProjectType.Value.ToString();
                    queryable = queryable.Where(o => o.CreatedByGroupCode == projectCode);
                }

                if (_currentUserService.LinkedCustomerId.HasValue)
                {
                    queryable = queryable.Where(o => o.CustomerId == _currentUserService.LinkedCustomerId.Value);
                }
            }

            if (!string.IsNullOrEmpty(filters.CurrentAccountCode))
            {
                queryable = queryable.Where(o => o.Customer != null && o.Customer.Code == filters.CurrentAccountCode);
            }

            if (!string.IsNullOrEmpty(filters.OrderStatus))
            {
                if (Enum.TryParse<CWI.Domain.Enums.OrderStatus>(filters.OrderStatus, out var status))
                {
                    queryable = queryable.Where(o => o.Status == status);
                }
            }

            if (filters.StartDate.HasValue)
            {
                queryable = queryable.Where(o => o.OrderedAt >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                queryable = queryable.Where(o => o.OrderedAt <= filters.EndDate.Value);
            }

            var orders = await queryable.ToListAsync(cancellationToken);

            var items = orders.SelectMany(o => o.Items.Select(i => new { Order = o, Item = i }))
                .GroupBy(x => new { x.Order.Id, BrandName = x.Item.Product?.Brand?.Name ?? "Unknown" })
                .Select(g => 
                {
                    var order = g.First().Order;
                    var brandTotal = g.Sum(x => x.Item.LineTotal);
                    var brandDiscount = g.Sum(x => x.Item.DiscountAmount);
                    var brandQty = g.Sum(x => x.Item.Quantity);
                    
                    return new OrderReportDto
                    {
                        OrderId = g.Key.Id,
                        CurrentAccountCode = order.Customer?.Code ?? "-",
                        CurrentAccountDescription = order.Customer?.Name ?? "Unknown Customer",
                        OrderDetails = order.OrderNumber,
                        Status = order.Status.ToString(), // Basit string çevrimi yeterli
                        Brand = g.Key.BrandName,
                        OrderDate = order.OrderedAt,
                        RequestedShipmentDate = order.ShippedAt,
                        TotalQty = brandQty,
                        Discount = brandDiscount,
                        Total = brandTotal,
                        Address = order.Customer != null 
                            ? $"{order.Customer.AddressLine1} {order.Customer.AddressLine2} {order.Customer.Town} {order.Customer.City} {order.Customer.Country}".Trim()
                            : "",
                        PaymentType = order.ShippingInfo?.PaymentMethod,
                        ShipmentMethod = order.ShippingInfo?.ShipmentTerms,
                        OrderDescription = order.Notes,
                        Season = order.Season,
                        SubTotal = brandTotal + brandDiscount,
                        GrandTotal = brandTotal
                    };
                })
                .OrderBy(x => x.OrderDate)
                .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Orders");

            // Başlıklar
            int col = 1;
            worksheet.Cells[1, col++].Value = "Order ID";
            worksheet.Cells[1, col++].Value = "Customer Code";
            worksheet.Cells[1, col++].Value = "Customer Name";
            worksheet.Cells[1, col++].Value = "Order Number";
            worksheet.Cells[1, col++].Value = "Status";
            worksheet.Cells[1, col++].Value = "Brand";
            worksheet.Cells[1, col++].Value = "Order Date";
            worksheet.Cells[1, col++].Value = "Req. Shipment Date";
            worksheet.Cells[1, col++].Value = "Total Qty";
            worksheet.Cells[1, col++].Value = "Discount";
            worksheet.Cells[1, col++].Value = "Total Amount";
            worksheet.Cells[1, col++].Value = "Payment Type";
            worksheet.Cells[1, col++].Value = "Shipment Method";
            worksheet.Cells[1, col++].Value = "Description";
            worksheet.Cells[1, col++].Value = "Season";

            // Stil
            using (var range = worksheet.Cells[1, 1, 1, col - 1])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            // Veriler
            int row = 2;
            foreach (var item in items)
            {
                col = 1;
                worksheet.Cells[row, col++].Value = item.OrderId;
                worksheet.Cells[row, col++].Value = item.CurrentAccountCode;
                worksheet.Cells[row, col++].Value = item.CurrentAccountDescription;
                worksheet.Cells[row, col++].Value = item.OrderDetails;
                worksheet.Cells[row, col++].Value = item.Status;
                worksheet.Cells[row, col++].Value = item.Brand;
                worksheet.Cells[row, col++].Value = item.OrderDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, col++].Value = item.RequestedShipmentDate?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, col++].Value = item.TotalQty;
                worksheet.Cells[row, col++].Value = item.Discount;
                worksheet.Cells[row, col++].Value = item.Total;
                worksheet.Cells[row, col++].Value = item.PaymentType;
                worksheet.Cells[row, col++].Value = item.ShipmentMethod;
                worksheet.Cells[row, col++].Value = item.OrderDescription;
                worksheet.Cells[row, col++].Value = item.Season;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            return await Task.FromResult(package.GetAsByteArray());
        }
    }
}
