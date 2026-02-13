namespace CWI.Domain.Constants;

/// <summary>
/// Uygulama genelindeki tüm yetki anahtarlarını tanımlar.
/// </summary>
public static class Permissions
{
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
    }

    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
    }

    public static class Orders
    {
        public const string View = "Permissions.Orders.View";
        public const string Create = "Permissions.Orders.Create";
        public const string Edit = "Permissions.Orders.Edit";
        public const string Delete = "Permissions.Orders.Delete";
        public const string Approve = "Permissions.Orders.Approve";
    }

    public static class Inventory
    {
        public const string View = "Permissions.Inventory.View";
        public const string Adjust = "Permissions.Inventory.Adjust";
        public const string WarehouseManagement = "Permissions.Inventory.WarehouseManagement";
    }

    public static class Customers
    {
        public const string View = "Permissions.Customers.View";
        public const string Create = "Permissions.Customers.Create";
        public const string Edit = "Permissions.Customers.Edit";
        public const string Delete = "Permissions.Customers.Delete";
    }

    public static class Reports
    {
        public const string Sales = "Permissions.Reports.Sales";
        public const string Inventory = "Permissions.Reports.Inventory";
        public const string Customers = "Permissions.Reports.Customers";
    }

    public static class Menus
    {
        public const string Dashboard = "Permissions.Menus.Dashboard";

        // Sales Menus
        public const string Sales = "Permissions.Menus.Sales";
        public const string Sales_PaymentReceived = "Permissions.Menus.Sales.PaymentReceived";
        public const string Sales_SalesOrder = "Permissions.Menus.Sales.SalesOrder";
        public const string Sales_ItemOrderCheck = "Permissions.Menus.Sales.ItemOrderCheck";
        public const string Sales_CustomerPaymentDetails = "Permissions.Menus.Sales.CustomerPaymentDetails";
        public const string Sales_SummaryCustomer = "Permissions.Menus.Sales.SummaryCustomer";
        public const string Sales_CustomerBalance = "Permissions.Menus.Sales.CustomerBalance";

        // Purchase Menus
        public const string Purchase = "Permissions.Menus.Purchase";
        public const string Purchase_PurchaseInvoice = "Permissions.Menus.Purchase.PurchaseInvoice";
        public const string Purchase_PaymentsMade = "Permissions.Menus.Purchase.PaymentsMade";
        public const string Purchase_VendorBalance = "Permissions.Menus.Purchase.VendorBalance";
        public const string Purchase_StockAdjustment = "Permissions.Menus.Purchase.StockAdjustment";
        public const string Purchase_PurchaseOrderEntry = "Permissions.Menus.Purchase.PurchaseOrderEntry";
        public const string Purchase_VendorProducts = "Permissions.Menus.Purchase.VendorProducts";

        // Inventory Menus (Existing constant, keeping it)
        public const string Inventory = "Permissions.Menus.Inventory";

        // Reports Menus
        public const string Reports = "Permissions.Menus.Reports";
        public const string Reports_OrdersToBeApproved = "Permissions.Menus.Reports.OrdersToBeApproved";
        public const string Reports_OrdersDetail = "Permissions.Menus.Reports.OrdersDetail";
        public const string Reports_StockReport = "Permissions.Menus.Reports.StockReport";
        public const string Reports_PurchaseOrders = "Permissions.Menus.Reports.PurchaseOrders";
        public const string Reports_PurchaseOrdersInvoice = "Permissions.Menus.Reports.PurchaseOrdersInvoice";

        // Settings Menus
        public const string Settings = "Permissions.Menus.Settings";
        public const string Settings_UserManagement = "Permissions.Menus.Settings.UserManagement";
        public const string Settings_RoleManagement = "Permissions.Menus.Settings.RoleManagement";
        public const string Settings_CustomerManagement = "Permissions.Menus.Settings.CustomerManagement";
        public const string Settings_WarehouseManagement = "Permissions.Menus.Settings.WarehouseManagement";
        public const string Settings_BrandManagement = "Permissions.Menus.Settings.BrandManagement";
        public const string Settings_PurchasePriceManagement = "Permissions.Menus.Settings.PurchasePriceManagement";
        public const string Settings_SalesPriceManagement = "Permissions.Menus.Settings.SalesPriceManagement";
    }

    /// <summary>
    /// Marka yönetimi yetkileri
    /// </summary>
    public static class Brands
    {
        public const string View = "Permissions.Brands.View";
        public const string Create = "Permissions.Brands.Create";
        public const string Edit = "Permissions.Brands.Edit";
        public const string Delete = "Permissions.Brands.Delete";
    }
    /// <summary>
    /// Satın alma fiyat yönetimi yetkileri
    /// </summary>
    public static class PurchasePrices
    {
        public const string View = "Permissions.PurchasePrices.View";
        public const string Create = "Permissions.PurchasePrices.Create";
        public const string Edit = "Permissions.PurchasePrices.Edit";
        public const string Delete = "Permissions.PurchasePrices.Delete";
    }

    /// <summary>
    /// Satış fiyat yönetimi yetkileri
    /// </summary>
    public static class SalesPrices
    {
        public const string View = "Permissions.SalesPrices.View";
        public const string Create = "Permissions.SalesPrices.Create";
        public const string Edit = "Permissions.SalesPrices.Edit";
        public const string Delete = "Permissions.SalesPrices.Delete";
    }

    /// <summary>
    /// Sistem izleme yetkileri
    /// </summary>
    public static class System
    {
        public const string ErrorLogsView = "Permissions.System.ErrorLogs.View";
        public const string ErrorLogsResolve = "Permissions.System.ErrorLogs.Resolve";
    }
}
