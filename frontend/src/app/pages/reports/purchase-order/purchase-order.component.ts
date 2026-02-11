import {
  Component,
  ChangeDetectionStrategy,
  signal,
  inject,
  computed,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DatePickerModule } from 'primeng/datepicker';
import { FluidModule } from 'primeng/fluid';
import { MessageService } from 'primeng/api';
import {
  PurchaseOrderService,
  PurchaseOrderDto,
} from '../../../core/services/purchase-order.service';
import { InventoryService, WarehouseDto } from '../../../core/services/inventory.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-purchase-order',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, TableModule, DatePickerModule, FluidModule],
  template: `
    <div class="purchase-order-page">
      <div class="flex flex-col gap-4">
        <!-- Filtre Paneli -->
        <div class="card p-4">
          <div class="font-semibold text-xl mb-4">Purchase Orders</div>
          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-12 gap-6 items-end">
              <div class="md:col-span-5 flex flex-col gap-2">
                <label for="startDate" class="font-medium text-sm text-surface-600 text-left"
                  >Starting Date</label
                >
                <p-datepicker
                  id="startDate"
                  [(ngModel)]="filters().startDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="06.04.2025"
                ></p-datepicker>
              </div>
              <div class="md:col-span-5 flex flex-col gap-2">
                <label for="endDate" class="font-medium text-sm text-surface-600 text-left"
                  >Ending Date</label
                >
                <p-datepicker
                  id="endDate"
                  [(ngModel)]="filters().endDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="06.01.2026"
                ></p-datepicker>
              </div>
              <div class="md:col-span-2 flex items-end justify-end">
                <p-button
                  label="Show Details"
                  styleClass="w-full btn-action-red font-bold"
                  (onClick)="onShowDetails()"
                  [loading]="loading()"
                ></p-button>
              </div>
            </div>

            <div class="flex flex-wrap items-center gap-3 mt-6">
              <span class="text-sm font-medium text-surface-500">Quick Select:</span>
              <div class="flex flex-wrap gap-2">
                <p-button
                  label="1st Quarter"
                  (onClick)="setQuarterFilters(1)"
                  severity="secondary"
                  size="small"
                  [outlined]="true"
                  styleClass="text-xs"
                ></p-button>
                <p-button
                  label="2nd Quarter"
                  (onClick)="setQuarterFilters(2)"
                  severity="secondary"
                  size="small"
                  [outlined]="true"
                  styleClass="text-xs"
                ></p-button>
                <p-button
                  label="3rd Quarter"
                  (onClick)="setQuarterFilters(3)"
                  severity="secondary"
                  size="small"
                  [outlined]="true"
                  styleClass="text-xs"
                ></p-button>
                <p-button
                  label="4th Quarter"
                  (onClick)="setQuarterFilters(4)"
                  severity="secondary"
                  size="small"
                  [outlined]="true"
                  styleClass="text-xs"
                ></p-button>
              </div>
            </div>
          </p-fluid>
        </div>

        <!-- Sipariş Tablosu -->
        <div class="card p-0 overflow-hidden">
          <p-table
            [value]="orders()"
            [rows]="10"
            [paginator]="true"
            [lazy]="true"
            (onLazyLoad)="loadOrders($event)"
            [totalRecords]="totalRecords()"
            [rowsPerPageOptions]="[10, 20, 50]"
            styleClass="p-datatable-sm custom-purchase-table"
            [responsiveLayout]="'scroll'"
            paginatorDropdownAppendTo="body"
          >
            <ng-template pTemplate="header">
              <tr>
                <th style="width: 120px">Date</th>
                <th>Order Ref No</th>
                <th>Customer Svc</th>
                <th class="text-right" style="width: 100px">QTY (PCS)</th>
                <th class="text-right" style="width: 120px">Amount</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-order>
              <tr>
                <td class="font-bold">{{ order.date | date: 'd.MM.yyyy' }}</td>
                <td class="font-medium">{{ order.orderRefNo }}</td>
                <td>{{ order.customerSvc }}</td>
                <td class="text-right">{{ order.qty | number: '1.0-3' }}</td>
                <td class="text-right font-bold">
                  {{ order.amount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="footer">
              <tr class="font-bold bg-surface-900 text-white dark:bg-black">
                <td colspan="3" class="text-right py-3 pr-4">Total (Current Page)</td>
                <td class="text-right">{{ pageTotals().qty | number: '1.0-3' }}</td>
                <td class="text-right">
                  {{ pageTotals().amount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="5" class="text-center p-8 text-surface-400">
                  No purchase orders found.
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      :host ::ng-deep {
        /* Modal Table Styles */
        .custom-modal-table {
          border: none !important;

          .p-datatable-wrapper {
            border: none !important;
          }

          .p-datatable-header,
          .p-datatable-thead > tr > th {
            background-color: #f8fafc;
            border-bottom: 2px solid #e2e8f0;
            border-top: none;
            border-left: none;
            border-right: none;
            color: #475569;
            font-weight: 700;
            padding: 1rem;
          }

          .p-datatable-tbody > tr {
            border: none !important;
            background-color: transparent !important;

            > td {
              border-bottom: 1px solid #f1f5f9;
              border-left: none;
              border-right: none;
              padding: 0.75rem;
              vertical-align: middle;
            }

            &:last-child > td {
              border-bottom: none;
            }
          }

          .dark .p-datatable-thead > tr > th {
            background-color: #1e1e1e;
            border-color: #333;
            color: #e2e8f0;
          }

          .dark .p-datatable-tbody > tr > td {
            border-color: #333;
          }

          .header-group-title {
            font-weight: 800;
            color: #334155;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            border-bottom: 2px solid #e2e8f0 !important;
          }
          .dark .header-group-title {
            color: #94a3b8;
            border-color: #333 !important;
          }

          .p-inputnumber-input {
            padding: 0.4rem 0.5rem !important;
            font-size: 0.875rem !important;
            border-radius: 4px;
            background: #ffffff;
            border: 1px solid #cbd5e1;
            text-align: center;
          }
          .dark .p-inputnumber-input {
            background: #1e1e1e;
            border-color: #475569;
            color: white;
          }
        }

        /* Genel Sayfa Yapısı */
        .purchase-order-page {
          .card {
            background: var(--p-surface-card);
            border: 1px solid var(--p-border-color);
            border-radius: 12px;
            box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.05);
          }
        }

        /* Tablo Stilleri */
        .custom-purchase-table {
          .p-datatable-thead > tr > th {
            background: #1e1e1e !important; /* Görseldeki koyu başlık */
            color: #ffffff !important;
            border-color: #333 !important;
            font-size: 12px !important;
            padding: 14px 10px !important;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;
          }

          .p-datatable-tbody > tr {
            background: var(--p-surface-card) !important;
            color: var(--p-text-color);

            > td {
              border-color: var(--p-border-color);
              padding: 12px 10px;
              font-size: 13px;
              vertical-align: middle;
            }

            &:hover {
              background: var(--p-surface-hover) !important;
            }
          }

          /* Paginator */
          .p-paginator {
            background: transparent !important;
            border-top: 1px solid var(--p-border-color) !important;
            padding: 1rem;
          }
        }

        /* Özel Kırmızı/Pembe Butonlar (Görseldeki Ton) */
        .btn-action-red {
          background: #e31e24 !important; /* Standart kırmızı yapıldı */
          border: none !important;
          color: #ffffff !important;
          font-weight: 700 !important;
          transition: all 0.2s ease;
          border-radius: 6px;

          &:hover {
            background: #c1191f !important;
            transform: translateY(-1px);
            box-shadow: 0 4px 12px rgba(227, 30, 36, 0.25);
          }

          &:active {
            transform: translateY(0);
          }

          &.p-button-sm {
            font-size: 11px !important;
            padding: 4px 12px !important;
          }
        }

        /* Dark Mode Spesifik Ayarlar */
        .dark .custom-purchase-table {
          .p-datatable-thead > tr > th {
            background: #121212 !important;
            border-color: #2a2a2a !important;
          }

          .p-datatable-tbody > tr {
            background: #1e1e1e !important;
            > td {
              border-color: #2a2a2a !important;
            }
          }
        }

        /* Input ve Seçim Alanları */
        .p-datepicker,
        .p-select {
          width: 100%;
          border-radius: 8px !important;

          &.p-inputtext,
          .p-select-label {
            font-size: 13px !important;
          }
        }
      }

      /* Görseldeki Show Details butonu genişliği için özel ayar */
      .show-details-container {
        display: flex;
        align-items: flex-end;
        height: 100%;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PurchaseOrder implements OnInit {
  private messageService = inject(MessageService);
  private purchaseService = inject(PurchaseOrderService);
  private inventoryService = inject(InventoryService);

  // Filtreler
  filters = signal<{ startDate: Date | null; endDate: Date | null }>({
    startDate: null,
    endDate: null,
  });

  /**
   * Seçilen çeyrek döneme göre tarih filtrelerini ayarlar (1-3, 4-6, 7-9, 10-12 aylar).
   * @param quarter Çeyrek dönem numarası (1, 2, 3 veya 4)
   */
  setQuarterFilters(quarter: number) {
    const year = new Date().getFullYear();
    let start: Date;
    let end: Date;

    switch (quarter) {
      case 1:
        start = new Date(year, 0, 1);
        end = new Date(year, 2, 31);
        break;
      case 2:
        start = new Date(year, 3, 1);
        end = new Date(year, 5, 30);
        break;
      case 3:
        start = new Date(year, 6, 1);
        end = new Date(year, 8, 30);
        break;
      case 4:
        start = new Date(year, 9, 1);
        end = new Date(year, 11, 31);
        break;
      default:
        return;
    }

    this.filters.update((prev) => ({
      ...prev,
      startDate: start,
      endDate: end,
    }));

    // Seçim yapıldıktan sonra raporu otomatik olarak tazele
    this.onShowDetails();
  }

  // Yükleme durumu
  loading = signal(false);

  /* Sayfa Toplamları (Computed Signal) */
  pageTotals = computed(() => {
    const data = this.orders();
    return data.reduce(
      (acc, curr) => ({
        qty: acc.qty + (curr.qty || 0),
        amount: acc.amount + (curr.amount || 0),
      }),
      { qty: 0, amount: 0 },
    );
  });

  orders = signal<PurchaseOrderDto[]>([]);
  totalRecords = signal(0);
  warehouses = signal<WarehouseDto[]>([]);

  ngOnInit() {
    this.loadWarehouses();
  }

  loadWarehouses() {
    this.inventoryService.getWarehouses().subscribe({
      next: (data) => {
        if (data.success && data.data) {
          this.warehouses.set(data.data);
        }
      },
    });
  }

  loadOrders(event?: any) {
    this.loading.set(true);

    const page = (event?.first || 0) / (event?.rows || 10) + 1;
    const pageSize = event?.rows || 10;

    this.purchaseService
      .getOrders({
        startDate: this.filters().startDate ?? undefined,
        endDate: this.filters().endDate ?? undefined,
        page,
        pageSize,
      })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.orders.set(response.data.data);
            this.totalRecords.set(response.data.totalCount);
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while loading orders.',
          });
        },
      });
  }

  /**
   * Detayları görüntüle butonu tetiklendiğinde (Ana sayfa)
   */
  onShowDetails() {
    this.loadOrders();
  }
}
