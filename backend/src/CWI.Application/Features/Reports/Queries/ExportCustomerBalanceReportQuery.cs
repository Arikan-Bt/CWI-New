using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace CWI.Application.Features.Reports.Queries;

public class ExportCustomerBalanceReportQuery : IRequest<byte[]>
{
    public CustomerBalanceReportRequest Request { get; set; } = null!;

    public class ExportCustomerBalanceReportHandler : IRequestHandler<ExportCustomerBalanceReportQuery, byte[]>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ExportCustomerBalanceReportHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<byte[]> Handle(ExportCustomerBalanceReportQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();
            
            var queryable = transactionRepo.AsQueryable()
                .Include(t => t.Customer)
                .AsNoTracking();

            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                queryable = queryable.Where(t => t.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            if (request.StartDate.HasValue)
            {
                queryable = queryable.Where(t => t.TransactionDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                queryable = queryable.Where(t => t.TransactionDate <= request.EndDate.Value);
            }

            var transactions = await queryable
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new CustomerBalanceReportItemDto
                {
                    CurrAccCode = t.Customer != null ? t.Customer.Code : "-",
                    CurrAccDescription = t.Customer != null ? t.Customer.Name : "Unknown",
                    Date = t.TransactionDate,
                    ReferenceId = t.ReferenceNumber ?? string.Empty,
                    TotalAmount = t.DebitAmount,
                    TotalPayment = t.CreditAmount,
                    Balance = t.Balance, // Satır bakiyesi
                    OrderStatus = t.TransactionType.ToString(),
                    Status = (t.DebitAmount - t.CreditAmount) != 0 ? "Open" : "Closed"
                })
                .ToListAsync(cancellationToken);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Customer Balance");

            // Başlıklar
            int col = 1;
            worksheet.Cells[1, col++].Value = "Account Code";
            worksheet.Cells[1, col++].Value = "Account Name";
            worksheet.Cells[1, col++].Value = "Date";
            worksheet.Cells[1, col++].Value = "Reference ID";
            worksheet.Cells[1, col++].Value = "Type";
            worksheet.Cells[1, col++].Value = "Debit (Borç)";
            worksheet.Cells[1, col++].Value = "Credit (Alacak)";
            worksheet.Cells[1, col++].Value = "Balance";
            worksheet.Cells[1, col++].Value = "Status";

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
            foreach (var item in transactions)
            {
                col = 1;
                worksheet.Cells[row, col++].Value = item.CurrAccCode;
                worksheet.Cells[row, col++].Value = item.CurrAccDescription;
                worksheet.Cells[row, col++].Value = item.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, col++].Value = item.ReferenceId;
                worksheet.Cells[row, col++].Value = item.OrderStatus;
                worksheet.Cells[row, col++].Value = item.TotalAmount;
                worksheet.Cells[row, col++].Value = item.TotalPayment;
                worksheet.Cells[row, col++].Value = item.Balance;
                worksheet.Cells[row, col++].Value = item.Status;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }
    }
}
