using CWI.Domain.Entities.Lookups;
using Microsoft.EntityFrameworkCore;

namespace CWI.Infrastructure.Persistence;

public static class CWIDbContextSeed
{
    public static async Task SeedAsync(CWIDbContext context)
    {
        // 1. İngilizce dilini bul
        var engLang = await context.Languages.FirstOrDefaultAsync(l => l.Code == "en");
        if (engLang == null) return;

        // 2. Invoice ayarlarını belirle
        var invoiceSettings = new Dictionary<string, string>
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
            
            // Footer
            { "Footer_Note", "Your Order will not be shipped until we receive payment" }
        };

        // 3. Mevcut ayarları çek (Invoice)
        var existingSettings = await context.LocalizedStrings
            .Where(x => x.LanguageId == engLang.Id && x.Module == "Invoice")
            .ToDictionaryAsync(x => x.Key, x => x);

        // 4. Yeni veya güncellenecek ayarları işle (Invoice)
        foreach (var setting in invoiceSettings)
        {
            if (existingSettings.TryGetValue(setting.Key, out var existingSetting))
            {
                // Varsa güncelle
                if (existingSetting.Value != setting.Value)
                {
                    existingSetting.Value = setting.Value;
                }
            }
            else
            {
                // Yoksa ekle
                context.LocalizedStrings.Add(new LocalizedString
                {
                    LanguageId = engLang.Id,
                    Key = setting.Key,
                    Value = setting.Value,
                    Module = "Invoice"
                });
            }
        }

        // ---------------------------------------------------------
        // PROFORMA AYARLARI
        // ---------------------------------------------------------

        // Proforma için Invoice ayarlarını kopyala
        var proformaSettings = new Dictionary<string, string>(invoiceSettings);
        
        // Sadece farklı olanları ez veya ekle
        proformaSettings["Invoice_Title"] = "PROFORMA INVOICE"; // Başlık farklı
        // Diğerlerinde key aynı kalsın (örn: Label_Invoice_No -> Invoice No :) 
        // İstenirse key'ler de 'Proforma_Title' gibi ayrılabilir ama kodda ortak kullanım için 'Invoice_Title' keyi 'Proforma' modülünde farklı değer alabilir.
        // Ancak GetInvoiceQuery içinde settings["Invoice_Title"] deniyor.
        // GetProformaInvoiceQuery tarafı henüz dinamik değil ama olacağında aynı keyleri kullanmak mantıklı.
        
        // 5. Mevcut ayarları çek (Proforma)
        var existingProformaSettings = await context.LocalizedStrings
            .Where(x => x.LanguageId == engLang.Id && x.Module == "Proforma")
            .ToDictionaryAsync(x => x.Key, x => x);

        // 6. Yeni veya güncellenecek ayarları işle (Proforma)
        foreach (var setting in proformaSettings)
        {
            if (existingProformaSettings.TryGetValue(setting.Key, out var existingSetting))
            {
                // Varsa güncelle
                if (existingSetting.Value != setting.Value)
                {
                    existingSetting.Value = setting.Value;
                }
            }
            else
            {
                // Yoksa ekle
                context.LocalizedStrings.Add(new LocalizedString
                {
                    LanguageId = engLang.Id,
                    Key = setting.Key,
                    Value = setting.Value,
                    Module = "Proforma"
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
