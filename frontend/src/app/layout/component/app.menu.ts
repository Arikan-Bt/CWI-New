import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AppMenuitem } from './app.menuitem';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [CommonModule, AppMenuitem, RouterModule],
  template: `
    <ul class="layout-menu">
      @for (item of model; track item.label; let i = $index) {
        @if (!item.separator) {
          <li app-menuitem [item]="item" [index]="i" [root]="true"></li>
        } @else {
          <li class="menu-separator"></li>
        }
      }
    </ul>
  `,
})
export class AppMenu implements OnInit {
  private authService = inject(AuthService);
  model: any[] = [];

  ngOnInit() {
    const rawModel = [
      {
        label: 'Home',
        items: [
          {
            label: 'Dashboard',
            icon: 'pi pi-fw pi-home',
            routerLink: ['/dashboard'],
            permission: 'Permissions.Menus.Dashboard',
          },
        ],
      },
      {
        label: 'General',
        items: [
          {
            label: 'Reports',
            icon: 'pi pi-fw pi-chart-bar',
            permission: 'Permissions.Menus.Reports',
            items: [
              {
                label: 'Orders Report',
                icon: 'pi pi-fw pi-check-square',
                routerLink: ['/pages/reports/orders-report'],
                permission: 'Permissions.Menus.Reports.OrdersToBeApproved',
              },
              {
                label: 'Orders Detail',
                icon: 'pi pi-fw pi-file-excel',
                routerLink: ['/pages/reports/orders-detail'],
                permission: 'Permissions.Menus.Reports.OrdersDetail',
              },
              {
                label: 'Stock Report',
                icon: 'pi pi-fw pi-box',
                routerLink: ['/pages/reports/stock-report'],
                permission: 'Permissions.Menus.Reports.StockReport',
              },
              {
                label: 'Purchase Orders',
                icon: 'pi pi-fw pi-shopping-bag',
                routerLink: ['/pages/reports/purchase-order'],
                permission: 'Permissions.Menus.Reports.PurchaseOrders',
              },
              {
                label: 'Purchase Orders Invoice',
                icon: 'pi pi-fw pi-receipt',
                routerLink: ['/pages/reports/purchase-orders-invoice'],
                permission: 'Permissions.Menus.Reports.PurchaseOrdersInvoice',
              },
              {
                label: 'Item Order Check',
                icon: 'pi pi-fw pi-check-square',
                routerLink: ['/pages/sales/item-order-check'],
                permission: 'Permissions.Menus.Sales.ItemOrderCheck',
              },
            ],
          },
          {
            label: 'Sales',
            icon: 'pi pi-fw pi-shopping-bag',
            permission: 'Permissions.Menus.Sales',
            items: [
              {
                label: 'Sales Order',
                icon: 'pi pi-fw pi-shopping-bag',
                routerLink: ['/pages/sales/sales-order'],
                permission: 'Permissions.Menus.Sales.SalesOrder',
              },
              {
                label: 'Sales Details',
                icon: 'pi pi-fw pi-file',
                routerLink: ['/pages/sales/sales-detail'],
                permission: 'Permissions.Menus.Reports.OrdersToBeApproved',
              },
              {
                label: 'Customer Balance',
                icon: 'pi pi-fw pi-credit-card',
                routerLink: ['/pages/sales/customer-balance'],
                permission: 'Permissions.Menus.Sales.CustomerBalance',
              },
              {
                label: 'Customer Payment Details',
                icon: 'pi pi-fw pi-chart-line',
                routerLink: ['/pages/sales/customer-payment-details'],
                permission: 'Permissions.Menus.Sales.CustomerPaymentDetails',
              },
              {
                label: 'Summary Customer',
                icon: 'pi pi-fw pi-users',
                routerLink: ['/pages/sales/summary-customer'],
                permission: 'Permissions.Menus.Sales.SummaryCustomer',
              },
              {
                label: 'Payment Received',
                icon: 'pi pi-fw pi-wallet',
                routerLink: ['/pages/sales/payment-received'],
                permission: 'Permissions.Menus.Sales.PaymentReceived',
              },
            ],
          },
          {
            label: 'Purchase',
            icon: 'pi pi-fw pi-shopping-cart',
            permission: 'Permissions.Menus.Purchase',
            items: [
              {
                label: 'Purchase Order Entry',
                icon: 'pi pi-fw pi-shopping-cart',
                routerLink: ['/pages/purchase/purchase-order-entry'],
                permission: 'Permissions.Menus.Purchase.PurchaseOrderEntry',
              },
              {
                label: 'Purchase Receive',
                icon: 'pi pi-fw pi-download',
                routerLink: ['/pages/purchase/purchase-receive'],
                permission: 'Permissions.Menus.Purchase.PurchaseOrderEntry',
              },
              {
                label: 'Purchase Invoice',
                icon: 'pi pi-fw pi-file',
                routerLink: ['/pages/purchase/purchase-invoice'],
                permission: 'Permissions.Menus.Purchase.PurchaseInvoice',
              },
              {
                label: 'Payments Made',
                icon: 'pi pi-fw pi-send',
                routerLink: ['/pages/purchase/payments-made'],
                permission: 'Permissions.Menus.Purchase.PaymentsMade',
              },
              {
                label: 'Vendor Balance',
                icon: 'pi pi-fw pi-building',
                routerLink: ['/pages/purchase/vendor-balance'],
                permission: 'Permissions.Menus.Purchase.VendorBalance',
              },
              {
                label: 'Stock Adjustment',
                icon: 'pi pi-fw pi-box',
                routerLink: ['/pages/purchase/stock-adjustment'],
                permission: 'Permissions.Menus.Purchase.StockAdjustment',
              },
              {
                label: 'Vendor Products',
                icon: 'pi pi-fw pi-table',
                routerLink: ['/pages/purchase/vendor-products'],
                permission: 'Permissions.Menus.Purchase.VendorProducts',
              },
            ],
          },
          {
            label: 'Settings',
            icon: 'pi pi-fw pi-cog',
            permission: 'Permissions.Menus.Settings',
            items: [
              {
                label: 'User Management',
                icon: 'pi pi-fw pi-user',
                routerLink: ['/pages/settings/users'],
                permission: 'Permissions.Menus.Settings.UserManagement',
              },
              {
                label: 'Role Management',
                icon: 'pi pi-fw pi-shield',
                routerLink: ['/pages/settings/roles'],
                permission: 'Permissions.Menus.Settings.RoleManagement',
              },
              {
                label: 'Customer Management',
                icon: 'pi pi-fw pi-building',
                routerLink: ['/pages/settings/customers'],
                permission: 'Permissions.Menus.Settings.CustomerManagement',
              },
              {
                label: 'Warehouse Management',
                icon: 'pi pi-fw pi-warehouse',
                routerLink: ['/pages/settings/warehouses'],
                permission: 'Permissions.Menus.Settings.WarehouseManagement',
              },
              {
                label: 'Brand Management',
                icon: 'pi pi-fw pi-tags',
                routerLink: ['/pages/settings/brands'],
                permission: 'Permissions.Menus.Settings.BrandManagement',
              },
            ],
          },
        ],
      },
    ];

    this.model = this.filterMenu(rawModel);
  }

  /**
   * Menüyü yetkilere göre filtreler
   */
  private filterMenu(items: any[]): any[] {
    return items
      .map((item) => {
        const newItem = { ...item };

        // Alt öğeleri varsa onları da filtrele
        if (newItem.items) {
          newItem.items = this.filterMenu(newItem.items);
        }

        // Görünürlük kontrolü
        let isVisible = true;

        // 1. Eğer bir yetki anahtarı varsa kontrol et
        if (newItem.permission) {
          isVisible = this.authService.hasPermission(newItem.permission);
        }

        // 2. Eğer routerLink yoksa ama items varsa (Kategori), alt öğelerinden en az biri görünür olmalı
        if (!newItem.routerLink && newItem.items && newItem.items.length === 0) {
          isVisible = false;
        }

        return isVisible ? newItem : null;
      })
      .filter((item) => item !== null);
  }
}
