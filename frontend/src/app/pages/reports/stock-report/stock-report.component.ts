import { Component, ChangeDetectionStrategy, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { FluidModule } from 'primeng/fluid';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService, MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { RippleModule } from 'primeng/ripple';
import { ReportService } from '../../../core/services/report.service';
import { StockReportItem, StockReportDetail } from '../../../core/models/stock-report.models';
import { effect } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-stock-report',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    TabsModule,
    TextareaModule,
    TabsModule,
    TextareaModule,
    FluidModule,
    FluidModule,
    CheckboxModule,
    MenuModule,
    RippleModule,
  ],
  template: `
    <div class="stock-report-page">
      <div class="card p-0 overflow-hidden">
        <!-- Header & Search -->
        <div class="p-4 flex flex-col gap-4 border-b border-surface-200 dark:border-surface-700">
          <div class="font-semibold text-xl">Stock Report</div>
          <p-fluid>
            <div class="flex flex-col md:flex-row items-center gap-6">
              <p-iconfield iconPosition="left" class="w-full md:w-80">
                <p-inputicon class="pi pi-search"></p-inputicon>
                <input
                  pInputText
                  type="text"
                  [(ngModel)]="searchValue"
                  placeholder="Search..."
                  class="w-full"
                />
              </p-iconfield>
              <div class="flex items-center">
                <p-button
                  label="Export"
                  icon="pi pi-download"
                  (onClick)="menu.toggle($event)"
                  [disabled]="!selectedBrand()"
                  severity="secondary"
                ></p-button>
                <p-menu #menu [model]="exportItems" [popup]="true"></p-menu>
              </div>
            </div>
          </p-fluid>
        </div>

        <!-- Marka Sekmeleri -->
        @if (brands().length > 0) {
          <p-tabs [value]="0">
            <p-tablist>
              @for (brand of brands(); track brand.id; let i = $index) {
                <p-tab [value]="i" (click)="selectedBrand.set(brand.name)">{{ brand.name }}</p-tab>
              }
            </p-tablist>
          </p-tabs>
        }

        <!-- Stok Tablosu -->
        <p-table
          [value]="filteredData()"
          [rows]="rows()"
          [paginator]="true"
          [lazy]="true"
          (onLazyLoad)="loadData($event)"
          [totalRecords]="totalRecords()"
          [rowsPerPageOptions]="[10, 20, 50]"
          styleClass="p-datatable-gridlines p-datatable-sm custom-stock-table"
          [responsiveLayout]="'scroll'"
          [showCurrentPageReport]="true"
          paginatorDropdownAppendTo="body"
          (onPage)="rows.set($event.rows)"
          [loading]="loading()"
          dataKey="itemCode"
        >
          <ng-template pTemplate="header">
            <tr>
              <th style="width: 3rem"></th>
              <th style="width: 100px">Picture</th>
              <th pSortableColumn="itemCode">
                <div class="flex items-center gap-2">
                  Item Code <p-sortIcon field="itemCode"></p-sortIcon>
                  <p-columnFilter type="text" field="itemCode" display="menu"></p-columnFilter>
                </div>
              </th>
              <th pSortableColumn="itemDescription">
                <div class="flex items-center gap-2">
                  Item Description <p-sortIcon field="itemDescription"></p-sortIcon>
                  <p-columnFilter
                    type="text"
                    field="itemDescription"
                    display="menu"
                  ></p-columnFilter>
                </div>
              </th>
              <th pSortableColumn="stock" class="text-center">
                Total Stock <p-sortIcon field="stock"></p-sortIcon>
              </th>
              <th pSortableColumn="reserved" class="text-center">
                Reserved <p-sortIcon field="reserved"></p-sortIcon>
              </th>
              <th pSortableColumn="available" class="text-center">
                Available <p-sortIcon field="available"></p-sortIcon>
              </th>
              <th pSortableColumn="incomingStock" class="text-center">
                Incoming Stock <p-sortIcon field="incomingStock"></p-sortIcon>
              </th>
              <th>Special Note</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-item let-expanded="expanded">
            <tr>
              <td>
                <p-button
                  type="button"
                  pRipple
                  [pRowToggler]="item"
                  [icon]="expanded ? 'pi pi-chevron-down' : 'pi pi-chevron-right'"
                  [text]="true"
                  [rounded]="true"
                  severity="secondary"
                />
              </td>
              <td class="text-center">
                <div class="image-container">
                  @if (item.picture) {
                    <div class="relative">
                      <img
                        [src]="item.picture"
                        [alt]="item.itemCode"
                        class="w-16 h-16 object-cover rounded border border-surface-200 dark:border-surface-700 cursor-zoom-in transition-transform duration-200 hover:scale-105"
                        #productImg
                        (mouseenter)="showPreview($event, item.picture, item.itemCode)"
                        (mousemove)="updatePreview($event)"
                        (mouseleave)="hidePreview()"
                        (error)="
                          productImg.style.display = 'none'; placeholder.style.display = 'flex'
                        "
                      />
                    </div>
                    <div
                      #placeholder
                      style="display: none"
                      class="w-16 h-16 bg-surface-100 dark:bg-surface-800 items-center justify-center rounded border border-surface-200 dark:border-surface-700"
                    >
                      <i class="pi pi-image text-surface-400 text-2xl"></i>
                    </div>
                  } @else {
                    <div
                      class="w-16 h-16 bg-surface-100 dark:bg-surface-800 flex items-center justify-center rounded border border-surface-200 dark:border-surface-700"
                    >
                      <i class="pi pi-image text-surface-400 text-2xl"></i>
                    </div>
                  }
                </div>
              </td>
              <td class="font-medium">{{ item.itemCode }}</td>
              <td>{{ item.itemDescription }}</td>
              <td class="text-center font-semibold text-surface-700">
                {{ item.stock | number }}
              </td>
              <td class="text-center text-orange-500">{{ item.reserved | number }}</td>
              <td
                class="text-center font-bold"
                [ngClass]="item.available < 0 ? 'text-red-500' : 'text-green-600'"
              >
                {{ item.available | number }}
              </td>
              <td class="text-center font-semibold text-blue-500">
                {{ item.incomingStock | number }}
              </td>
              <td>
                <div class="flex gap-2 items-start">
                  <textarea
                    pTextarea
                    [(ngModel)]="item.specialNote"
                    [autoResize]="true"
                    rows="2"
                    class="flex-1 text-sm p-2 border-surface-200"
                  ></textarea>
                  <p-button
                    label="Save"
                    severity="danger"
                    size="small"
                    (onClick)="onSaveNote(item)"
                  ></p-button>
                </div>
              </td>
            </tr>
          </ng-template>

          <ng-template pTemplate="expandedrow" let-item>
            <tr>
              <td colspan="9">
                <div
                  class="p-4 bg-surface-50 dark:bg-surface-900 border-x border-b border-surface-200 dark:border-surface-700"
                >
                  <div class="font-bold mb-3 flex items-center gap-2">
                    <i class="pi pi-info-circle text-primary"></i>
                    Stock Details (By Arrivals/Adjustments)
                  </div>
                  <div class="flex flex-col gap-4">
                    @for (group of movementGroups; track group) {
                      @if (getGroupDetails(item, group).length > 0) {
                        <div class="rounded border border-surface-200 dark:border-surface-700">
                          <div class="px-3 py-2 font-semibold bg-surface-100 dark:bg-surface-800">
                            {{ group }}
                          </div>
                          <p-table
                            [value]="getGroupDetails(item, group)"
                            styleClass="p-datatable-sm p-datatable-gridlines inner-table"
                          >
                            <ng-template pTemplate="header">
                              <tr>
                                <th>Vendor</th>
                                <th>Shelf Number</th>
                                <th>Pack List</th>
                                <th>Receive Date</th>
                                <th class="text-right">Price</th>
                                <th class="text-center">Quantity</th>
                              </tr>
                            </ng-template>
                            <ng-template pTemplate="body" let-detail>
                              <tr>
                                <td>{{ detail.supplierName || '-' }}</td>
                                <td>{{ detail.shelfNumber || '-' }}</td>
                                <td>{{ detail.packList || '-' }}</td>
                                <td>
                                  {{
                                    detail.occurredAt || detail.receiveDate
                                      ? (detail.occurredAt || detail.receiveDate
                                        | date: 'dd.MM.yyyy HH:mm')
                                      : '-'
                                  }}
                                </td>
                                <td class="text-right font-medium">
                                  {{ detail.price | currency: detail.currency || 'USD' }}
                                </td>
                                <td
                                  class="text-center font-bold"
                                  [ngClass]="
                                    detail.quantity < 0 ? 'text-red-500' : 'text-green-600'
                                  "
                                >
                                  {{ detail.quantity | number }}
                                </td>
                              </tr>
                            </ng-template>
                          </p-table>
                        </div>
                      }
                    }

                    @if (!item.details || item.details.length === 0) {
                      <div
                        class="text-center p-3 text-surface-400 border rounded border-surface-200 dark:border-surface-700"
                      >
                        No detailed records found.
                      </div>
                    }
                  </div>
                </div>
              </td>
            </tr>
          </ng-template>

          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="9" class="text-center p-8 text-surface-400">No stock items found.</td>
            </tr>
          </ng-template>
        </p-table>
      </div>

      <!-- Custom Image Preview Overlay -->
      @if (previewData(); as preview) {
        <div
          class="fixed z-9999 pointer-events-none flex flex-col items-center p-2 bg-white dark:bg-surface-900 border border-surface-200 dark:border-surface-700 rounded shadow-2xl"
          [style.left.px]="preview.x"
          [style.top.px]="preview.y"
        >
          <img
            [src]="preview.url"
            [alt]="preview.code"
            class="max-w-[500px] max-h-[500px] object-contain rounded bg-white"
          />
          <div class="mt-2 text-center font-bold text-surface-700 dark:text-surface-0 text-sm">
            {{ preview.code }}
          </div>
        </div>
      }
    </div>
  `,
  styles: [
    `
      :host ::ng-deep {
        .custom-stock-table {
          .p-datatable-thead > tr > th {
            background: #1e1e1e !important;
            color: #ffffff !important;
            border-color: #333 !important;
            font-size: 11px !important;
            padding: 10px 8px !important;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;

            .p-sortable-column-icon {
              font-size: 10px;
              color: rgba(255, 255, 255, 0.5) !important;
              margin-left: 0.5rem;
            }
          }

          .p-datatable-tbody > tr {
            > td {
              border-color: var(--p-surface-200);
              padding: 12px 8px;
              font-size: 13px;
              vertical-align: middle;
            }
          }
        }

        .dark .custom-stock-table {
          .p-datatable-thead > tr > th {
            background: #1e1e1e !important;
            color: #ffffff !important;
            border-color: #333 !important;
          }
          .p-datatable-tbody > tr > td {
            border-color: #333 !important;
          }
        }
      }

      .image-container {
        display: flex;
        justify-content: center;
        align-items: center;
      }

      .inner-table {
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
        border-radius: 4px;
        overflow: hidden;

        ::ng-deep .p-datatable-thead > tr > th {
          background-color: var(--p-surface-100) !important;
          color: var(--p-surface-700) !important;
          font-weight: 600 !important;
          text-transform: none !important;
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StockReport {
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);

  // Markalar listesi
  brands = signal<any[]>([]);
  selectedBrand = signal<string | null>(null);
  searchValue = signal('');
  rows = signal(10);
  loading = signal(false);
  totalRecords = signal(0);
  movementGroups: Array<'Adjustment' | 'Stock Details' | 'Status'> = [
    'Adjustment',
    'Stock Details',
    'Status',
  ];

  // Tüm veriler
  reportData = signal<StockReportItem[]>([]);

  exportItems: MenuItem[] = [
    {
      label: 'Excel Export',
      icon: 'pi pi-file-excel',
      command: () => {
        this.exportExcel();
      },
    },
  ];

  constructor() {
    this.loadBrands();

    // Marka veya arama değiştiğinde verileri yükle
    effect(() => {
      // Trigger reload when brand or search changes
      // We do this by calling loadData with default (first page) parameters essentially
      // However, lazy table handles initial load.
      // But if search changes, we need to reset to page 1.
      // Since p-table handles lazy load on init, we might not need to call loadData explicitly here
      // EXCEPT when filter criteria (brand/search) changes to force a reload from page 1.
      const brand = this.selectedBrand();
      const search = this.searchValue();

      // We can use a ViewChild to reset table, but since we are using signals and don't want to overcomplicate
      // we can rely on the user or the table's state.
      // Ideally, when brand or search changes, we should reset the table to page 1.
      // For now let's just trigger loadData({first:0, rows: this.rows()})

      if (brand) {
        // Ensure brand is selected
        this.loadData({ first: 0, rows: this.rows() });
      }
    });
  }

  // Markaları yükle
  loadBrands() {
    this.reportService.getBrands().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const brands = res.data;
          this.brands.set(brands);
          if (brands.length > 0 && !this.selectedBrand()) {
            this.selectedBrand.set(brands[0].name);
          }
        }
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'An error occurred while loading brands.',
        });
      },
    });
  }

  // Verileri yükle
  loadData(event?: any) {
    const brand = this.selectedBrand();
    if (!brand) return;

    this.loading.set(true);
    const page = (event?.first || 0) / (event?.rows || 10) + 1;
    const pageSize = event?.rows || 10;

    // Filters
    const filterItemCode = event?.filters?.itemCode?.[0]?.value;
    const filterItemDescription = event?.filters?.itemDescription?.[0]?.value;
    const filterShelfNumber = event?.filters?.shelfNumber?.[0]?.value;

    // Sort
    const sortField = event?.sortField;
    const sortOrder = event?.sortOrder;

    this.reportService
      .getStockReportData({
        brand: brand,
        searchValue: this.searchValue(),
        page: page,
        pageSize: pageSize,
        sortField: sortField,
        sortOrder: sortOrder,
        filterItemCode: filterItemCode,
        filterItemDescription: filterItemDescription,
        filterShelfNumber: filterShelfNumber,
      })
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.reportData.set(res.data.items);
            this.totalRecords.set(res.data.totalCount);
          }
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while loading stock data.',
          });
        },
      });
  }

  // Filtrelenmiş veri (Artık backend tarafında filtreleniyor ama client-side filtreleme de kalabilir)
  filteredData = computed(() => this.reportData());

  getGroupDetails(item: StockReportItem, group: string): StockReportDetail[] {
    const details = item.details || [];
    return details
      .filter((x) => x.movementGroup === group)
      .sort((a, b) => {
        const aDate = new Date(a.occurredAt || a.receiveDate || 0).getTime();
        const bDate = new Date(b.occurredAt || b.receiveDate || 0).getTime();
        return bDate - aDate;
      });
  }

  /** Preview data for custom tooltip */
  previewData = signal<{ url: string; code: string; x: number; y: number } | null>(null);

  showPreview(event: MouseEvent, url: string, code: string) {
    this.updatePreviewPosition(event, url, code);
  }

  hidePreview() {
    this.previewData.set(null);
  }

  updatePreview(event: MouseEvent) {
    const current = this.previewData();
    if (current) {
      this.updatePreviewPosition(event, current.url, current.code);
    }
  }

  private updatePreviewPosition(event: MouseEvent, url: string, code: string) {
    const offsetX = 20;
    const offsetY = 20;

    this.previewData.set({
      url,
      code,
      x: event.clientX + offsetX,
      y: event.clientY + offsetY,
    });
  }

  // Notu kaydet
  onSaveNote(item: StockReportItem) {
    this.reportService
      .updateStockNote({
        itemCode: item.itemCode,
        note: item.specialNote,
      })
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: `Special note saved for ${item.itemCode}.`,
            });
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while saving the note.',
          });
        },
      });
  }

  exportExcel() {
    const brand = this.selectedBrand();
    if (!brand) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a brand.',
      });
      return;
    }

    this.reportService.exportStockReport(brand, this.searchValue()).subscribe({
      next: (data) => {
        const blob = new Blob([data], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `StockReport_${brand}_${new Date().toISOString().slice(0, 10)}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Could not download the report.',
        });
      },
    });
  }
}
