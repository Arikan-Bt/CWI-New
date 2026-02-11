import { Routes } from '@angular/router';
import { Dashboard } from './dashboard/dashboard';

export default [
  { path: 'dashboard', component: Dashboard },
  // Sales
  {
    path: 'sales/payment-received',
    loadComponent: () =>
      import('./sales/payment-received/payment-received.component').then((m) => m.PaymentReceived),
  },
  {
    path: 'sales/sales-order',
    loadComponent: () =>
      import('./sales/sales-order/sales-order.component').then((m) => m.SalesOrder),
  },
  {
    path: 'sales/item-order-check',
    loadComponent: () =>
      import('./sales/item-order-check/item-order-check.component').then((m) => m.ItemOrderCheck),
  },
  {
    path: 'sales/customer-payment-details',
    loadComponent: () =>
      import('./sales/customer-payment-details/customer-payment-details.component').then(
        (m) => m.CustomerPaymentDetails,
      ),
  },
  {
    path: 'sales/summary-customer',
    loadComponent: () =>
      import('./sales/summary-customer/summary-customer.component').then((m) => m.SummaryCustomer),
  },
  {
    path: 'sales/customer-balance',
    loadComponent: () =>
      import('./sales/customer-balance/customer-balance.component').then((m) => m.CustomerBalance),
  },
  {
    path: 'sales/sales-detail',
    loadComponent: () =>
      import('./sales/sales-detail/sales-detail.component').then((m) => m.SalesDetail),
  },
  // Purchase
  {
    path: 'purchase/purchase-invoice',
    loadComponent: () =>
      import('./purchase/purchase-invoice/purchase-invoice.component').then(
        (m) => m.PurchaseInvoice,
      ),
  },
  {
    path: 'purchase/payments-made',
    loadComponent: () =>
      import('./purchase/payments-made/payments-made.component').then((m) => m.PaymentsMade),
  },
  {
    path: 'purchase/vendor-balance',
    loadComponent: () =>
      import('./purchase/vendor-balance/vendor-balance.component').then((m) => m.VendorBalance),
  },
  {
    path: 'purchase/stock-adjustment',
    loadComponent: () =>
      import('./purchase/stock-adjustment/stock-adjustment.component').then(
        (m) => m.StockAdjustment,
      ),
  },
  {
    path: 'purchase/purchase-order-entry',
    loadComponent: () =>
      import('./purchase/purchase-order-entry/purchase-order-entry.component').then(
        (m) => m.PurchaseOrderEntry,
      ),
  },
  {
    path: 'purchase/vendor-products',
    loadComponent: () =>
      import('./purchase/vendor-products/vendor-products.component').then((m) => m.VendorProducts),
  },
  {
    path: 'purchase/purchase-receive',
    loadComponent: () =>
      import('./purchase/purchase-receive/purchase-receive.component').then(
        (m) => m.PurchaseReceive,
      ),
  },
  // Reports
  {
    path: 'reports/orders-report',
    loadComponent: () =>
      import('./reports/orders-report/orders-report.component').then((m) => m.OrdersReport),
  },
  {
    path: 'reports/orders-detail',
    loadComponent: () =>
      import('./reports/orders-to-be-approved/orders-to-be-approved.component').then(
        (m) => m.OrdersToBeApproved,
      ),
  },
  {
    path: 'reports/stock-report',
    loadComponent: () =>
      import('./reports/stock-report/stock-report.component').then((m) => m.StockReport),
  },
  {
    path: 'reports/purchase-order',
    loadComponent: () =>
      import('./reports/purchase-order/purchase-order.component').then((m) => m.PurchaseOrder),
  },
  {
    path: 'reports/purchase-orders-invoice',
    loadComponent: () =>
      import('./reports/purchase-order-invoice/purchase-order-invoice.component').then(
        (m) => m.PurchaseOrderInvoice,
      ),
  },
  {
    path: 'reports/pre-order',
    loadComponent: () => import('./reports/pre-order/pre-order.component').then((m) => m.PreOrder),
  },
  // Settings
  {
    path: 'settings/brands',
    loadComponent: () => import('./settings/brand-management').then((m) => m.BrandManagement),
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
] as Routes;
