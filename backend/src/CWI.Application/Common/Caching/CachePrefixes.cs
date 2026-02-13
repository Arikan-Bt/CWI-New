namespace CWI.Application.Common.Caching;

public static class CachePrefixes
{
    public const string LookupCurrencies = "lookup:payments:currencies";
    public const string LookupBrandsReports = "lookup:reports:brands";
    public const string LookupBrandsProducts = "lookup:products:brands";
    public const string LookupCustomers = "lookup:reports:customers";
    public const string LookupPaymentMethods = "lookup:reports:payment-methods";
    public const string LookupShipmentTerms = "lookup:reports:shipment-terms";
    public const string LookupWarehouses = "lookup:inventory:warehouses";

    public const string LookupRoles = "lookup:roles";
    public const string LookupUsers = "lookup:users";
    public const string LookupProductSalesPrices = "lookup:product-sales-prices";
    public const string LookupProductPurchasePrices = "lookup:product-purchase-prices";
}
