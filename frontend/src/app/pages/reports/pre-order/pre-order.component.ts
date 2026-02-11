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
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { BadgeModule } from 'primeng/badge';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { FluidModule } from 'primeng/fluid';
import { TooltipModule } from 'primeng/tooltip';
import { ReportService } from '../../../core/services/report.service';
import { OrderReportItem } from '../../../core/models/orders-report.models';

/**
 * Pre Order öğesi arayüzü
 */
export interface PreOrderItem {
  brand: string;
  orderDate: Date;
  requestedShipmentDate: Date;
  referenceCode: string;
  currentAccount: string;
  totalQty: number;
  discount: number;
  totalAmount: number;
  orderStatus: string;
}

@Component({
  selector: 'app-pre-order',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    DatePickerModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    BadgeModule,
    BadgeModule,
    FluidModule,
    TooltipModule,
  ],
  template: `
    <div class="pre-order-page">
      <div class="flex flex-col gap-4">
        <!-- Filtre Paneli -->
        <div class="card p-4">
          <div class="font-semibold text-xl mb-4">Pre Order Report</div>
          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-12 gap-6 items-end">
              <div class="md:col-span-5 flex flex-col gap-2">
                <label for="startDate" class="font-medium text-sm text-surface-600"
                  >Starting Date</label
                >
                <p-datepicker
                  id="startDate"
                  [(ngModel)]="startDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="08/02/2024"
                ></p-datepicker>
              </div>
              <div class="md:col-span-5 flex flex-col gap-2">
                <label for="endDate" class="font-medium text-sm text-surface-600"
                  >Ending Date</label
                >
                <p-datepicker
                  id="endDate"
                  [(ngModel)]="endDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="06.01.2026"
                ></p-datepicker>
              </div>
              <div class="md:col-span-2 flex items-end justify-end">
                <p-button
                  label="Show Details"
                  styleClass="w-full btn-action-red"
                  (onClick)="onShowDetails()"
                ></p-button>
              </div>
            </div>
          </p-fluid>
        </div>

        <!-- Sonuçlar Paneli -->
        <div class="card p-0 overflow-hidden border-surface-200 dark:border-surface-700">
          <!-- Üst Bilgi ve Arama -->
          <div
            class="p-4 bg-surface-0 dark:bg-surface-900 flex justify-between items-center flex-wrap gap-4 border-b border-surface-200 dark:border-surface-700"
          >
            <div class="flex items-center gap-4">
              <p-badge
                value="Beverly Hills Polo Club"
                severity="info"
                styleClass="px-3 py-1 bg-sky-500 text-white border-none text-xs font-bold"
              ></p-badge>
            </div>

            <div class="flex items-center gap-3">
              <span class="text-sm text-surface-500">Search:</span>
              <p-iconfield iconPosition="left">
                <p-inputicon class="pi pi-search text-xs"></p-inputicon>
                <input
                  pInputText
                  type="text"
                  [(ngModel)]="searchValue"
                  placeholder="Search in results..."
                  class="w-full md:w-64 h-8 text-xs bg-surface-50 dark:bg-surface-800 border-surface-200 dark:border-surface-700"
                />
              </p-iconfield>
            </div>
          </div>

          <!-- Tablo -->
          <p-table
            [value]="filteredData()"
            [rows]="rows()"
            [paginator]="true"
            [lazy]="true"
            (onLazyLoad)="loadData($event)"
            [totalRecords]="totalRecords()"
            [rowsPerPageOptions]="[10, 20, 50]"
            styleClass="p-datatable-sm custom-preorder-table"
            [responsiveLayout]="'scroll'"
            [showCurrentPageReport]="true"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
            paginatorDropdownAppendTo="body"
            (onPage)="rows.set($event.rows)"
            [loading]="loading()"
          >
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="brand">BRAND <p-sortIcon field="brand"></p-sortIcon></th>
                <th pSortableColumn="orderDate">
                  ORDER DATE <p-sortIcon field="orderDate"></p-sortIcon>
                </th>
                <th pSortableColumn="requestedShipmentDate">
                  REQ. SHIPMENT DATE <p-sortIcon field="requestedShipmentDate"></p-sortIcon>
                </th>
                <th pSortableColumn="referenceCode">
                  REFERENCE CODE <p-sortIcon field="referenceCode"></p-sortIcon>
                </th>
                <th pSortableColumn="currentAccount">
                  CURRENT ACCOUNT <p-sortIcon field="currentAccount"></p-sortIcon>
                </th>
                <th pSortableColumn="totalQty" class="text-right">
                  TOTAL QTY (PCS) <p-sortIcon field="totalQty"></p-sortIcon>
                </th>
                <th pSortableColumn="discount" class="text-right">
                  DISCOUNT <p-sortIcon field="discount"></p-sortIcon>
                </th>
                <th pSortableColumn="totalAmount" class="text-right">
                  TOTAL AMOUNT <p-sortIcon field="totalAmount"></p-sortIcon>
                </th>
                <th pSortableColumn="orderStatus">
                  ORDER STATUS <p-sortIcon field="orderStatus"></p-sortIcon>
                </th>
                <th class="text-center">ACTION</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-item>
              <tr>
                <td>{{ item.brand }}</td>
                <td>{{ item.orderDate | date: 'dd.MM.yyyy' }}</td>
                <td>{{ item.requestedShipmentDate | date: 'dd.MM.yyyy' }}</td>
                <td class="font-medium">{{ item.referenceCode }}</td>
                <td>{{ item.currentAccount }}</td>
                <td class="text-right">{{ item.totalQty | number }}</td>
                <td class="text-right">
                  {{ item.discount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right font-bold">
                  {{ item.totalAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td>
                  <span class="text-surface-600">{{ item.orderStatus }}</span>
                </td>
                <td class="text-center">
                  <p-button
                    label="Action"
                    severity="danger"
                    size="small"
                    styleClass="btn-table-action"
                    (onClick)="onAction(item)"
                  ></p-button>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="footer">
              <tr class="font-bold bg-surface-50">
                <td colspan="5" class="text-left py-3">Total</td>
                <td class="text-right">{{ totals().totalQty | number }}</td>
                <td class="text-right">0.00</td>
                <td class="text-right">
                  {{ totals().totalAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td colspan="2"></td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="10" class="text-center p-8 text-surface-400">No pre-orders found.</td>
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
        .pre-order-page {
          .card {
            background: var(--p-surface-card);
            border: 1px solid var(--p-border-color);
            border-radius: 12px;
            box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.05);
          }
        }

        .custom-preorder-table {
          .p-datatable-thead > tr > th {
            background: #f8f9fa !important;
            color: #495057 !important;
            border-bottom: 1px solid #e9ecef !important;
            font-size: 10px !important;
            padding: 10px 8px !important;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;

            .p-sortable-column-icon {
              font-size: 10px;
              margin-left: 4px;
            }
          }

          .p-datatable-tbody > tr > td {
            font-size: 12px;
            padding: 10px 8px;
            border-bottom: 1px solid #f1f3f5;
          }

          .p-datatable-tfoot > tr > td {
            font-size: 12px;
            padding: 12px 8px;
            background: #f8f9fa !important;
          }
        }

        /* Özel Kırmızı Butonlar */
        .btn-action-red {
          background: #e31e24 !important;
          border: none !important;
          color: #ffffff !important;
          font-weight: 700 !important;
          transition: all 0.2s;

          &:hover {
            background: #c1191f !important;
          }
        }

        .btn-table-action {
          background: #e31e24 !important;
          border: none !important;
          color: white !important;
          font-size: 10px !important;
          font-weight: 600 !important;
          padding: 4px 12px !important;

          &:hover {
            background: #c1191f !important;
          }
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PreOrder implements OnInit {
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);

  // Filtreler
  startDate = signal<Date | null>(new Date(2024, 0, 1)); // 01/01/2024
  endDate = signal<Date | null>(new Date());
  searchValue = signal('');
  rows = signal(10);
  loading = signal(false);
  totalRecords = signal(0);

  // Veriler
  // Reuse existing interface or map OrderReportItem to PreOrderItem structure
  // For simplicity, let's use the backend response structure but mapped to local view if needed
  // But PreOrder component uses 'PreOrderItem' interface locally defined.
  // We should probably map OrderReportItem to PreOrderItem.
  reportData = signal<PreOrderItem[]>([]); // Using local interface for now, populated from backend

  ngOnInit() {
    // Initial load handled by table lazy load or explicit call
    // this.loadData();
  }

  // Filtrelenmiş veri - Backend handles searching generally, but if we want client side search on the current page:
  // With lazy load, search usually triggers reload.
  filteredData = computed(() => {
    return this.reportData();
  });

  // Toplamlar
  totals = computed(() => {
    const data = this.reportData();
    return {
      totalQty: data.reduce((acc, curr) => acc + curr.totalQty, 0),
      totalAmount: data.reduce((acc, curr) => acc + curr.totalAmount, 0),
    };
  });

  /**
   * Verileri yükle
   */
  loadData(event?: any) {
    this.loading.set(true);

    // Check if dates are valid
    if (!this.startDate() || !this.endDate()) {
      this.loading.set(false);
      return;
    }

    const page = (event?.first || 0) / (event?.rows || 10) + 1;
    const pageSize = event?.rows || 10;

    // We reuse getOrdersReport but set Status to PreOrder
    // Assuming 'PreOrder' is the exact status string expected by backend enum parsing
    this.reportService
      .getOrdersReport({
        startDate: this.startDate()!.toISOString(),
        endDate: this.endDate()!.toISOString(),
        orderStatus: 'PreOrder',
        displayProductPhoto: false,
        page: page,
        pageSize: pageSize,
        // We can add search param if backend supports it broadly or use existing fields
      })
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            // Map API response to PreOrderItem
            const mappedItems: PreOrderItem[] = res.data.data.map((item: OrderReportItem) => ({
              brand: item.brand,
              orderDate: new Date(item.orderDate),
              requestedShipmentDate: item.requestedShipmentDate
                ? new Date(item.requestedShipmentDate)
                : new Date(),
              referenceCode: item.orderDetails, // Mapping OrderNumber/Ref to referenceCode
              currentAccount: item.currentAccountDescription,
              totalQty: item.totalQty,
              discount: item.discount,
              totalAmount: item.total,
              orderStatus: item.status,
            }));

            this.reportData.set(mappedItems);
            this.totalRecords.set(res.data.totalCount);
          }
          this.loading.set(false);
        },
        error: (err: any) => {
          this.loading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while loading data.',
          });
        },
      });
  }

  /**
   * Detayları görüntüle
   */
  onShowDetails() {
    // Trigger reload from page 1
    this.loadData({ first: 0, rows: this.rows() });
  }

  /**
   * Aksiyon işlemi
   */
  onAction(item: PreOrderItem) {
    this.messageService.add({
      severity: 'info',
      summary: 'Info',
      detail: `Action triggered for order ${item.referenceCode}.`,
    });
  }
}
