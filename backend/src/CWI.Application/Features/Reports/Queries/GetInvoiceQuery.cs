using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Lookups;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;

namespace CWI.Application.Features.Reports.Queries;

public record GetInvoiceQuery(long OrderId) : IRequest<byte[]>;

public class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, byte[]>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvoiceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<byte[]> Handle(GetInvoiceQuery query, CancellationToken cancellationToken)
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

        // Ayarları yükle (Proforma ile aynı ayarları kullanıyoruz şimdilik - Adres, Banka vs.)
        var settings = await GetInvoiceSettings(cancellationToken);

        using var workbook = new XLWorkbook();
        workbook.Properties.Author = "ARIKAN YAZILIM";
        workbook.Properties.Title = "CWI B2B Invoice " + DateTime.Now.Year;

        // Logo yolu
        var logoPath = @"c:\New Project\CWI\frontend\src\assets\images\Logo.png";
        
        // 1. Kısım: "SL" ile başlayan ürünler
        var slItems = order.Items.Where(x => x.Product.Sku.StartsWith("SL")).OrderBy(x => x.Product.Sku).ToList();
        if (slItems.Any())
        {
            var sheet = workbook.Worksheets.Add("Invoice 1");
            // Prefixleri "INV" olarak güncelledik
            AddOrderSheet(sheet, order, slItems, logoPath, "SLCL", "SLINV", settings);
        }

        // 2. Kısım: Diğer ürünler
        var otherItems = order.Items.Where(x => !x.Product.Sku.StartsWith("SL")).OrderBy(x => x.Product.Sku).ToList();
        if (otherItems.Any())
        {
            var sheetName = slItems.Any() ? "Invoice 2" : "Invoice";
            var sheet = workbook.Worksheets.Add(sheetName);
            // Prefixleri "INV" olarak güncelledik
            AddOrderSheet(sheet, order, otherItems, logoPath, "BHPCINV24-", "BHPCINV24-", settings);
        }

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task<Dictionary<string, string>> GetInvoiceSettings(CancellationToken cancellationToken)
    {
        // 1. Varsayılan Değerler
        var settings = new Dictionary<string, string>
        {
            // Titles & Labels
            { "Invoice_Title", "INVOICE" },
            { "Label_To", "To :" },
            { "Label_Attn", "Attn :" },
            { "Label_Order_Date", "Order Date :" },
            { "Label_Collection_No", "Collection No :" },
            { "Label_Invoice_No", "Invoice No :" },
            
            // Table Headers
            { "Header_Item_No", "ITEM NO" },
            { "Header_Item_Code", "ITEM CODE" },
            { "Header_Description", "DESCRIPTION" },
            { "Header_Qty", "QTY" },
            { "Header_Unit_Price", "UNIT PRICE" },
            { "Header_Currency", "CURRENCY" },
            { "Header_Sub_Total", "SUB TOTAL" },

            // Totals
            { "Label_Total_Qty", "Total Qty :" },
            { "Label_Total_Amount", "Total Amount :" },

            // Sections
            { "Label_Bank_Instruction", "Bank Transfer Instruction" },

            // Address & Terms
            { "Company_Address_Line1", "UNIT C.16/F OF BLOCK 1, WAH FUNG INDUSTRIAL CENTRE" },
            { "Company_Address_Line2", "33-39 KWAI FUNG CRESCENT, KWAI CHUNG, HONG KONG" },
            { "Company_Phone", "(852) 3525 0285" },
            
            // Term Labels
            { "Term_Transportation_Label", "Transportation Fee :" },
            { "Term_Payment_Label", "Payment :" },
            { "Term_Delivery_Label", "Delivery :" },
            { "Term_Shipment_Label", "Shipment :" },

            // Term Values
            { "Term_Transportation", "Paid By Buyer" },
            { "Term_Payment", "In Advance Before Shipment" },
            { "Term_Delivery", "EXW HK" },
            { "Term_Shipment", "AIR SHIPMENT" },
            
            // Bank Labels
            { "Label_Bank", "Bank :" },
            { "Label_Bank_Acc_No", "Bank Acc No :" },
            { "Label_Swift_Code", "SWIFT Code :" },
            { "Label_Bank_Address", "Bank Address :" },

            // Bank Values
            { "Bank_Name", "Standard Chartered Bank (Hong Kong) Limited Bank" },
            { "Bank_Account", "36807798556" },
            { "Bank_Swift", "SCBLHKHHXXX" },
            { "Bank_Address", "3/F, Standard Chartered Bank Building, 4-4A Des Voeux Road Central, Hong Kong" },
            
            { "Footer_Note", "Your Order will not be shipped until we receive payment" }
        };

        // 2. İngilizce dilini bul
        var engLang = await _unitOfWork.Repository<Language>().AsQueryable()
            .FirstOrDefaultAsync(l => l.Code == "en" && l.IsActive, cancellationToken);
        
        if (engLang == null) return settings;

        // 3. Veritabanından ayarları çek (Module = 'Invoice' varsa onu, yoksa 'Proforma')
        var dbSettings = await _unitOfWork.Repository<LocalizedString>().AsQueryable()
            .Where(x => x.LanguageId == engLang.Id && (x.Module == "Proforma" || x.Module == "Invoice"))
            .ToListAsync(cancellationToken);

        // 4. Varsa ez
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
                             string colPrefix, string piPrefix, Dictionary<string, string> settings)
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

        // Şirket Adres Bilgileri
        sheet.Range(2, 1, 2, 7).Merge().Value = settings["Company_Address_Line1"];
        sheet.Range(3, 1, 3, 7).Merge().Value = settings["Company_Address_Line2"];
        sheet.Range(4, 1, 4, 7).Merge().Value = settings["Company_Phone"];

        sheet.Range(2, 1, 4, 7).Style.Font.FontSize = 10;
        sheet.Row(4).Style.Font.FontSize = 8;
        sheet.Range(2, 1, 4, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // Title: INVOICE (Dynamic)
        var titleRange = sheet.Range(6, 1, 6, 7);
        titleRange.Merge();
        titleRange.Value = settings["Invoice_Title"];
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 18;

        // Customer Info
        sheet.Cell(7, 1).Value = settings["Label_To"];
        sheet.Cell(7, 1).Style.Font.Bold = true;
        sheet.Range(7, 2, 7, 4).Merge().Value = order.Customer?.Name ?? "-";

        sheet.Cell(8, 1).Value = settings["Label_Attn"];
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
        sheet.Cell(7, 6).Value = settings["Label_Order_Date"];
        sheet.Cell(7, 6).Style.Font.Bold = true;
        sheet.Cell(7, 7).Value = order.OrderedAt.ToString("dd/MM/yyyy");

        sheet.Cell(8, 6).Value = settings["Label_Collection_No"];
        sheet.Cell(8, 6).Style.Font.Bold = true;
        sheet.Cell(8, 7).Value = colPrefix + (15000 + order.Id);

        sheet.Cell(9, 6).Value = settings["Label_Invoice_No"]; 
        sheet.Cell(9, 6).Style.Font.Bold = true;
        sheet.Cell(9, 7).Value = piPrefix + (15000 + order.Id);

        // Table Header
        var headers = new[] { 
            settings["Header_Item_No"], 
            settings["Header_Item_Code"], 
            settings["Header_Description"], 
            settings["Header_Qty"], 
            settings["Header_Unit_Price"], 
            settings["Header_Currency"], 
            settings["Header_Sub_Total"] 
        };

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
        qtyRange.Merge().Value = settings["Label_Total_Qty"];
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
        amountRange.Merge().Value = settings["Label_Total_Amount"];
        amountRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        amountRange.Style.Font.Bold = true;
        amountRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        sheet.Cell(rowIndex, 7).Value = items.Sum(x => x.LineTotal);
        sheet.Cell(rowIndex, 7).Style.NumberFormat.Format = "#,##0.00";
        sheet.Cell(rowIndex, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Cell(rowIndex, 7).Style.Font.Bold = true;
        sheet.Cell(rowIndex, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Terms & Conditions
        rowIndex += 2;
        var terms = new[]
        {
            new { Label = settings["Term_Transportation_Label"], Value = settings["Term_Transportation"] },
            new { Label = settings["Term_Payment_Label"], Value = settings["Term_Payment"] },
            new { Label = settings["Term_Delivery_Label"], Value = settings["Term_Delivery"] },
            new { Label = settings["Term_Shipment_Label"], Value = settings["Term_Shipment"] }
        };

        foreach (var term in terms)
        {
            sheet.Range(rowIndex, 1, rowIndex, 2).Merge().Value = term.Label;
            sheet.Range(rowIndex, 3, rowIndex, 6).Merge().Value = term.Value;
            rowIndex++;
        }

        // Bank Details
        rowIndex += 2;
        sheet.Range(rowIndex, 1, rowIndex, 6).Merge().Value = settings["Label_Bank_Instruction"];
        sheet.Cell(rowIndex, 1).Style.Font.Bold = true;
        sheet.Cell(rowIndex, 1).Style.Font.Underline = XLFontUnderlineValues.Single;
        rowIndex += 2;

        var bankInfo = new[]
        {
            new { Label = settings["Label_Bank"], Value = settings.ContainsKey("Bank_Name") ? settings["Bank_Name"] : "" },
            new { Label = settings["Label_Bank_Acc_No"], Value = settings.ContainsKey("Bank_Account") ? settings["Bank_Account"] : "" },
            new { Label = settings["Label_Swift_Code"], Value = settings.ContainsKey("Bank_Swift") ? settings["Bank_Swift"] : "" },
            new { Label = settings["Label_Bank_Address"], Value = settings.ContainsKey("Bank_Address") ? settings["Bank_Address"] : "" }
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
