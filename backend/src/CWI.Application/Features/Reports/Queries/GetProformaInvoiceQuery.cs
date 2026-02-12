using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Lookups;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;

namespace CWI.Application.Features.Reports.Queries;

public record GetProformaInvoiceQuery(long OrderId) : IRequest<byte[]>;

public class GetProformaInvoiceQueryHandler : IRequestHandler<GetProformaInvoiceQuery, byte[]>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProformaInvoiceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<byte[]> Handle(GetProformaInvoiceQuery query, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Repository<Order, long>()
            .AsQueryable()
            .Include(o => o.Customer)
            .Include(o => o.Currency)
            .Include(o => o.ShippingInfo)
            .Include(o => o.DeliveryRequest)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Brand)
            .FirstOrDefaultAsync(o => o.Id == query.OrderId, cancellationToken);

        if (order == null) return Array.Empty<byte>();

        // Ayarları yükle
        var settings = await GetProformaSettings(cancellationToken);

        using var workbook = new XLWorkbook();
        workbook.Properties.Author = "ARIKAN YAZILIM";
        workbook.Properties.Title = "CWI B2B " + DateTime.Now.Year;

        // Logo yolu
        var logoPath = @"c:\New Project\CWI\frontend\src\assets\images\Logo.png";
        
        // 1. Kısım: "SL" ile başlayan ürünler
        var slItems = order.Items.Where(x => x.Product.Sku.StartsWith("SL")).OrderBy(x => x.Product.Sku).ToList();
        if (slItems.Any())
        {
            var sheet = workbook.Worksheets.Add("Proposal 1");
            var colPrefix = settings.GetValueOrDefault("Proforma_SL_Collection_Prefix", "SLCL");
            var piPrefix = settings.GetValueOrDefault("Proforma_SL_PI_Prefix", "SLPI");
            var startNumStr = settings.GetValueOrDefault("Order_Sequence_Start", "3000");
            if (!long.TryParse(startNumStr, out long startNum)) startNum = 3000;

            // SL ürünleri için mevcut mantığı (ID bazlı) koruyalım veya siz isterseniz onu da değiştirebiliriz.
            // Kullanıcı BHPCIN26-3000 için talepte bulundu.
            var finalColNo = colPrefix + (startNum + order.Id);
            var finalPiNo = piPrefix + (startNum + order.Id);

            AddOrderSheet(sheet, order, slItems, logoPath, finalColNo, finalPiNo, settings);
        }

        // 2. Kısım: Diğer ürünler
        var otherItems = order.Items.Where(x => !x.Product.Sku.StartsWith("SL")).OrderBy(x => x.Product.Sku).ToList();
        if (otherItems.Any())
        {
            var sheetName = slItems.Any() ? "Proposal 2" : "Proposal";
            var sheet = workbook.Worksheets.Add(sheetName);
            
            // --- Tekil Numara Atama Mantığı ---
            // Bu sipariş için daha önce bir numara atanmış mı kontrol edelim
            var assignedNoKey = $"Assigned_No_{order.Id}";
            var assignedSetting = await _unitOfWork.Repository<LocalizedString>().AsQueryableTracking()
                .FirstOrDefaultAsync(x => x.Key == assignedNoKey && x.Module == "AssignedOrders", cancellationToken);

            string finalNumber;

            if (assignedSetting != null)
            {
                // Zaten atanmış, aynı numarayı kullanalım
                finalNumber = assignedSetting.Value;
            }
            else
            {
                // Henüz atanmamış, yeni bir numara alalım ve sayacı artıralım
                var formatSetting = await _unitOfWork.Repository<LocalizedString>().AsQueryableTracking()
                    .FirstOrDefaultAsync(x => x.Key == "Order_BHPC_Format" && x.Module == "OrderSettings", cancellationToken);

                finalNumber = formatSetting?.Value ?? "BHPCIN26-3000";

                // Atanan numarayı bu sipariş için kaydedelim
                await _unitOfWork.Repository<LocalizedString>().AddAsync(new LocalizedString
                {
                    Key = assignedNoKey,
                    Value = finalNumber,
                    Module = "AssignedOrders",
                    LanguageId = 1 // Varsayılan dil
                }, cancellationToken);

                // Küresel sayacı bir sonraki sipariş için artıralım
                if (formatSetting != null && finalNumber.Contains("-"))
                {
                    var lastDashIdx = finalNumber.LastIndexOf('-');
                    var prefix = finalNumber.Substring(0, lastDashIdx + 1);
                    var numPart = finalNumber.Substring(lastDashIdx + 1);
                    if (long.TryParse(numPart, out long currentNum))
                    {
                        formatSetting.Value = prefix + (currentNum + 1);
                    }
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            // ----------------------------------

            AddOrderSheet(sheet, order, otherItems, logoPath, finalNumber, finalNumber, settings);
        }

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task<Dictionary<string, string>> GetProformaSettings(CancellationToken cancellationToken)
    {
        // 1. Varsayılan Değerler
        var settings = new Dictionary<string, string>
        {
            { "Company_Address_Line1", "UNIT C.16/F OF BLOCK 1, WAH FUNG INDUSTRIAL CENTRE" },
            { "Company_Address_Line2", "33-39 KWAI FUNG CRESCENT, KWAI CHUNG, HONG KONG" },
            { "Company_Phone", "(852) 3525 0285" },
            { "Term_Transportation", "Paid By Buyer" },
            { "Term_Payment", "In Advance Before Shipment" },
            { "Term_Delivery", "EXW HK" },
            { "Term_Shipment", "AIR SHIPMENT" },
            { "Bank_Name", "Standard Chartered Bank (Hong Kong) Limited Bank" },
            { "Bank_Account", "36807798556" },
            { "Bank_Swift", "SCBLHKHHXXX" },
            { "Bank_Address", "3/F, Standard Chartered Bank Building, 4-4A Des Voeux Road Central, Hong Kong" },
            { "Footer_Note", "Your Order will not be shipped until we receive payment" }
        };

        // 2. Veritabanından ayarları çek (Module = 'Proforma' veya 'OrderSettings')
        var dbSettings = await _unitOfWork.Repository<LocalizedString>().AsQueryable()
            .Where(x => x.Module == "Proforma" || x.Module == "OrderSettings")
            .ToListAsync(cancellationToken);

        // 3. Varsa ez
        foreach (var item in dbSettings)
        {
            if (settings.ContainsKey(item.Key))
                settings[item.Key] = item.Value;
            else
                settings.Add(item.Key, item.Value);
        }
        
        return settings;
    }

    private void AddOrderSheet(IXLWorksheet sheet, Order order, List<OrderItem> items, string logoPath, 
                             string collectionNo, string piNo, Dictionary<string, string> settings)
    {
        // Sayfa Ayarları
        sheet.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        sheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        sheet.PageSetup.Margins.Left = 0.5;
        sheet.PageSetup.Margins.Right = 0.5;

        // Row 1: Header & Logos
        sheet.Row(1).Height = 60;
        
        // Logo Ekleme
        if (File.Exists(logoPath))
        {
            try 
            {
                var picture = sheet.AddPicture(logoPath)
                    .MoveTo(sheet.Cell(1, 1))
                    .Scale(0.35); 
            }
            catch (Exception) { }
        }

        // Şirket Adres Bilgileri (Dinamik)
        sheet.Range(2, 1, 2, 7).Merge().Value = settings["Company_Address_Line1"];
        sheet.Range(3, 1, 3, 7).Merge().Value = settings["Company_Address_Line2"];
        sheet.Range(4, 1, 4, 7).Merge().Value = settings["Company_Phone"];

        sheet.Range(2, 1, 4, 7).Style.Font.FontSize = 10;
        sheet.Row(4).Style.Font.FontSize = 8;
        sheet.Range(2, 1, 4, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // Title: PROFORMA INVOICE
        var titleRange = sheet.Range(6, 1, 6, 7);
        titleRange.Merge();
        titleRange.Value = "PROFORMA INVOICE";
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 18;

        // Customer Info
        sheet.Cell(7, 1).Value = "To :";
        sheet.Cell(7, 1).Style.Font.Bold = true;
        sheet.Range(7, 2, 7, 4).Merge().Value = order.Customer?.Name ?? "-";

        sheet.Cell(8, 1).Value = "Attn :";
        sheet.Cell(8, 1).Style.Font.Bold = true;
        
        var address = order.ShippingInfo?.ShippingAddress ?? 
                     (order.Customer != null 
                        ? string.Join(" ", new[] { order.Customer.AddressLine1, order.Customer.City, order.Customer.Country }.Where(s => !string.IsNullOrWhiteSpace(s)))
                        : "");
        
        var addressRange = sheet.Range(8, 2, 9, 4);
        addressRange.Merge().Value = address;
        addressRange.Style.Alignment.WrapText = true;
        addressRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

        // Order Metadata
        sheet.Cell(7, 6).Value = "Order Date :";
        sheet.Cell(7, 6).Style.Font.Bold = true;
        sheet.Cell(7, 7).Value = order.OrderedAt.ToString("dd/MM/yyyy");

        sheet.Cell(8, 7).Value = collectionNo;

        sheet.Cell(9, 6).Value = "PI No :";
        sheet.Cell(9, 6).Style.Font.Bold = true;
        sheet.Cell(9, 7).Value = piNo;

        // Table Header
        var headers = new[] { "ITEM NO", "ITEM CODE", "DESCRIPTION", "QTY", "UNIT PRICE", "CURRENCY", "SUB TOTAL" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(12, i + 1);
            cell.Value = headers[i];
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Table Data
        int rowIndex = 13;
        int itemNo = 1;
        foreach (var item in items)
        {
            sheet.Cell(rowIndex, 1).Value = itemNo++;
            sheet.Cell(rowIndex, 2).Value = item.Product.Sku;
            sheet.Cell(rowIndex, 3).Value = item.Product.Name;
            sheet.Cell(rowIndex, 4).Value = item.Quantity;
            sheet.Cell(rowIndex, 5).Value = item.UnitPrice;
            sheet.Cell(rowIndex, 5).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(rowIndex, 6).Value = order.Currency?.Code ?? "USD";
            sheet.Cell(rowIndex, 7).Value = item.LineTotal;
            sheet.Cell(rowIndex, 7).Style.NumberFormat.Format = "#,##0.00";

            for (int i = 1; i <= 7; i++)
            {
                var cell = sheet.Cell(rowIndex, i);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            sheet.Cell(rowIndex, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            rowIndex++;
        }

        // Totals
        var qtyRange = sheet.Range(rowIndex, 1, rowIndex, 3);
        qtyRange.Merge().Value = "Total Qty :";
        qtyRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        qtyRange.Style.Font.Bold = true;
        qtyRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        sheet.Cell(rowIndex, 4).Value = items.Sum(x => x.Quantity);
        sheet.Cell(rowIndex, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Cell(rowIndex, 4).Style.Font.Bold = true;
        sheet.Cell(rowIndex, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        for (int i = 5; i <= 7; i++) sheet.Cell(rowIndex, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        rowIndex++;
        var amountRange = sheet.Range(rowIndex, 1, rowIndex, 6);
        amountRange.Merge().Value = "Total Amount :";
        amountRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        amountRange.Style.Font.Bold = true;
        amountRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        sheet.Cell(rowIndex, 7).Value = items.Sum(x => x.LineTotal);
        sheet.Cell(rowIndex, 7).Style.NumberFormat.Format = "#,##0.00";
        sheet.Cell(rowIndex, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Cell(rowIndex, 7).Style.Font.Bold = true;
        sheet.Cell(rowIndex, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Terms & Conditions (Dinamik)
        rowIndex += 2;
        var terms = new[]
        {
            new { Label = "Transportation Fee :", Value = settings["Term_Transportation"] },
            new { Label = "Payment :", Value = settings["Term_Payment"] },
            new { Label = "Delivery :", Value = settings["Term_Delivery"] },
            new { Label = "Shipment :", Value = settings["Term_Shipment"] }
        };

        foreach (var term in terms)
        {
            sheet.Range(rowIndex, 1, rowIndex, 2).Merge().Value = term.Label;
            sheet.Range(rowIndex, 3, rowIndex, 6).Merge().Value = term.Value;
            rowIndex++;
        }

        // Bank Details (Dinamik)
        rowIndex += 2;
        sheet.Range(rowIndex, 1, rowIndex, 6).Merge().Value = "Bank Transfer Instruction";
        sheet.Cell(rowIndex, 1).Style.Font.Bold = true;
        sheet.Cell(rowIndex, 1).Style.Font.Underline = XLFontUnderlineValues.Single;
        rowIndex += 2;

        var bankInfo = new[]
        {
            new { Label = "Bank :", Value = settings.ContainsKey("Bank_Name") ? settings["Bank_Name"] : "" },
            new { Label = "Bank Acc No :", Value = settings.ContainsKey("Bank_Account") ? settings["Bank_Account"] : "" },
            new { Label = "SWIFT Code :", Value = settings.ContainsKey("Bank_Swift") ? settings["Bank_Swift"] : "" },
            new { Label = "Bank Address :", Value = settings.ContainsKey("Bank_Address") ? settings["Bank_Address"] : "" }
        };

        foreach (var info in bankInfo)
        {
            sheet.Range(rowIndex, 1, rowIndex, 2).Merge().Value = info.Label;
            sheet.Cell(rowIndex, 1).Style.Font.Bold = true;
            sheet.Range(rowIndex, 3, rowIndex, 7).Merge().Value = info.Value;
            rowIndex++;
        }

        sheet.Range(rowIndex + 1, 1, rowIndex + 1, 7).Merge().Value = settings.ContainsKey("Footer_Note") ? settings["Footer_Note"] : "";
        sheet.Cell(rowIndex + 1, 1).Style.Font.Italic = true;

        sheet.Column(1).Width = 8;
        sheet.Column(2).Width = 15;
        sheet.Column(3).Width = 40;
        sheet.Column(4).Width = 8;
        sheet.Column(5).Width = 12;
        sheet.Column(6).Width = 10;
        sheet.Column(7).Width = 15;
    }
}
