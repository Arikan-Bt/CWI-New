import {
  Component,
  ChangeDetectionStrategy,
  signal,
  inject,
  computed,
  OnInit,
} from '@angular/core';
import * as XLSX from 'xlsx';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CheckboxModule } from 'primeng/checkbox';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { BadgeModule } from 'primeng/badge';
import { ToastModule } from 'primeng/toast';
import { TabsModule } from 'primeng/tabs';
import { MessageService } from 'primeng/api';
import { FluidModule } from 'primeng/fluid';
import { PopoverModule } from 'primeng/popover';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { FullScreenModalComponent } from '../../../shared/components/full-screen-modal/full-screen-modal.component';
import { OrderReportPrintComponent } from './print/order-report-print.component';
import { ReportService } from '../../../core/services/report.service';
import {
  OrderReportItem,
  OrderDetailItem,
  OrdersReportRequest,
  OrderDetailRequest,
} from '../../../core/models/orders-report.models';
import {
  ItemOrderCheckRequest,
  ItemOrderCheckResponse,
} from '../../../core/models/item-order-check.models';
import { getOrderStatusSeverity } from '../../../core/utils/status-utils';

@Component({
  selector: 'app-orders-report',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    DatePickerModule,
    ButtonModule,
    TableModule,
    CheckboxModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    BadgeModule,
    ToastModule,
    TabsModule,
    FluidModule,
    FullScreenModalComponent,
    OrderReportPrintComponent,
    TagModule,
  ],

  template: `
    <div class="orders-report-page">
      <div class="flex flex-col gap-4">
        <!-- Üst Panel: Filtreler -->
        <div class="card">
          <div class="font-semibold text-xl mb-4">Orders Report</div>
          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <!-- Current Account -->
              <div class="flex flex-col gap-2">
                <label for="currentAccount" class="font-medium text-sm text-surface-600"
                  >Current Account</label
                >
                <p-select
                  id="currentAccount"
                  [options]="accountOptions()"
                  [(ngModel)]="filters().currentAccountCode"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Load All"
                  [filter]="true"
                ></p-select>
              </div>

              <!-- Order Status -->
              <div class="flex flex-col gap-2">
                <label for="orderStatus" class="font-medium text-sm text-surface-600"
                  >Order Status</label
                >
                <p-select
                  id="orderStatus"
                  [options]="statusOptions()"
                  [(ngModel)]="filters().orderStatus"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Load All"
                ></p-select>
              </div>

              <!-- Report Type / Checkbox -->
              <div class="flex flex-col gap-2">
                <label class="font-medium text-sm text-surface-600">Report Type</label>
                <div class="flex items-center gap-2 h-full">
                  <p-checkbox
                    [(ngModel)]="filters().displayProductPhoto"
                    [binary]="true"
                    inputId="displayPhoto"
                  ></p-checkbox>
                  <label for="displayPhoto" class="text-sm">Product Photo</label>
                </div>
              </div>

              <!-- Start of Day -->
              <div class="flex flex-col gap-2">
                <label for="startDate" class="font-medium text-sm text-surface-600"
                  >Start of Day</label
                >
                <p-datepicker
                  id="startDate"
                  [(ngModel)]="filters().startDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="01.01.2025"
                ></p-datepicker>
              </div>

              <!-- End of Day -->
              <div class="flex flex-col gap-2">
                <label for="endDate" class="font-medium text-sm text-surface-600">End of Day</label>
                <p-datepicker
                  id="endDate"
                  [(ngModel)]="filters().endDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="06.01.2025"
                ></p-datepicker>
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

            <div class="flex justify-end mt-6">
              <p-button
                label="Report Orders"
                icon="pi pi-search"
                severity="danger"
                (onClick)="onReport()"
                [loading]="loading()"
                styleClass="w-full md:w-auto"
              ></p-button>
            </div>
          </p-fluid>
        </div>

        <!-- Alt Panel: Sonuçlar -->
        <div class="card p-0 overflow-hidden border-surface-200 dark:border-surface-700">
          <div
            class="p-4 bg-surface-0 dark:bg-surface-900 border-b border-surface-200 dark:border-surface-700"
          >
            <div class="flex justify-between items-center flex-wrap gap-4">
              <div class="font-semibold text-lg">Report Results</div>
              <div class="flex items-center gap-3">
                <p-iconfield iconPosition="left">
                  <p-inputicon class="pi pi-search text-xs"></p-inputicon>
                  <input
                    pInputText
                    type="text"
                    placeholder="Search..."
                    (input)="dt.filterGlobal($any($event.target).value, 'contains')"
                    class="w-full md:w-64 h-8 text-xs bg-surface-50 dark:bg-surface-800 border-surface-200 dark:border-surface-700"
                  />
                </p-iconfield>
              </div>
            </div>
          </div>

          <!-- Marka Sekmeleri -->
          <p-tabs [value]="0">
            <p-tablist>
              @for (brand of brands(); track brand; let i = $index) {
                <p-tab [value]="i" (click)="selectedBrand.set(brand)">{{ brand }}</p-tab>
              }
            </p-tablist>
          </p-tabs>

          <p-table
            #dt
            [value]="filteredReportData()"
            [rows]="rows()"
            [paginator]="true"
            [lazy]="true"
            [totalRecords]="totalRecords()"
            [first]="first()"
            (onLazyLoad)="onLazyLoad($event)"
            styleClass="p-datatable-gridlines p-datatable-sm custom-report-table"
            [responsiveLayout]="'scroll'"
            [globalFilterFields]="[
              'currentAccountCode',
              'currentAccountDescription',
              'orderDetails',
              'brand',
            ]"
            [rowsPerPageOptions]="[10, 20, 50]"
            [showCurrentPageReport]="true"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
            paginatorDropdownAppendTo="body"
          >
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="currentAccountCode">
                  <div class="flex items-center gap-2">
                    CURRENT ACCOUNT CODE <p-sortIcon field="currentAccountCode"></p-sortIcon>
                    <p-columnFilter
                      type="text"
                      field="currentAccountCode"
                      display="menu"
                    ></p-columnFilter>
                  </div>
                </th>
                <th pSortableColumn="currentAccountDescription">
                  <div class="flex items-center gap-2">
                    CURRENT ACCOUNT DESCRIPTION
                    <p-sortIcon field="currentAccountDescription"></p-sortIcon>
                    <p-columnFilter
                      type="text"
                      field="currentAccountDescription"
                      display="menu"
                    ></p-columnFilter>
                  </div>
                </th>
                <th pSortableColumn="orderDetails">
                  <div class="flex items-center gap-2">
                    ORDER NO <p-sortIcon field="orderDetails"></p-sortIcon>
                    <p-columnFilter
                      type="text"
                      field="orderDetails"
                      display="menu"
                    ></p-columnFilter>
                  </div>
                </th>
                <th pSortableColumn="status" class="text-center">
                  <div class="flex items-center gap-2 justify-center">
                    STATUS <p-sortIcon field="status"></p-sortIcon>
                    <p-columnFilter type="text" field="status" display="menu"></p-columnFilter>
                  </div>
                </th>
                <th pSortableColumn="brand">
                  <div class="flex items-center gap-2">
                    BRAND <p-sortIcon field="brand"></p-sortIcon>
                    <p-columnFilter type="text" field="brand" display="menu"></p-columnFilter>
                  </div>
                </th>
                <th pSortableColumn="orderDate">
                  <div class="flex items-center gap-2">
                    ORDER DATE <p-sortIcon field="orderDate"></p-sortIcon>
                    <p-columnFilter type="date" field="orderDate" display="menu"></p-columnFilter>
                  </div>
                </th>
                <th pSortableColumn="requestedShipmentDate">
                  <div class="flex items-center gap-2">
                    REQUESTED SHIPMENT DATE <p-sortIcon field="requestedShipmentDate"></p-sortIcon>
                    <p-columnFilter
                      type="date"
                      field="requestedShipmentDate"
                      display="menu"
                    ></p-columnFilter>
                  </div>
                </th>
                <th pSortableColumn="totalQty" class="text-right">
                  TOTAL QTY <p-sortIcon field="totalQty"></p-sortIcon>
                </th>
                <th pSortableColumn="discount" class="text-right">
                  DISCOUNT <p-sortIcon field="discount"></p-sortIcon>
                </th>
                <th pSortableColumn="total" class="text-right">
                  TOTAL <p-sortIcon field="total"></p-sortIcon>
                </th>
                <th class="text-center">DETAIL</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-item>
              <tr class="dark:bg-surface-900 dark:text-surface-0">
                <td>{{ item.currentAccountCode }}</td>
                <td>{{ item.currentAccountDescription }}</td>
                <td>{{ item.orderDetails }}</td>
                <td class="text-center">
                  <p-tag [value]="item.status" [severity]="getSeverity(item.status)"></p-tag>
                </td>
                <td>{{ item.brand }}</td>
                <td>{{ item.orderDate | date: 'dd.MM.yyyy' }}</td>
                <td>{{ item.requestedShipmentDate | date: 'dd.MM.yyyy' }}</td>
                <td class="text-right font-semibold">{{ item.totalQty | number }}</td>
                <td class="text-right">
                  {{ item.discount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right font-bold">
                  {{ item.total | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-center">
                  <p-button
                    icon="pi pi-search"
                    severity="danger"
                    [rounded]="true"
                    [text]="true"
                    size="small"
                    (onClick)="onViewDetail(item)"
                  ></p-button>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="footer">
              <tr class="font-bold bg-surface-900 text-white dark:bg-black">
                <td colspan="7" class="text-left py-3">Total (Current Page)</td>
                <td class="text-right">{{ calculatePageTotals(dt).qty | number }}</td>
                <td class="text-right">0 $</td>
                <td class="text-right">
                  {{ calculatePageTotals(dt).amount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td></td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr class="dark:bg-surface-900">
                <td colspan="10" class="text-center p-8 text-surface-400 font-medium">
                  No data found. Please use filters to generate the report.
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>

      <!-- Detay Modalı -->
      <app-full-screen-modal [(visible)]="showDetail">
        <div header class="flex items-center justify-between w-full">
          <div class="text-xl font-bold text-surface-900 dark:text-surface-0">ORDER DETAILS</div>
        </div>

        <div class="flex flex-col bg-white dark:bg-surface-950 p-6">
          <!-- Görseldeki Header Tasarımı -->
          <div
            class="border border-surface-200 dark:border-surface-700 rounded-sm overflow-hidden mb-6"
          >
            <div
              class="flex justify-between items-center p-3 border-b border-surface-200 dark:border-surface-700 bg-surface-50 dark:bg-surface-900"
            >
              <div
                class="flex-1 text-center font-bold text-surface-600 dark:text-surface-400 uppercase tracking-wider"
              >
                Sales Orders
              </div>
              <div class="flex items-center gap-2">
                <div class="text-surface-600 dark:text-surface-400 font-bold text-sm">
                  {{ selectedOrder()?.orderDetails }}
                </div>
                @if (selectedOrder()?.status; as status) {
                  <p-tag [value]="status" [severity]="getSeverity(status)"></p-tag>
                }
              </div>
            </div>
            <div class="flex justify-between items-center p-4">
              <div class="text-xl font-bold text-surface-700 dark:text-surface-200">
                {{ selectedOrder()?.currentAccountDescription }}
              </div>
              <div class="flex items-center gap-4">
                <p-iconfield iconPosition="left" class="print:hidden">
                  <p-inputicon class="pi pi-search text-xs"></p-inputicon>
                  <input
                    pInputText
                    type="text"
                    placeholder="Search Product..."
                    (input)="dtDetail.filterGlobal($any($event.target).value, 'contains')"
                    class="w-full md:w-64 h-8 text-xs bg-surface-50 dark:bg-surface-800 border-surface-200 dark:border-surface-700"
                  />
                </p-iconfield>
                <div class="text-sm font-medium text-surface-600 dark:text-surface-400">
                  Order Date : {{ selectedOrder()?.orderDate | date: 'dd.MM.yyyy' }}
                </div>
              </div>
            </div>

            <!-- Detay Tablosu -->
            <p-table
              #dtDetail
              [value]="detailData()"
              [rows]="detailRows()"
              [paginator]="true"
              styleClass="p-datatable-gridlines p-datatable-sm detail-table"
              [responsiveLayout]="'scroll'"
              [globalFilterFields]="['productCode', 'productName']"
              [showCurrentPageReport]="true"
              currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
              [rowsPerPageOptions]="[10, 20, 50]"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
              (onPage)="detailRows.set($event.rows)"
            >
              <ng-template pTemplate="header">
                <tr class="bg-surface-50 dark:bg-surface-900">
                  @if (filters().displayProductPhoto) {
                    <th class="w-20 text-center text-surface-600 dark:text-surface-400">Picture</th>
                  }
                  <th
                    pSortableColumn="productCode"
                    class="w-48 text-surface-600 dark:text-surface-400"
                  >
                    Product Code <p-sortIcon field="productCode"></p-sortIcon>
                  </th>
                  <th pSortableColumn="productName" class="text-surface-600 dark:text-surface-400">
                    Product Name <p-sortIcon field="productName"></p-sortIcon>
                  </th>
                  <th
                    pSortableColumn="qty"
                    class="w-32 text-right text-surface-600 dark:text-surface-400"
                  >
                    QTY (PCS) <p-sortIcon field="qty"></p-sortIcon>
                  </th>
                  <th
                    pSortableColumn="amount"
                    class="w-32 text-right text-surface-600 dark:text-surface-400"
                  >
                    Amount <p-sortIcon field="amount"></p-sortIcon>
                  </th>
                  <th
                    pSortableColumn="total"
                    class="w-32 text-right text-surface-600 dark:text-surface-400"
                  >
                    Total <p-sortIcon field="total"></p-sortIcon>
                  </th>
                </tr>
              </ng-template>
              <ng-template pTemplate="body" let-detail>
                <tr class="text-surface-700 dark:text-surface-300">
                  @if (filters().displayProductPhoto) {
                    <td class="text-center">
                      <div class="flex justify-center items-center">
                        @if (detail.picture) {
                          <div class="relative">
                            <img
                              [src]="detail.picture"
                              [alt]="detail.productCode"
                              class="w-16 h-16 object-cover rounded border border-surface-200 dark:border-surface-700 cursor-zoom-in transition-transform duration-200 hover:scale-105"
                              #productImg
                              (mouseenter)="showPreview($event, detail.picture, detail.productCode)"
                              (mousemove)="updatePreview($event)"
                              (mouseleave)="hidePreview()"
                              (error)="
                                productImg.style.display = 'none';
                                placeholder.style.display = 'flex'
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
                  }
                  <td class="font-medium">{{ detail.productCode }}</td>
                  <td>{{ detail.productName }}</td>
                  <td class="text-right">{{ detail.qty | number }}</td>
                  <td class="text-right">
                    {{ detail.amount | currency: 'USD' : 'symbol' : '1.2-2' }}
                  </td>
                  <td class="text-right font-semibold">
                    {{ detail.total | currency: 'USD' : 'symbol' : '1.2-2' }}
                  </td>
                </tr>
              </ng-template>
            </p-table>

            <!-- Alt Bilgi / Footer Alanı -->
            <div
              class="flex flex-col md:flex-row border-t border-surface-200 dark:border-surface-700"
            >
              <!-- Sol Taraf: Açıklama ve Adres -->
              <div
                class="flex-1 p-6 border-r border-surface-200 dark:border-surface-700 flex flex-col gap-4"
              >
                <div>
                  <div class="text-lg font-bold text-surface-600 dark:text-surface-400 mb-2">
                    Order Description
                  </div>
                  <div class="text-sm text-surface-500 min-h-6"></div>
                </div>
                <div class="flex gap-2 text-sm">
                  <span class="font-bold text-surface-700 dark:text-surface-200">Address :</span>
                  <span class="text-surface-600 dark:text-surface-400"
                    >Av. 14 Janvier Slim Center Khezama Est</span
                  >
                </div>
                <div class="flex gap-2 text-sm">
                  <span class="font-bold text-surface-700 dark:text-surface-200"
                    >Payment Type :</span
                  >
                  <span class="text-surface-600 dark:text-surface-400"></span>
                </div>
              </div>

              <!-- Sağ Taraf: Toplamlar -->
              <div class="w-full md:w-80 p-0">
                <div class="flex flex-col">
                  <div
                    class="flex justify-between items-center p-3 border-b border-surface-200 dark:border-surface-700"
                  >
                    <span class="text-sm font-semibold text-surface-600 dark:text-surface-400"
                      >Total Qty</span
                    >
                    <span class="text-sm font-bold text-surface-800 dark:text-surface-100">{{
                      selectedOrder()?.totalQty | number
                    }}</span>
                  </div>
                  <div
                    class="flex justify-between items-center p-3 border-b border-surface-200 dark:border-surface-700"
                  >
                    <span class="text-sm font-semibold text-surface-600 dark:text-surface-400"
                      >Grand Total</span
                    >
                    <div class="flex items-center gap-1">
                      <span class="text-sm font-bold text-surface-800 dark:text-surface-100">{{
                        selectedOrder()?.total | currency: 'USD' : 'symbol' : '1.2-2'
                      }}</span>
                      <i class="pi pi-dollar text-xs text-surface-500"></i>
                    </div>
                  </div>
                  <div
                    class="flex justify-between items-center p-3 border-b border-surface-200 dark:border-surface-700"
                  >
                    <span class="text-sm font-semibold text-surface-600 dark:text-surface-400"
                      >Discount</span
                    >
                    <div class="flex items-center gap-1">
                      <span class="text-sm font-bold text-surface-800 dark:text-surface-100"
                        >0,00</span
                      >
                      <i class="pi pi-dollar text-xs text-surface-500"></i>
                    </div>
                  </div>
                  <div
                    class="flex justify-between items-center p-3 bg-surface-50 dark:bg-surface-900 border-b border-surface-200 dark:border-surface-700"
                  >
                    <span class="text-sm font-bold text-surface-900 dark:text-surface-0"
                      >Total Amount</span
                    >
                    <div class="flex items-center gap-1">
                      <span class="text-lg font-black text-surface-900 dark:text-surface-0">{{
                        selectedOrder()?.total | currency: 'USD' : 'symbol' : '1.2-2'
                      }}</span>
                      <i
                        class="pi pi-dollar text-sm text-surface-900 dark:text-surface-0 font-bold"
                      ></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div footer class="flex justify-end gap-2 print:hidden">
          <p-button
            label="Close"
            icon="pi pi-times"
            severity="secondary"
            [text]="true"
            (onClick)="showDetail.set(false)"
          ></p-button>
          <p-button
            label="Print Report"
            icon="pi pi-print"
            severity="danger"
            (onClick)="onPrint()"
          ></p-button>
          <p-button
            label="Excel Report"
            icon="pi pi-file-excel"
            severity="success"
            (onClick)="onExportExcel()"
          ></p-button>
        </div>
      </app-full-screen-modal>

      <!-- MANUAL SCALABLE PRINT COMPONENT -->
      <app-order-report-print
        id="print-section"
        class="print-only hidden"
        [order]="selectedOrder()"
        [details]="detailData()"
        [showPhotos]="filters().displayProductPhoto"
      ></app-order-report-print>
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
    <!-- End of orders-report-page -->
  `,
  styles: [
    `
      :host ::ng-deep {
        .p-datepicker {
          width: 100%;
        }

        .custom-report-table {
          .p-datatable-thead > tr > th {
            background: #1e1e1e !important;
            color: #ffffff !important;
            border-color: #333 !important;
            font-size: 10px !important;
            padding: 10px 8px !important;
            white-space: nowrap;
            font-weight: 700;
            letter-spacing: 0.5px;
            text-transform: uppercase;

            .p-sortable-column-icon {
              color: rgba(255, 255, 255, 0.5) !important;
              font-size: 10px;
            }
          }

          .p-datatable-tbody > tr {
            background-color: transparent;
            color: inherit;

            > td {
              border-color: var(--p-surface-200);
              padding: 8px;
              font-size: 12px;
            }
          }

          .p-datatable-tfoot > tr > td {
            background: #1e1e1e !important;
            color: #ffffff !important;
            border-color: #333 !important;
            padding: 12px 8px !important;
            font-size: 12px;
          }
        }

        .dark .custom-report-table {
          .p-datatable-tbody > tr > td {
            border-color: #333 !important;
          }
        }
        .p-badge {
          padding: 0.25rem 0.75rem;
          height: auto;
          font-size: 0.75rem;
        }

        .detail-table {
          .p-datatable-thead > tr > th {
            background: #f8fafc !important;
            color: #64748b !important;
            border-color: var(--p-surface-200) !important;
            font-size: 11px !important;
            padding: 12px 16px !important;
            font-weight: 700;
          }

          .p-datatable-tbody > tr > td {
            border-color: var(--p-surface-200) !important;
            padding: 12px 16px !important;
            font-size: 13px;
          }
        }

        .dark .detail-table {
          .p-datatable-thead > tr > th {
            background: #1e1e1e !important;
            color: #94a3b8 !important;
            border-color: #333 !important;
          }
          .p-datatable-tbody > tr > td {
            border-color: #333 !important;
          }
        }

        .print-only {
          display: none;
        }

        @media print {
          /* Reset page */
          @page {
            size: A4;
            margin: 10mm;
          }

          body * {
            visibility: hidden;
          }

          #print-section,
          #print-section * {
            visibility: visible;
          }

          #print-section {
            display: block !important;
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
          }

          /* Ensure reset of styles for print section */
          .print-only {
            display: block !important;
          }

          /* Hide other specific elements just in case */
          app-full-screen-modal,
          .orders-report-page {
            display: none !important;
          }
          /* Re-enable display for parents of print-section if needed */
          .orders-report-page {
            display: block !important;
            visibility: hidden;
          }
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersReport implements OnInit {
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);

  loading = signal(false);
  rows = signal(10);
  searchValue = signal('');

  // Filtreleme verileri
  filters = signal<OrdersReportRequest>({
    currentAccountCode: undefined,
    orderStatus: undefined,
    displayProductPhoto: false,
    startDate: new Date(2025, 0, 1).toISOString(),
    endDate: new Date(2025, 0, 6).toISOString(),
    page: 1,
    pageSize: 10,
  });

  // Server-side pagination için toplam kayıt sayısı
  totalRecords = signal(0);
  first = signal(0);

  // Seçenekler
  accountOptions = signal<{ label: string; value: string }[]>([]);
  statusOptions = signal<{ label: string; value: string }[]>([]);

  ngOnInit() {
    this.loadLookups();
  }

  loadLookups() {
    this.reportService.getCustomers().subscribe((res) => {
      if (res.success && res.data) {
        this.accountOptions.set(res.data);
        // Eğer tek bir seçenek varsa (Vendor kullanıcıları için), otomatik seç
        if (res.data.length === 1) {
          this.filters.update((f) => ({ ...f, currentAccountCode: res.data![0].value }));
        }
      }
    });
    this.reportService.getOrderStatuses().subscribe((res) => {
      if (res.success && res.data) {
        this.statusOptions.set(res.data);
      }
    });
  }

  // Rapor verisi
  reportData = signal<OrderReportItem[]>([]);

  // Markalar listesi
  brands = signal<string[]>([]);
  selectedBrand = signal('');

  // Filtrelenmiş rapor verisi
  filteredReportData = computed(() => {
    const brand = this.selectedBrand();
    return this.reportData().filter((item) => item.brand === brand);
  });

  // Detay verisi
  showDetail = signal(false);
  detailRows = signal(10);
  selectedOrder = signal<OrderReportItem | null>(null);
  detailData = signal<OrderDetailItem[]>([]);

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
      startDate: start as any,
      endDate: end as any,
    }));

    // Tarihler set edildikten hemen sonra raporu tetikle
    this.onReport();
  }

  // Raporu getir
  onReport() {
    this.loading.set(true);
    // Sayfa sıfırla
    this.first.set(0);
    this.filters.update((f) => ({ ...f, page: 1 }));

    this.fetchReport();
  }

  // Lazy loading event handler - sayfa değişikliğinde çağrılır
  onLazyLoad(event: any) {
    // İlk yüklemede veya veri yoksa işlem yapma
    if (this.reportData().length === 0 && this.totalRecords() === 0) {
      return;
    }

    const page = Math.floor(event.first / event.rows) + 1;
    const sortOrder = event.sortOrder === -1 ? -1 : 1;
    this.first.set(event.first);
    this.rows.set(event.rows);
    this.filters.update((f) => ({
      ...f,
      page,
      pageSize: event.rows,
      sortField: event.sortField || undefined,
      sortOrder,
    }));

    this.fetchReport();
  }

  // Backend'den veri çekme
  private fetchReport() {
    this.loading.set(true);

    const currentFilters = this.filters();
    const request: OrdersReportRequest = {
      ...currentFilters,
      startDate: currentFilters.startDate
        ? new Date(currentFilters.startDate).toISOString()
        : (null as any),
      endDate: currentFilters.endDate
        ? new Date(currentFilters.endDate).toISOString()
        : (null as any),
    };

    this.reportService.getOrdersReport(request).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.reportData.set(res.data.data);
          this.brands.set(res.data.brands);
          // Server-side pagination için toplam kayıt sayısını güncelle
          this.totalRecords.set(res.data.totalCount ?? 0);

          // Eğer mevcut seçili marka yeni listede yoksa ilkini seç
          if (res.data.brands.length > 0 && !res.data.brands.includes(this.selectedBrand())) {
            this.selectedBrand.set(res.data.brands[0]);
          }

          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Report generated successfully.',
          });
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'Failed to generate report.',
          });
        }
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to generate report.',
        });
      },
    });
  }

  /**
   * Sayfada görünen (filtrelenmiş ve sayfalanmış) verilerin toplamını hesaplar.
   * @param table PrimeNG Table referansı
   */
  calculatePageTotals(table: any) {
    if (!table || !table.value) return { qty: 0, amount: 0 };

    // Eğer paginator varsa, o anki sayfanın verilerini al (first ve rows kullanarak)
    const data = table.filteredValue || table.value;
    const first = table.first || 0;
    const rows = table.rows || 0;

    const pageData = data.slice(first, first + rows);

    return {
      qty: pageData.reduce((acc: number, curr: any) => acc + (curr.totalQty || 0), 0),
      amount: pageData.reduce((acc: number, curr: any) => acc + (curr.total || 0), 0),
    };
  }

  // Detay görüntüle (Sipariş Detayları)
  onViewDetail(item: OrderReportItem) {
    this.selectedOrder.set(item);

    const request: OrderDetailRequest = {
      orderId: item.orderId,
      brand: item.brand,
      pageNumber: 1,
      pageSize: 1000, // Bu sayfa henüz server-side pagination'a geçmediği için tümünü çekiyoruz
    };

    this.reportService.getOrderDetails(request).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.detailData.set(res.data.data);
          this.showDetail.set(true);
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'Failed to load order details.',
          });
        }
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load order details.',
        });
      },
    });
  }

  today = new Date();

  /** Preview data for custom tooltip */
  previewData = signal<{ url: string; code: string; x: number; y: number } | null>(null);

  constructor() {}

  getSeverity(
    status: string,
  ): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | undefined {
    return getOrderStatusSeverity(status);
  }

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
    // Ekranın sağına taşma kontrolü yapılabilir ama şimdilik basit tutuyoruz
    const offsetX = 20;
    const offsetY = 20;

    this.previewData.set({
      url,
      code,
      x: event.clientX + offsetX,
      y: event.clientY + offsetY,
    });
  }

  // Raporu yazdır
  onPrint() {
    const printContent = document.getElementById('print-section');

    if (!printContent) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Content to print not found.',
      });
      return;
    }

    const iframe = document.createElement('iframe');
    iframe.style.position = 'absolute';
    iframe.style.width = '0px';
    iframe.style.height = '0px';
    iframe.style.border = 'none';

    document.body.appendChild(iframe);

    const doc = iframe.contentWindow?.document;

    if (doc) {
      // 1. Base URL'i al (Göreceli kaynakların çalışması için kritik)
      const baseUrl = document.location.origin + document.location.pathname;

      // 2. Stil dosyalarını (<link rel="stylesheet">) mutlak yollarla kopyala
      const links = Array.from(document.head.querySelectorAll('link[rel="stylesheet"]'))
        .map((link) => {
          const newLink = link.cloneNode(true) as HTMLLinkElement;
          // href özelliğini mutlak yola çevir (DOM property her zaman tam URL döner)
          if (link instanceof HTMLLinkElement && link.href) {
            newLink.href = link.href;
          }
          return newLink.outerHTML;
        })
        .join('');

      // 3. Inline stilleri (<style>) kopyala
      const styles = Array.from(document.head.querySelectorAll('style'))
        .map((style) => style.outerHTML)
        .join('');

      doc.open();
      doc.write(`
        <!DOCTYPE html>
        <html>
          <head>
            <base href="${baseUrl}">
            <title>Order Report</title>
            <meta charset="utf-8">
            ${links}
            ${styles}
            <style>
              /* Yazdırma ayarları */
              @page { size: A4; margin: 10mm; }
              body { 
                margin: 0; 
                padding: 0; 
                background-color: white; 
                -webkit-print-color-adjust: exact; 
                print-color-adjust: exact;
                font-family: 'Inter', sans-serif; /* Varsayılan font */
              }
              /* Bileşeni görünür yap ve blok olarak ayarla */
              .print-only { display: block !important; }
              app-order-report-print { display: block; width: 100%; }
              
              /* Tailwind'in bazı resetlerini zorla */
              *, ::before, ::after {
                box-sizing: border-box;
                border-width: 0;
                border-style: solid;
                border-color: #e5e7eb;
              }
            </style>
          </head>
          <body>
            ${printContent.outerHTML}
          </body>
        </html>
      `);
      doc.close();

      // İçerik ve stiller yüklendiğinde yazdır
      iframe.onload = () => {
        // Stillerin parse edilmesi ve render edilmesi için güvenli bir bekleme süresi
        setTimeout(() => {
          iframe.contentWindow?.focus();
          iframe.contentWindow?.print();

          // Yazdırma diyaloğu kapandıktan sonra temizle
          setTimeout(() => {
            if (document.body.contains(iframe)) {
              document.body.removeChild(iframe);
            }
          }, 2000);
        }, 1000); // 1 saniye bekle (stilller için)
      };
    }
  }

  // Excel Raporu
  onExportExcel() {
    const order = this.selectedOrder();
    const details = this.detailData();

    if (!order || !details) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'No data available to export.',
      });
      return;
    }

    // 1. Veri Hazırlığı (AOA - Array of Arrays)
    const data: any[][] = [];

    // Başlık Bölümü
    data.push(['SALES ORDERS', '', '', '', '', order.orderDetails || '']);
    data.push([
      order.currentAccountDescription || '',
      '',
      '',
      '',
      '',
      `Date: ${new Date(order.orderDate).toLocaleDateString('tr-TR')}`,
    ]);
    data.push([]); // Boş satır

    // Tablo Başlıkları
    const headers = ['Product Code', 'Product Name', 'QTY (PCS)', 'Amount', 'Total'];
    data.push(headers);

    // Tablo Verileri
    details.forEach((item) => {
      data.push([
        item.productCode,
        item.productName,
        item.qty,
        this.formatCurrency(item.amount),
        this.formatCurrency(item.total),
      ]);
    });

    data.push([]); // Boş satır

    // Footer Bölümü
    // Footer sol taraf ve sağ tarafı aynı satırlara denk getirmek için
    const footerStartRow = data.length;

    // Satır 1: Order Description & Total Qty
    data.push(['Order Description', '', '', 'Total Qty', order.totalQty]);

    // Satır 2: Address & Grand Total
    data.push([
      'Address : Av. 14 Janvier Slim Center Khezama Est',
      '',
      '',
      'Grand Total',
      this.formatCurrency(order.total) + ' $',
    ]);

    // Satır 3: Payment Type & Discount
    data.push(['Payment Type :', '', '', 'Discount', '0,00 $']);

    // Satır 4: Total Amount
    data.push(['', '', '', 'Total Amount', this.formatCurrency(order.total) + ' $']);

    // 2. Worksheet Oluşturma
    const ws: XLSX.WorkSheet = XLSX.utils.aoa_to_sheet(data);

    // Sütun Genişlikleri Ayarlama
    ws['!cols'] = [
      { wch: 20 }, // Product Code
      { wch: 40 }, // Product Name
      { wch: 15 }, // Qty
      { wch: 15 }, // Amount
      { wch: 15 }, // Total
      { wch: 20 }, // Extra column if needed
    ];

    // Stil Ayarları (Basit stil ayarları - XLSX Community sürümü kısıtlı stil desteğine sahiptir, pro sürüm gerekir.
    // Ancak temel hücre birleştirmeleri yapılabilir)

    // Başlık birleştirmeleri
    // SALES ORDERS
    // A1 hücresi
    // orderDetails F1 hücresi (Zaten orada)

    // 3. Workbook ve İndirme
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Order Report');

    // Dosya isimlendirme
    const fileName = `Order_Report_${order.orderDetails || 'Unknown'}.xlsx`;
    XLSX.writeFile(wb, fileName);
  }

  private formatCurrency(value: number): string {
    return value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }
}
