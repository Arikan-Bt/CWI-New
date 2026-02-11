import {
  Component,
  ChangeDetectionStrategy,
  signal,
  inject,
  computed,
  OnInit,
} from '@angular/core';
import { CommonModule, DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DatePickerModule } from 'primeng/datepicker';
import { TooltipModule } from 'primeng/tooltip';
import { FluidModule } from 'primeng/fluid';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { ReportService } from '../../../core/services/report.service';
import { PurchaseOrderInvoiceReportItem } from '../../../core/models/purchase-order-invoice-report.models';
import { finalize } from 'rxjs';
import { FullScreenModalComponent } from '../../../shared/components/full-screen-modal/full-screen-modal.component';
import {
  PurchaseOrderService,
  PurchaseOrderDetailDto,
  PurchaseOrderItemDto,
} from '../../../core/services/purchase-order.service';

@Component({
  selector: 'app-purchase-order-invoice',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    DatePickerModule,
    TooltipModule,
    FluidModule,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    ProgressSpinnerModule,
    DecimalPipe,
    DatePipe,
    FullScreenModalComponent,
  ],
  template: `
    <div class="purchase-order-invoice-page">
      <div class="flex flex-col gap-4">
        <!-- Detail Fullscreen Modal -->
        <app-full-screen-modal [(visible)]="isModalOpen">
          <div header class="text-xl font-bold flex items-center gap-2">
            <i class="pi pi-file-edit text-primary"></i>
            <span>Purchase Order Detail</span>
          </div>

          <div class="p-0">
            @if (isModalLoading()) {
              <div class="flex flex-col items-center justify-center p-20 gap-4">
                <p-progressspinner styleClass="w-12 h-12" strokeWidth="4"></p-progressspinner>
                <span class="text-muted-color animate-pulse">Loading details...</span>
              </div>
            } @else {
              <div class="overflow-hidden">
                <p-table
                  [value]="detailItems()"
                  styleClass="p-datatable-sm detail-modal-table"
                  [rowHover]="true"
                >
                  <ng-template pTemplate="header">
                    <!-- Group Headers -->
                    <tr>
                      <th colspan="2" class="header-group text-center">ORDER DETAILS</th>
                      <th
                        colspan="3"
                        class="header-group text-center bg-blue-50/50 dark:bg-blue-900/10"
                      >
                        ORDER
                      </th>
                      <th
                        colspan="2"
                        class="header-group text-center bg-green-50/50 dark:bg-green-900/10"
                      >
                        INVOICE
                      </th>
                    </tr>

                    <!-- Sub Info Row -->
                    <tr class="sub-info-row">
                      <th colspan="2" class="p-3">
                        <div class="flex items-center gap-2">
                          <span class="text-muted-color">Order Ref No :</span>
                          <span class="font-bold text-primary">{{
                            selectedInvoice()?.orderRefNo
                          }}</span>
                        </div>
                      </th>
                      <th colspan="3" class="p-3">
                        <div class="flex items-center gap-2">
                          <span class="text-muted-color">Order Date :</span>
                          <span class="font-bold">{{
                            selectedInvoice()?.invoiceDate | date: 'dd.MM.yyyy'
                          }}</span>
                        </div>
                      </th>
                      <th colspan="2" class="p-3">
                        <div class="flex items-center gap-2">
                          <span class="text-muted-color">Invoice Ref :</span>
                          <span class="font-bold">{{ selectedInvoice()?.invoiceRefNum }}</span>
                        </div>
                      </th>
                    </tr>

                    <!-- Column Headers -->
                    <tr class="column-headers">
                      <th style="width: 15%">Product Code</th>
                      <th style="width: 25%">Product Name</th>
                      <th style="width: 10%" class="text-center">QTY (PCS)</th>
                      <th style="width: 10%" class="text-right">Unit Price</th>
                      <th style="width: 10%" class="text-right">Amount</th>
                      <th style="width: 10%" class="text-center">QTY (PCS)</th>
                      <th style="width: 10%" class="text-right">Unit Price</th>
                    </tr>
                  </ng-template>

                  <ng-template pTemplate="body" let-item>
                    <tr class="detail-row">
                      <td class="font-bold text-primary-600">{{ item.productCode }}</td>
                      <td class="text-sm">{{ item.productName }}</td>
                      <td class="text-center font-medium">{{ item.orderQty | number: '1.0-2' }}</td>
                      <td class="text-right text-surface-600">
                        {{ item.orderUnitPrice | currency: 'USD' : 'symbol' : '1.2-2' }}
                      </td>
                      <td class="text-right font-medium">
                        {{ item.orderAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                      </td>
                      <td class="text-center">
                        <span
                          class="inline-flex items-center justify-center px-2 py-1 rounded bg-green-50 dark:bg-green-900/20 text-green-700 dark:text-green-400 font-bold border border-green-100 dark:border-green-800/30"
                        >
                          {{ item.receive | number: '1.0-2' }}
                        </span>
                      </td>
                      <td class="text-right font-medium">
                        {{ item.invoiceUnitPrice | currency: 'USD' : 'symbol' : '1.2-2' }}
                      </td>
                    </tr>
                  </ng-template>

                  <ng-template pTemplate="footer">
                    <tr class="totals-row font-bold">
                      <td colspan="2" class="text-right text-lg pr-4">Totals</td>
                      <td class="text-center text-lg text-primary">
                        {{ totalOrderQty() | number: '1.0-3' }}
                      </td>
                      <td></td>
                      <td></td>
                      <td
                        class="text-center text-lg text-green-600 bg-green-50/50 dark:bg-green-900/10 border-x border-green-100 dark:border-green-800/20"
                      >
                        {{ totalInvoiceQty() | number: '1.0-3' }}
                      </td>
                      <td
                        class="text-right text-lg text-green-600 bg-green-50/50 dark:bg-green-900/10"
                      >
                        {{ totalInvoiceAmount() | currency: 'USD' : 'symbol' : '1.2-2' }}
                      </td>
                    </tr>
                  </ng-template>
                </p-table>
              </div>
            }
          </div>

          <div footer class="flex justify-end gap-3">
            <p-button
              label="Close"
              icon="pi pi-times"
              [outlined]="true"
              severity="secondary"
              (onClick)="isModalOpen.set(false)"
            ></p-button>
            <p-button
              label="Print Report"
              icon="pi pi-print"
              styleClass="btn-show-details"
            ></p-button>
          </div>
        </app-full-screen-modal>

        <!-- Filtre Paneli -->
        <div class="card p-4">
          <div class="font-semibold text-xl mb-4">Purchase Orders Invoice</div>
          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-12 gap-6 items-end">
              <div class="md:col-span-4 flex flex-col gap-2">
                <label for="startDate" class="font-medium text-sm text-surface-600"
                  >Starting Date</label
                >
                <p-datepicker
                  id="startDate"
                  [(ngModel)]="startDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="01.01.2022"
                ></p-datepicker>
              </div>
              <div class="md:col-span-4 flex flex-col gap-2">
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
              <div class="md:col-span-4 flex flex-col gap-2">
                <label for="globalSearch" class="font-medium text-sm text-surface-600"
                  >Search</label
                >
                <p-iconfield>
                  <p-inputicon class="pi pi-search"></p-inputicon>
                  <input
                    id="globalSearch"
                    type="text"
                    pInputText
                    [(ngModel)]="searchQuery"
                    (keyup.enter)="loadData()"
                    placeholder="Search invoice or order ref..."
                    class="w-full"
                  />
                </p-iconfield>
              </div>
            </div>
            <div class="flex justify-end mt-4">
              <p-button
                label="Show Details"
                styleClass="px-8 btn-show-details"
                [loading]="isLoading()"
                (onClick)="loadData()"
              ></p-button>
            </div>
          </p-fluid>
        </div>

        <!-- Fatura Listesi Tablosu -->
        <div class="card p-0 overflow-hidden relative">
          @if (isLoading()) {
            <div
              class="absolute inset-0 flex items-center justify-center bg-white/50 z-10 transition-all"
            >
              <p-progressspinner styleClass="w-12 h-12" strokeWidth="4"></p-progressspinner>
            </div>
          }

          <p-table
            [value]="invoices()"
            [rows]="10"
            [paginator]="true"
            [lazy]="true"
            (onLazyLoad)="loadData($event)"
            [totalRecords]="totalRecords()"
            [rowsPerPageOptions]="[10, 20, 50]"
            styleClass="p-datatable-sm custom-invoice-table"
            [responsiveLayout]="'scroll'"
            paginatorDropdownAppendTo="body"
          >
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="invoiceDate">
                  Invoice Date <p-sortIcon field="invoiceDate"></p-sortIcon>
                </th>
                <th pSortableColumn="invoiceRefNum">
                  Invoice Ref Num <p-sortIcon field="invoiceRefNum"></p-sortIcon>
                </th>
                <th pSortableColumn="orderRefNo">
                  Order Ref No <p-sortIcon field="orderRefNo"></p-sortIcon>
                </th>
                <th class="text-right" pSortableColumn="invoiceQty">
                  Invoice QTY (PCS) <p-sortIcon field="invoiceQty"></p-sortIcon>
                </th>
                <th class="text-right" pSortableColumn="invoiceAmount">
                  Invoice Amount <p-sortIcon field="invoiceAmount"></p-sortIcon>
                </th>
                <th class="text-right" pSortableColumn="orderQty">
                  Order QTY (PCS) <p-sortIcon field="orderQty"></p-sortIcon>
                </th>
                <th class="text-right" pSortableColumn="orderAmount">
                  Order Amount <p-sortIcon field="orderAmount"></p-sortIcon>
                </th>
                <th class="text-right">Pending QTY (PCS)</th>
                <th class="text-right">Pending Amount</th>
                <th class="text-center">Detail</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-invoice>
              <tr>
                <td>{{ invoice.invoiceDate | date: 'd.MM.yyyy' }}</td>
                <td class="font-medium">{{ invoice.invoiceRefNum }}</td>
                <td class="text-surface-500">{{ invoice.orderRefNo }}</td>
                <td class="text-right">{{ invoice.invoiceQty | number: '1.0-3' }}</td>
                <td class="text-right font-medium">
                  {{ invoice.invoiceAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right">{{ invoice.orderQty | number: '1.0-3' }}</td>
                <td class="text-right">
                  {{ invoice.orderAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right" [ngClass]="{ 'text-red-500': invoice.pendingQty > 0 }">
                  {{ invoice.pendingQty | number: '1.0-3' }}
                </td>
                <td class="text-right" [ngClass]="{ 'text-red-500': invoice.pendingAmount < 0 }">
                  {{ invoice.pendingAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-center">
                  <p-button
                    icon="pi pi-search"
                    [text]="true"
                    [rounded]="true"
                    severity="danger"
                    (onClick)="onViewDetail(invoice)"
                    pTooltip="View Detail"
                  ></p-button>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="10" class="text-center p-8 text-surface-400">No invoices found.</td>
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
        /* Sayfa ve Kart Yapısı */
        .purchase-order-invoice-page {
          .card {
            background: var(--p-surface-card);
            border: 1px solid var(--p-border-color);
            border-radius: 12px;
            box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.05);
          }
        }

        /* Tablo Tasarımı */
        .custom-invoice-table {
          .p-datatable-thead > tr > th {
            background: #ffffff !important;
            color: #64748b !important;
            border-bottom: 1px solid #e2e8f0 !important;
            border-right: 1px solid #f1f5f9 !important;
            font-size: 13px !important;
            padding: 16px 12px !important;
            font-weight: 600;
            text-align: left;
          }

          .p-datatable-tbody > tr {
            background: var(--p-surface-card) !important;
            transition: background 0.2s;

            > td {
              border-bottom: 1px solid var(--p-border-color);
              border-right: 1px solid #f8fafc;
              padding: 14px 12px;
              font-size: 13px;
              vertical-align: middle;
            }

            &:hover {
              background: #f8fafc !important;
            }
          }
        }

        /* Dark Mode Uyumluluğu */
        .dark .custom-invoice-table {
          .p-datatable-thead > tr > th {
            background: #1e1e1e !important;
            color: #94a3b8 !important;
            border-bottom: 1px solid #334155 !important;
            border-right: 1px solid #334155 !important;
          }

          .p-datatable-tbody > tr {
            background: #1e1e1e !important;
            > td {
              border-bottom: 1px solid #334155 !important;
              border-right: 1px solid #334155 !important;
            }
            &:hover {
              background: #2d2d2d !important;
            }
          }
        }

        /* Özel Kırmızı Butonlar */
        .btn-show-details {
          background: #e31e24 !important;
          border: none !important;
          color: #ffffff !important;
          font-weight: 700 !important;
          padding: 12px 24px !important;
          border-radius: 6px !important;
          transition: all 0.2s;

          &:hover {
            background: #c1191f !important;
            transform: translateY(-1px);
            box-shadow: 0 4px 12px rgba(227, 30, 36, 0.25);
          }
        }

        /* Detail Modal Table Styles */
        .detail-modal-table {
          .header-group {
            background: #f8fafc !important;
            color: #475569 !important;
            font-weight: 800 !important;
            font-size: 14px !important;
            text-transform: uppercase;
            letter-spacing: 1px;
            padding: 1.25rem !important;
            border-bottom: 2px solid #e2e8f0 !important;
          }

          .sub-info-row {
            th {
              background: #ffffff !important;
              border-bottom: 1px solid #f1f5f9 !important;
              font-size: 13px;
            }
          }

          .column-headers {
            th {
              background: #f1f5f9 !important;
              color: #64748b !important;
              font-weight: 700 !important;
              font-size: 12px !important;
              text-transform: uppercase;
              padding: 1rem 0.75rem !important;
              border-bottom: 2px solid #cbd5e1 !important;
            }
          }

          .detail-row {
            td {
              padding: 1rem 0.75rem !important;
              border-bottom: 1px solid #f8fafc !important;
            }
            &:hover {
              background-color: #f8fafc !important;
            }
          }

          .totals-row {
            background: #f8fafc !important;
            td {
              padding: 1.25rem 0.75rem !important;
              border-top: 2px solid #cbd5e1 !important;
            }
          }

          .dark & {
            .header-group {
              background: #1e1e1e !important;
              color: #94a3b8 !important;
              border-color: #334155 !important;
            }
            .sub-info-row th {
              background: #121212 !important;
              border-color: #334155 !important;
            }
            .column-headers th {
              background: #1a1a1a !important;
              color: #94a3b8 !important;
              border-color: #334155 !important;
            }
            .detail-row {
              td {
                border-color: #334155 !important;
              }
              &:hover {
                background-color: #262626 !important;
              }
            }
            .totals-row {
              background: #1a1a1a !important;
              td {
                border-color: #334155 !important;
              }
            }
          }
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PurchaseOrderInvoice implements OnInit {
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);

  // Filtreler
  startDate = signal<Date | null>(null);
  endDate = signal<Date | null>(null);
  searchQuery = signal<string>('');
  isLoading = signal<boolean>(false);
  totalRecords = signal(0);

  // Veriler
  invoices = signal<PurchaseOrderInvoiceReportItem[]>([]);

  // Modal State
  private purchaseOrderService = inject(PurchaseOrderService);
  isModalOpen = signal(false);
  isModalLoading = signal(false);
  selectedInvoice = signal<PurchaseOrderInvoiceReportItem | null>(null);
  detailItems = signal<PurchaseOrderItemDto[]>([]);

  // Computed Totals for Modal
  totalOrderQty = computed(() => this.detailItems().reduce((acc, item) => acc + item.orderQty, 0));
  totalInvoiceQty = computed(() =>
    this.detailItems().reduce((acc, item) => acc + (item.receive || 0), 0),
  );
  totalInvoiceAmount = computed(() =>
    this.detailItems().reduce(
      (acc, item) => acc + (item.receive || 0) * (item.invoiceUnitPrice || 0),
      0,
    ),
  );

  ngOnInit() {
    // Sayfa açıldığında otomatik yükleme yapılmıyor.
  }

  /**
   * Backend'den verileri yükler
   */
  loadData(event?: any) {
    this.isLoading.set(true);

    const page = (event?.first || 0) / (event?.rows || 10) + 1;
    const pageSize = event?.rows || 10;
    const sortField = event?.sortField;
    const sortOrder = event?.sortOrder;

    const request = {
      startDate: this.startDate() || undefined,
      endDate: this.endDate() || undefined,
      searchQuery: this.searchQuery() || undefined,
      page,
      pageSize,
      sortField,
      sortOrder,
    };

    this.reportService
      .getPurchaseOrderInvoiceReport(request)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.invoices.set(res.data.data);
            this.totalRecords.set(res.data.totalCount);

            if (page === 1 && res.data.data.length === 0) {
              this.messageService.add({
                severity: 'warn',
                summary: 'Info',
                detail: 'No invoices found matching criteria.',
              });
            }
          }
        },
        error: (err) => {
          console.error('Data load error:', err);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while loading data.',
          });
        },
      });
  }

  /**
   * Detay görüntüleme
   */
  onViewDetail(invoice: PurchaseOrderInvoiceReportItem) {
    this.selectedInvoice.set(invoice);
    this.isModalOpen.set(true);
    this.isModalLoading.set(true);

    this.purchaseOrderService
      .getOrderDetails(invoice.id.toString())
      .pipe(finalize(() => this.isModalLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.detailItems.set(res.data.items);
          }
        },
        error: (err) => {
          console.error('Detail load error:', err);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while loading invoice details.',
          });
          this.isModalOpen.set(false);
        },
      });
  }
}
