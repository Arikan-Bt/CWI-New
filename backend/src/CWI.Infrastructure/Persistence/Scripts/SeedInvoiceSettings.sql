-- =============================================
-- Script Name: SeedInvoiceSettings.sql
-- Description: Inserts static settings for the Invoice module into LocalizedStrings table.
-- Generated: 22.01.2026
-- =============================================

DECLARE @LangId INT;

-- Get Language ID for English ('en')
SELECT TOP 1 @LangId = Id FROM Languages WHERE Code = 'en';

IF @LangId IS NOT NULL
BEGIN
    PRINT 'Found English Language ID: ' + CAST(@LangId AS NVARCHAR(10));
    PRINT 'Seeding Invoice Settings...';

    -- Temp table to hold the data to be inserted/updated
    CREATE TABLE #InvoiceParams (
        [Key] NVARCHAR(100) COLLATE DATABASE_DEFAULT,
        [Value] NVARCHAR(MAX) COLLATE DATABASE_DEFAULT,
        [Module] NVARCHAR(50) COLLATE DATABASE_DEFAULT
    );

    -- Insert static data into temp table
    INSERT INTO #InvoiceParams ([Key], [Value], [Module]) VALUES 
    
    -- Titles & Labels
    ('Invoice_Title', 'INVOICE', 'Invoice'),
    ('Label_To', 'To :', 'Invoice'),
    ('Label_Attn', 'Attn :', 'Invoice'),
    ('Label_Order_Date', 'Order Date :', 'Invoice'),
    ('Label_Collection_No', 'Collection No :', 'Invoice'),
    ('Label_Invoice_No', 'Invoice No :', 'Invoice'),

    -- Table Headers
    ('Header_Item_No', 'ITEM NO', 'Invoice'),
    ('Header_Item_Code', 'ITEM CODE', 'Invoice'),
    ('Header_Description', 'DESCRIPTION', 'Invoice'),
    ('Header_Qty', 'QTY', 'Invoice'),
    ('Header_Unit_Price', 'UNIT PRICE', 'Invoice'),
    ('Header_Currency', 'CURRENCY', 'Invoice'),
    ('Header_Sub_Total', 'SUB TOTAL', 'Invoice'),

    -- Totals
    ('Label_Total_Qty', 'Total Qty :', 'Invoice'),
    ('Label_Total_Amount', 'Total Amount :', 'Invoice'),

    -- Sections
    ('Label_Bank_Instruction', 'Bank Transfer Instruction', 'Invoice'),

    -- Address & Terms
    ('Company_Address_Line1', 'UNIT C.16/F OF BLOCK 1, WAH FUNG INDUSTRIAL CENTRE', 'Invoice'),
    ('Company_Address_Line2', '33-39 KWAI FUNG CRESCENT, KWAI CHUNG, HONG KONG', 'Invoice'),
    ('Company_Phone', '(852) 3525 0285', 'Invoice'),

    -- Term Labels
    ('Term_Transportation_Label', 'Transportation Fee :', 'Invoice'),
    ('Term_Payment_Label', 'Payment :', 'Invoice'),
    ('Term_Delivery_Label', 'Delivery :', 'Invoice'),
    ('Term_Shipment_Label', 'Shipment :', 'Invoice'),

    -- Term Values
    ('Term_Transportation', 'Paid By Buyer', 'Invoice'),
    ('Term_Payment', 'In Advance Before Shipment', 'Invoice'),
    ('Term_Delivery', 'EXW HK', 'Invoice'),
    ('Term_Shipment', 'AIR SHIPMENT', 'Invoice'),

    -- Bank Labels
    ('Label_Bank', 'Bank :', 'Invoice'),
    ('Label_Bank_Acc_No', 'Bank Acc No :', 'Invoice'),
    ('Label_Swift_Code', 'SWIFT Code :', 'Invoice'),
    ('Label_Bank_Address', 'Bank Address :', 'Invoice'),

    -- Bank Values
    ('Bank_Name', 'Standard Chartered Bank (Hong Kong) Limited Bank', 'Invoice'),
    ('Bank_Account', '36807798556', 'Invoice'),
    ('Bank_Swift', 'SCBLHKHHXXX', 'Invoice'),
    ('Bank_Address', '3/F, Standard Chartered Bank Building, 4-4A Des Voeux Road Central, Hong Kong', 'Invoice'),

    -- Footer
    ('Footer_Note', 'Your Order will not be shipped until we receive payment', 'Invoice');

    -- MERGE (Upsert) Logic
    MERGE LocalizedStrings AS target
    USING #InvoiceParams AS source
    ON (target.LanguageId = @LangId AND target.[Key] = source.[Key] AND target.Module = source.[Module])
    WHEN MATCHED THEN
        UPDATE SET target.Value = source.Value
    WHEN NOT MATCHED THEN
        INSERT (LanguageId, [Key], Value, Module)
        VALUES (@LangId, source.[Key], source.[Value], source.[Module]);

    DROP TABLE #InvoiceParams;

    PRINT 'Invoice Settings seeded successfully.';
END
ELSE
BEGIN
    PRINT 'ERROR: English Language (Code=''en'') not found in Languages table. Aborting seed.';
END
