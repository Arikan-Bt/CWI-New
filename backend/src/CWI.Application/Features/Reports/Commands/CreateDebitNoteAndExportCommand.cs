using ClosedXML.Excel;
using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Commands;

/// <summary>
/// Debit note kaydı oluşturur ve excel çıktısı üretir.
/// </summary>
public class CreateDebitNoteAndExportCommand : IRequest<byte[]>
{
    public CreateDebitNoteExportRequest Request { get; set; } = null!;

    public class CreateDebitNoteAndExportCommandHandler : IRequestHandler<CreateDebitNoteAndExportCommand, byte[]>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateDebitNoteAndExportCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<byte[]> Handle(CreateDebitNoteAndExportCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var customer = await _unitOfWork.Repository<Customer, int>()
                .AsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == request.CustomerCode, cancellationToken);

            if (customer == null)
            {
                return Array.Empty<byte>();
            }

            var orderQuery = _unitOfWork.Repository<Order, long>()
                .AsQueryable()
                .Include(o => o.Customer)
                .Include(o => o.Currency)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking()
                .Where(o => o.Id == request.OrderId && o.CustomerId == customer.Id);

            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            var order = await orderQuery.FirstOrDefaultAsync(cancellationToken);
            if (order == null || order.Status != OrderStatus.Canceled)
            {
                return Array.Empty<byte>();
            }

            var paidAmount = await CalculatePaidAmount(customer.Id, order.Id, request.InvoiceNo, order.OrderNumber, cancellationToken);
            if (paidAmount <= 0 || request.Amount <= 0)
            {
                return Array.Empty<byte>();
            }

            var transaction = new CustomerTransaction
            {
                CustomerId = customer.Id,
                TransactionType = TransactionType.DebitNote,
                TransactionDate = request.DebitNoteDate,
                ReferenceNumber = request.InvoiceNo,
                Description = $"Debit Note - {request.Notes}",
                DocumentType = "Debit Note",
                ApplicationReference = order.Id.ToString(),
                DebitAmount = request.Amount,
                CreditAmount = 0,
                Balance = request.Amount,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<CustomerTransaction, long>().AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return CreateDebitNoteExcel(order, request, paidAmount);
        }

        private async Task<decimal> CalculatePaidAmount(
            int customerId,
            long orderId,
            string invoiceNo,
            string orderNumber,
            CancellationToken cancellationToken)
        {
            var transactions = await _unitOfWork.Repository<CustomerTransaction, long>()
                .AsQueryable()
                .AsNoTracking()
                .Where(t => t.CustomerId == customerId && t.CreditAmount > 0)
                .ToListAsync(cancellationToken);

            var sum = transactions
                .Where(t =>
                    (long.TryParse(t.ApplicationReference, out var appOrderId) && appOrderId == orderId) ||
                    string.Equals(t.ReferenceNumber, invoiceNo, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.ReferenceNumber, orderNumber, StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.CreditAmount);

            return sum;
        }

        private static byte[] CreateDebitNoteExcel(Order order, CreateDebitNoteExportRequest request, decimal paidAmount)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Debit Note");

            sheet.Column(1).Width = 18;
            sheet.Column(2).Width = 28;
            sheet.Column(3).Width = 18;
            sheet.Column(4).Width = 18;
            sheet.Column(5).Width = 14;
            sheet.Column(6).Width = 14;
            sheet.Column(7).Width = 18;

            sheet.Range(1, 1, 1, 7).Merge().Value = "DEBIT NOTE";
            sheet.Range(1, 1, 1, 7).Style.Font.Bold = true;
            sheet.Range(1, 1, 1, 7).Style.Font.FontSize = 18;
            sheet.Range(1, 1, 1, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            sheet.Cell(3, 1).Value = "Customer :";
            sheet.Cell(3, 2).Value = order.Customer?.Name ?? "-";
            sheet.Cell(3, 5).Value = "Invoice No :";
            sheet.Cell(3, 6).Value = request.InvoiceNo;

            sheet.Cell(4, 1).Value = "Debit Note Date :";
            sheet.Cell(4, 2).Value = request.DebitNoteDate.ToString("dd/MM/yyyy");
            sheet.Cell(4, 5).Value = "Canceled Date :";
            sheet.Cell(4, 6).Value = (order.UpdatedAt ?? order.CreatedAt).ToString("dd/MM/yyyy");

            sheet.Cell(5, 1).Value = "Paid Amount :";
            sheet.Cell(5, 2).Value = paidAmount;
            sheet.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(5, 5).Value = "Debit Note Amount :";
            sheet.Cell(5, 6).Value = request.Amount;
            sheet.Cell(5, 6).Style.NumberFormat.Format = "#,##0.00";

            sheet.Cell(6, 1).Value = "Notes :";
            sheet.Range(6, 2, 6, 7).Merge().Value = request.Notes ?? string.Empty;
            sheet.Range(6, 2, 6, 7).Style.Alignment.WrapText = true;

            var headers = new[] { "ITEM NO", "ITEM CODE", "DESCRIPTION", "QTY", "UNIT PRICE", "CURRENCY", "SUB TOTAL" };
            for (var i = 0; i < headers.Length; i++)
            {
                var headerCell = sheet.Cell(8, i + 1);
                headerCell.Value = headers[i];
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var row = 9;
            var lineNo = 1;
            foreach (var item in order.Items.OrderBy(i => i.Product?.Sku))
            {
                sheet.Cell(row, 1).Value = lineNo++;
                sheet.Cell(row, 2).Value = item.Product?.Sku ?? string.Empty;
                sheet.Cell(row, 3).Value = item.Product?.Name ?? string.Empty;
                sheet.Cell(row, 4).Value = item.Quantity;
                sheet.Cell(row, 5).Value = item.UnitPrice;
                sheet.Cell(row, 6).Value = order.Currency?.Code ?? "USD";
                sheet.Cell(row, 7).Value = item.LineTotal;

                sheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                sheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";

                for (var col = 1; col <= 7; col++)
                {
                    sheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(row, col).Style.Alignment.Horizontal = col == 3
                        ? XLAlignmentHorizontalValues.Left
                        : XLAlignmentHorizontalValues.Center;
                }

                row++;
            }

            var totalQty = order.Items.Sum(x => x.Quantity);
            var totalAmount = order.Items.Sum(x => x.LineTotal);

            sheet.Range(row, 1, row, 3).Merge().Value = "Total Qty :";
            sheet.Range(row, 1, row, 3).Style.Font.Bold = true;
            sheet.Range(row, 1, row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            sheet.Cell(row, 4).Value = totalQty;
            sheet.Cell(row, 4).Style.Font.Bold = true;

            for (var col = 5; col <= 7; col++)
            {
                sheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            row++;
            sheet.Range(row, 1, row, 6).Merge().Value = "Total Amount :";
            sheet.Range(row, 1, row, 6).Style.Font.Bold = true;
            sheet.Range(row, 1, row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            sheet.Cell(row, 7).Value = totalAmount;
            sheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(row, 7).Style.Font.Bold = true;

            row++;
            sheet.Range(row, 1, row, 6).Merge().Value = "Debit Note Amount :";
            sheet.Range(row, 1, row, 6).Style.Font.Bold = true;
            sheet.Range(row, 1, row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            sheet.Cell(row, 7).Value = request.Amount;
            sheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(row, 7).Style.Font.Bold = true;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
