import {
  Component,
  ChangeDetectionStrategy,
  signal,
  inject,
  computed,
  OnInit,
} from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { BadgeModule } from 'primeng/badge';
import { ToastModule } from 'primeng/toast';
import { TabsModule } from 'primeng/tabs';
import { MessageService } from 'primeng/api';
import { FluidModule } from 'primeng/fluid';
import { TagModule } from 'primeng/tag';
import { FullScreenModalComponent } from '../../../shared/components/full-screen-modal/full-screen-modal.component';
import { OrderReportPrintComponent } from '../../reports/orders-report/print/order-report-print.component';
import { OrderMasterFormPrintComponent } from './print/order-master-form-print.component';
import { ReportService } from '../../../core/services/report.service';
import {
  OrderReportItem,
  OrderDetailItem,
  OrdersReportRequest,
  UpdateOrderRequest,
} from '../../../core/models/orders-report.models';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { TextareaModule } from 'primeng/textarea';
import { PaginatorModule } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { getOrderStatusSeverity } from '../../../core/utils/status-utils';
import { OrderDetailRequest } from '../../../core/models/orders-report.models';
import { ProductService } from '../../../core/services/product.service';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ProductDto } from '../../../core/models/product.models';
import {
  UpdateOrderItemRequest,
  RemoveOrderItemRequest,
  PackingListDto,
  PackingListItemDto,
  PackingListCartonDto,
  SavePackingListCommand,
  OrderWarehouseSelectionDto,
} from '../../../core/models/orders-report.models';
import { environment } from '../../../../environments/environment';
import { InventoryService, ProductStockStatusDto } from '../../../core/services/inventory.service';
import { RadioButtonModule } from 'primeng/radiobutton';
import { STOCK_CHECK_TRIGGER_STATUSES } from '../../../core/constants/order.constants';

@Component({
  selector: 'app-sales-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    DatePickerModule,
    ButtonModule,
    TableModule,
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
    InputGroupModule,
    InputGroupAddonModule,
    TextareaModule,
    PaginatorModule,
    ProgressSpinnerModule,
    DialogModule,
    CheckboxModule,
    DialogModule,
    CheckboxModule,
    OrderMasterFormPrintComponent,
    RadioButtonModule,
  ],
  template: `
    <div class="orders-report-page">
      <div class="flex flex-col gap-4">
        <!-- Üst Panel: Filtreler -->
        <div class="card">
          <div class="font-semibold text-xl mb-4">Sales Detail</div>
          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <!-- Başlangıç Tarihi -->
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

              <!-- Bitiş Tarihi -->
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

              <!-- Sipariş Durumu -->
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
                  [fluid]="true"
                ></p-select>
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
                <p-tab [value]="i" (click)="onBrandChange(brand)">{{ brand }}</p-tab>
              }
            </p-tablist>
          </p-tabs>

          <p-table
            #dt
            [value]="reportData()"
            [rows]="rows()"
            [paginator]="true"
            [lazy]="true"
            (onLazyLoad)="onLazyLoad($event)"
            [totalRecords]="totalRecords()"
            styleClass="p-datatable-gridlines p-datatable-sm custom-report-table"
            [responsiveLayout]="'scroll'"
            [globalFilterFields]="[
              'currentAccountCode',
              'currentAccountDescription',
              'orderDetails',
              'brand',
              'orderId',
            ]"
            [rowsPerPageOptions]="[10, 20, 50]"
            [showCurrentPageReport]="true"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
            paginatorDropdownAppendTo="body"
          >
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="currentAccountDescription">
                  CURRENT ACCOUNT DESCRIPTION
                  <p-sortIcon field="currentAccountDescription"></p-sortIcon>
                </th>
                <th pSortableColumn="orderDetails">
                  ORDER NO <p-sortIcon field="orderDetails"></p-sortIcon>
                </th>
                <th pSortableColumn="status" class="text-center">
                  STATUS <p-sortIcon field="status"></p-sortIcon>
                </th>
                <th pSortableColumn="brand">BRAND <p-sortIcon field="brand"></p-sortIcon></th>
                <th pSortableColumn="orderDate">
                  ORDER DATE <p-sortIcon field="orderDate"></p-sortIcon>
                </th>
                <th pSortableColumn="requestedShipmentDate">
                  REQUESTED SHIPMENT DATE <p-sortIcon field="requestedShipmentDate"></p-sortIcon>
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
                <th class="text-center">EDIT</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-item>
              <tr class="dark:bg-surface-900 dark:text-surface-0">
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
                    icon="pi pi-pencil"
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
                <td colspan="6" class="text-left py-3">Total (Current Page)</td>
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

      <!-- Detay Modalı (Edit Formu) -->
      <app-full-screen-modal [(visible)]="showDetail" [style]="{ width: '70vw' }">
        <div header class="flex items-center justify-between w-full no-print">
          <div class="text-xl font-bold text-surface-900 dark:text-surface-0">Order Details</div>
        </div>

        <div class="flex flex-col bg-surface-50 dark:bg-surface-950 p-6 min-h-[600px]">
          <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-stretch">
            <!-- Sol Sütun: Order Additional Information -->
            <div class="lg:col-span-8 flex">
              <div
                class="card p-0 h-full flex flex-col overflow-hidden border-surface-200 dark:border-surface-700 w-full"
              >
                <div class="bg-surface-200 dark:bg-surface-800 p-3">
                  <span
                    class="font-bold text-surface-700 dark:text-surface-100 uppercase tracking-wider text-sm"
                    >Order Additional Information</span
                  >
                </div>
                <div class="p-6 flex-1 flex flex-col gap-6 bg-white dark:bg-surface-900">
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-4">
                    <div class="flex flex-col gap-1">
                      <span class="text-xs font-bold text-surface-500 uppercase"
                        >Current Account Code</span
                      >
                      <span class="text-sm font-semibold text-surface-700 dark:text-surface-200">
                        {{ selectedOrder()?.currentAccountCode }}
                        {{ selectedOrder()?.currentAccountDescription }}
                      </span>
                    </div>

                    <div class="flex flex-col gap-1">
                      <span class="text-xs font-bold text-surface-500 uppercase">Address</span>
                      <span class="text-sm text-surface-600 dark:text-surface-300">
                        {{ selectedOrder()?.address || 'N/A' }}
                      </span>
                    </div>

                    <div class="flex flex-col gap-1">
                      <span class="text-xs font-bold text-surface-500 uppercase">Payment Type</span>
                      <span class="text-sm text-surface-600 dark:text-surface-300">
                        {{ selectedOrder()?.paymentType || 'N/A' }}
                      </span>
                    </div>

                    <div class="flex flex-col gap-2">
                      <label class="text-xs font-bold text-surface-500 uppercase"
                        >Total Discount</label
                      >
                      <p-inputgroup>
                        <input
                          type="number"
                          pInputText
                          [ngModel]="editForm().discountPercent"
                          (ngModelChange)="updateFormField('discountPercent', $event)"
                          placeholder="0"
                        />
                        <p-inputgroup-addon>
                          <i class="pi pi-percentage"></i>
                        </p-inputgroup-addon>
                      </p-inputgroup>
                    </div>

                    <div class="flex flex-col gap-2">
                      <label class="text-xs font-bold text-surface-500 uppercase"
                        >Requested Shipment Date</label
                      >
                      <p-datepicker
                        [ngModel]="editForm().requestedShipmentDate"
                        (ngModelChange)="updateFormField('requestedShipmentDate', $event)"
                        [showIcon]="true"
                        dateFormat="dd.mm.yy"
                        [fluid]="true"
                      ></p-datepicker>
                    </div>

                    <div class="flex flex-col gap-2">
                      <label class="text-xs font-bold text-surface-500 uppercase"
                        >Shipment Method</label
                      >
                      <p-select
                        [options]="shipmentOptions()"
                        [ngModel]="editForm().shipmentMethod"
                        (ngModelChange)="updateFormField('shipmentMethod', $event)"
                        optionLabel="label"
                        optionValue="value"
                        [fluid]="true"
                        placeholder="Select Method"
                      ></p-select>
                    </div>

                    <div class="flex flex-col gap-2">
                      <label class="text-xs font-bold text-surface-500 uppercase"
                        >Order Status</label
                      >
                      <p-select
                        [options]="editStatusOptions()"
                        [ngModel]="editForm().status"
                        (ngModelChange)="updateFormField('status', $event)"
                        optionLabel="label"
                        optionValue="value"
                        [fluid]="true"
                      ></p-select>
                    </div>

                    <div class="flex flex-col gap-2 md:col-span-2">
                      <label class="text-xs font-bold text-surface-500 uppercase"
                        >Order Description</label
                      >
                      <textarea
                        pTextarea
                        [ngModel]="editForm().orderDescription"
                        (ngModelChange)="updateFormField('orderDescription', $event)"
                        rows="3"
                        [fluid]="true"
                        placeholder="Order Description"
                      ></textarea>
                    </div>
                  </div>

                  <div class="flex justify-end mt-4 gap-2">
                    @if (editForm().status === 'PreOrder') {
                      <p-button
                        label="Reservation Quantity Not Enough"
                        icon="pi pi-exclamation-triangle"
                        severity="warn"
                        styleClass="px-4"
                      ></p-button>
                    }
                    <p-button
                      label="Save"
                      icon="pi pi-check"
                      severity="danger"
                      [loading]="saving()"
                      (onClick)="onSave()"
                      styleClass="px-8"
                    ></p-button>
                  </div>
                </div>
              </div>
            </div>
            <!-- Sağ Sütun: Page Total -->
            <div class="lg:col-span-4 flex">
              <div
                class="card p-0 h-full flex flex-col overflow-hidden border-surface-200 dark:border-surface-700 w-full"
              >
                <div class="bg-surface-200 dark:bg-surface-800 p-3">
                  <span
                    class="font-bold text-surface-700 dark:text-surface-100 uppercase tracking-wider text-sm"
                    >Page Total</span
                  >
                </div>
                <div class="p-0 bg-white dark:bg-surface-900 flex-1 flex flex-col">
                  <div class="divide-y divide-surface-100 dark:divide-surface-800">
                    <div class="flex justify-between items-center p-4">
                      <span class="text-sm font-semibold text-surface-500">Total Qty</span>
                      <span class="text-base font-bold text-surface-700 dark:text-surface-200">
                        {{ editTotals().totalQty | number }}
                      </span>
                    </div>
                    <div class="flex justify-between items-center p-4">
                      <span class="text-sm font-semibold text-surface-500">Total Discount</span>
                      <span class="text-base font-bold text-surface-700 dark:text-surface-200">
                        {{ editTotals().discountAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                      </span>
                    </div>
                    <div class="flex justify-between items-center p-4">
                      <span class="text-sm font-semibold text-surface-500">Grand Total</span>
                      <span class="text-xl font-black text-surface-900 dark:text-surface-0">
                        {{ editTotals().grandTotal | currency: 'USD' : 'symbol' : '1.2-2' }}
                      </span>
                    </div>
                  </div>

                  <div class="p-6 mt-auto flex flex-col gap-2">
                    <p-button
                      label="Proforma Invoice"
                      icon="pi pi-file"
                      severity="danger"
                      styleClass="w-full text-sm py-2"
                      [loading]="proformaLoading()"
                      (onClick)="onProformaInvoice()"
                    ></p-button>
                    <p-button
                      label="Invoice"
                      icon="pi pi-file-pdf"
                      severity="danger"
                      styleClass="w-full text-sm py-2"
                      [loading]="invoiceLoading()"
                      (onClick)="onInvoice()"
                    ></p-button>
                    <p-button
                      label="Packing List"
                      icon="pi pi-box"
                      severity="danger"
                      styleClass="w-full text-sm py-2"
                      (onClick)="onPackingList()"
                    ></p-button>
                    <p-button
                      label="Print Order"
                      icon="pi pi-print"
                      severity="danger"
                      styleClass="w-full text-sm py-2"
                      [loading]="printLoading()"
                      (onClick)="onPrint()"
                    ></p-button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Ürünler Bölümü (Görseldeki Tasarım) -->
          <div class="mt-8">
            <div
              class="flex items-center justify-between bg-surface-200 dark:bg-surface-800 p-3 rounded-t-md mb-6"
            >
              <span
                class="font-bold text-surface-700 dark:text-surface-100 uppercase tracking-wider text-sm"
                >Products</span
              >
              <p-button
                label="Add New Product"
                icon="pi pi-plus"
                severity="danger"
                size="small"
                [raised]="true"
                (onClick)="onAddNewProduct()"
              ></p-button>
            </div>

            <div class="relative min-h-[200px]">
              @if (detailLoading()) {
                <div
                  class="absolute inset-0 flex flex-col items-center justify-center bg-white/50 dark:bg-surface-900/50 z-10 gap-3"
                >
                  <p-progressspinner styleClass="w-12 h-12" strokeWidth="4"></p-progressspinner>
                  <span class="text-surface-600 dark:text-surface-400 animate-pulse font-medium"
                    >Loading products...</span
                  >
                </div>
              }

              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                @for (product of detailData(); track product.productCode) {
                  <div
                    class="bg-white dark:bg-surface-900 border border-surface-200 dark:border-surface-700 rounded-lg overflow-hidden flex flex-col shadow-sm hover:shadow-md transition-shadow duration-300"
                  >
                    <!-- Ürün Başlığı -->
                    <div class="p-3 bg-surface-50 dark:bg-surface-800/50 text-center">
                      <span
                        class="text-xs font-bold text-surface-600 dark:text-surface-300 uppercase"
                      >
                        {{ product.productName }}
                      </span>
                    </div>

                    <!-- Ürün Resmi ve Kod -->
                    <div
                      class="p-6 flex flex-col items-center gap-4 border-b border-dashed border-surface-200 dark:border-surface-700"
                    >
                      <div
                        class="w-40 h-40 flex items-center justify-center bg-white rounded-lg p-2"
                      >
                        @if (imageErrors().has(product.productCode)) {
                          <div
                            class="flex flex-col items-center justify-center text-surface-400 dark:text-surface-600 gap-2"
                          >
                            <i class="pi pi-image text-4xl opacity-50"></i>
                            <span class="text-[10px] font-bold uppercase tracking-widest opacity-70"
                              >No Image</span
                            >
                          </div>
                        } @else {
                          <img
                            [src]="
                              environment.cdnUrl + '/ProductImages/' + product.productCode + '.jpg'
                            "
                            [alt]="product.productName"
                            class="max-w-full max-h-full object-contain"
                            (error)="onImageError(product.productCode)"
                          />
                        }
                      </div>
                      <span class="text-blue-600 font-bold text-sm tracking-widest">{{
                        product.productCode
                      }}</span>
                    </div>

                    <!-- Ürün Form Alanları -->
                    <div class="p-5 flex flex-col gap-4">
                      <div class="flex flex-col gap-1.5">
                        <label class="text-[10px] font-bold text-surface-400 uppercase"
                          >Product Code : {{ product.productCode }}</label
                        >
                      </div>

                      <div class="flex flex-col gap-1.5">
                        <label class="text-[10px] font-bold text-surface-400 uppercase"
                          >Price :</label
                        >
                        <input
                          pInputText
                          type="number"
                          [ngModel]="product.amount"
                          (ngModelChange)="onProductValueChange(product, 'amount', $event)"
                          class="h-9 text-sm"
                        />
                      </div>

                      <div class="flex flex-col gap-1.5">
                        <label class="text-[10px] font-bold text-surface-400 uppercase"
                          >QTY (PCS) :</label
                        >
                        <p-inputgroup class="h-9">
                          <button
                            type="button"
                            pButton
                            icon="pi pi-minus"
                            class="p-button-outlined p-button-secondary w-10!"
                            (click)="onProductQtyChange(product, -1)"
                          ></button>
                          <input
                            type="number"
                            pInputText
                            [ngModel]="product.qty"
                            (ngModelChange)="onProductValueChange(product, 'qty', $event)"
                            class="text-center text-sm font-bold"
                          />
                          <button
                            type="button"
                            pButton
                            icon="pi pi-plus"
                            class="p-button-outlined p-button-secondary w-10!"
                            (click)="onProductQtyChange(product, 1)"
                          ></button>
                        </p-inputgroup>
                      </div>

                      <div class="flex flex-col gap-1.5">
                        <label class="text-[10px] font-bold text-surface-400 uppercase">
                          Total :
                          <span class="text-surface-900 dark:text-white font-bold">{{
                            product.amount * product.qty | currency: 'USD' : 'symbol' : '1.2-2'
                          }}</span>
                        </label>
                      </div>

                      <div class="flex flex-col gap-1.5">
                        <label class="text-[10px] font-bold text-surface-400 uppercase"
                          >Order Description :</label
                        >
                        <textarea
                          pTextarea
                          [(ngModel)]="product.notes"
                          rows="2"
                          class="text-xs resize-none"
                          placeholder="Order Description"
                        ></textarea>
                      </div>

                      <!-- Butonlar -->
                      <div class="flex justify-between items-center mt-4">
                        <p-button
                          label="Save"
                          severity="danger"
                          class="h-10"
                          styleClass="text-xs"
                          [disabled]="!isProductChanged(product)"
                          (onClick)="onSaveProduct(product)"
                        ></p-button>

                        <p-button
                          label="Remove from List"
                          severity="danger"
                          class="h-10"
                          styleClass="text-xs"
                          (onClick)="onRemoveProduct(product)"
                        ></p-button>
                      </div>
                    </div>
                  </div>
                } @empty {
                  @if (!detailLoading()) {
                    <div
                      class="col-span-full flex flex-col items-center justify-center p-12 bg-surface-50 dark:bg-surface-800/50 rounded-lg border border-dashed border-surface-300 dark:border-surface-600"
                    >
                      <i class="pi pi-box text-5xl text-surface-400 mb-4"></i>
                      <span class="text-surface-600 dark:text-surface-400 font-medium"
                        >No products found for this order.</span
                      >
                    </div>
                  }
                }
              </div>
            </div>

            <!-- Sayfalama (Pagination) -->
            <div
              class="mt-8 flex justify-center bg-white dark:bg-surface-900 p-4 rounded-md border border-surface-200 dark:border-surface-700 shadow-sm"
            >
              <p-paginator
                [rows]="detailRows()"
                [totalRecords]="detailTotalCount()"
                [first]="detailPage() * detailRows()"
                [rowsPerPageOptions]="[10, 20, 50]"
                (onPageChange)="onDetailPageChange($event)"
                styleClass="bg-transparent border-none"
              ></p-paginator>
            </div>
          </div>
        </div>

        <div footer class="flex justify-end gap-2 no-print">
          <p-button
            label="Close"
            icon="pi pi-times"
            severity="secondary"
            [text]="true"
            (onClick)="showDetail.set(false)"
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

      <app-order-master-form-print
        id="master-form-print-section"
        class="hidden"
        [order]="selectedOrder()"
        [details]="printData()"
        project="CWI"
      ></app-order-master-form-print>
    </div>

    <!-- Ürün Seçme Modalı -->
    <p-dialog
      header="Select Product to Add"
      [(visible)]="showProductSelector"
      [modal]="true"
      [style]="{ width: '80vw' }"
      [breakpoints]="{ '960px': '90vw' }"
      [draggable]="false"
      [resizable]="false"
    >
      <div class="flex flex-col gap-6">
        <!-- Arama Filtresi -->
        <div
          class="flex justify-between items-center bg-surface-50 dark:bg-surface-800 p-4 rounded-lg"
        >
          <p-iconfield iconPosition="left" class="w-full md:w-80">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              [ngModel]="productSearchTerm()"
              (ngModelChange)="productSearchTerm.set($event)"
              (input)="onProductSearch()"
              placeholder="Search products..."
              class="w-full"
            />
          </p-iconfield>
        </div>

        <div class="relative min-h-[400px]">
          @if (productSelectorLoading()) {
            <div
              class="absolute inset-0 flex flex-col items-center justify-center bg-white/50 dark:bg-surface-900/50 z-10 gap-3"
            >
              <p-progressspinner styleClass="w-12 h-12" strokeWidth="4"></p-progressspinner>
              <span class="text-surface-600 dark:text-surface-400 font-medium"
                >Loading products...</span
              >
            </div>
          }

          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            @for (product of availableProducts(); track product.sku) {
              <div
                class="bg-white dark:bg-surface-900 border border-surface-200 dark:border-surface-700 rounded-lg overflow-hidden flex flex-col shadow-sm cursor-pointer hover:border-red-400 transition-all duration-300"
                (click)="onSelectProduct(product)"
              >
                <div class="p-3 bg-surface-50 dark:bg-surface-800/50 text-center">
                  <span class="text-xs font-bold text-surface-600 dark:text-surface-300 uppercase">
                    {{ product.name }}
                  </span>
                </div>
                <div class="p-4 flex flex-col items-center gap-3">
                  <div class="w-32 h-32 flex items-center justify-center bg-white rounded-lg">
                    @if (imageErrors().has(product.sku)) {
                      <div
                        class="flex flex-col items-center justify-center text-surface-400 dark:text-surface-600 gap-2"
                      >
                        <i class="pi pi-image text-4xl opacity-50"></i>
                        <span class="text-[10px] font-bold uppercase tracking-widest opacity-70"
                          >No Image</span
                        >
                      </div>
                    } @else {
                      <img
                        [src]="environment.cdnUrl + '/ProductImages/' + product.sku + '.jpg'"
                        [alt]="product.name"
                        class="max-w-full max-h-full object-contain"
                        (error)="onImageError(product.sku)"
                      />
                    }
                  </div>
                  <span class="text-blue-600 font-bold text-xs">{{ product.sku }}</span>
                  <span class="font-bold text-lg text-red-500"
                    >$ {{ product.purchasePrice | number: '1.2-2' }}</span
                  >
                </div>
                <div
                  class="p-3 bg-surface-0 dark:bg-surface-900 border-t border-surface-100 dark:border-surface-800 text-center"
                >
                  <p-button
                    label="Select"
                    icon="pi pi-plus"
                    size="small"
                    severity="danger"
                    [text]="true"
                    class="w-full"
                  ></p-button>
                </div>
              </div>
            }
          </div>
        </div>

        <div class="mt-4 flex justify-center">
          <p-paginator
            [rows]="productSelectorRows()"
            [totalRecords]="productSelectorTotalCount()"
            [first]="productSelectorPage() * productSelectorRows()"
            [rowsPerPageOptions]="[8, 16, 24]"
            (onPageChange)="onProductSelectorPageChange($event)"
          ></p-paginator>
        </div>
      </div>
    </p-dialog>

    <!-- Warehouse Selection Modal -->
    <p-dialog
      header="Select Warehouse"
      [(visible)]="showWarehouseSelector"
      [modal]="true"
      [style]="{ width: '50vw' }"
      [draggable]="false"
      [resizable]="false"
    >
      <div class="flex flex-col gap-4">
        <p class="text-surface-600 dark:text-surface-300 mb-4">
          Some products are available in multiple warehouses. Please select which warehouse to
          fulfill from.
        </p>

        <div class="flex flex-col gap-4 max-h-[60vh] overflow-y-auto">
          @for (item of stockStatusList(); track item.productCode) {
            @if (item.hasMultipleWarehouses) {
              <div class="border border-surface-200 dark:border-surface-700 rounded-lg p-4">
                <div class="font-bold mb-2">
                  {{ item.productName }}
                  <span class="text-xs text-surface-500">({{ item.productCode }})</span>
                </div>
                <div class="text-sm mb-3">
                  Required Qty: <span class="font-semibold">{{ item.requiredQty }}</span>
                </div>

                <div class="flex flex-col gap-2">
                  @for (wh of item.warehouses; track wh.warehouseId) {
                    <div class="flex items-center">
                      <p-radiobutton
                        [name]="item.productCode"
                        [value]="wh.warehouseId"
                        [(ngModel)]="warehouseSelections()[item.productCode]"
                        [inputId]="item.productCode + '_' + wh.warehouseId"
                      ></p-radiobutton>
                      <label
                        [for]="item.productCode + '_' + wh.warehouseId"
                        class="ml-2 cursor-pointer"
                      >
                        {{ wh.warehouseName }} (Available: {{ wh.availableQty }})
                      </label>
                    </div>
                  }
                </div>
              </div>
            }
          }
        </div>

        <div class="flex justify-end gap-2 mt-4">
          <p-button
            label="Cancel"
            severity="secondary"
            [text]="true"
            (onClick)="showWarehouseSelector.set(false)"
          ></p-button>
          <p-button
            label="Confirm & Save"
            severity="danger"
            (onClick)="onSaveWarehouseSelection()"
          ></p-button>
        </div>
      </div>
    </p-dialog>

    <!-- Packing List Modal -->
    <app-full-screen-modal [(visible)]="showPackingList" [style]="{ width: '70vw' }">
      <div header class="flex items-center justify-between w-full no-print">
        <div class="text-xl font-bold text-surface-900 dark:text-surface-0">PACKING LIST</div>
        <div class="flex gap-2">
          <p-button
            label="SAVE"
            icon="pi pi-check"
            severity="success"
            size="small"
            (onClick)="onSavePackingList()"
            [loading]="packingListSaving()"
          ></p-button>
          <p-button label="SAVE AS EXCEL (2007 FORMAT)" severity="danger" size="small"></p-button>
        </div>
      </div>

      <div class="flex flex-col gap-6 bg-surface-50 dark:bg-surface-950 p-6 min-h-[600px]">
        <!-- Section 1: Order Details / Items (Rename to Packing List in image context, but keeps content) -->
        <div class="card p-0 overflow-hidden border-surface-200 dark:border-surface-700">
          <div class="bg-surface-400 dark:bg-surface-700 p-3">
            <span class="font-bold text-white uppercase tracking-wider text-sm">ORDER DETAILS</span>
          </div>
          <p-table
            [value]="packingListItems()"
            styleClass="p-datatable-sm detail-table"
            [rowHover]="true"
          >
            <ng-template pTemplate="header">
              <tr>
                <th style="width: 20%">Item Code</th>
                <th style="width: 40%">ItemDesc</th>
                <th style="width: 15%" class="text-center">QTY (PCS)</th>
                <th style="width: 25%">Carton NO</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-item>
              <tr>
                <td class="font-medium">{{ item.productCode }}</td>
                <td>{{ item.productName }}</td>
                <td class="text-center font-bold">{{ item.qty }}</td>
                <td>
                  <input
                    pInputText
                    type="text"
                    [(ngModel)]="item.cartonNo"
                    class="w-full h-8 text-sm"
                  />
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>

        <!-- Section 2: Cartons -->
        <div class="card p-0 overflow-hidden border-surface-200 dark:border-surface-700">
          <div class="bg-surface-400 dark:bg-surface-700 p-3">
            <span class="font-bold text-white uppercase tracking-wider text-sm">Cartons</span>
          </div>

          <!-- List of added cartons (Optional, implicitly needed) -->
          @if (cartons().length > 0) {
            <p-table [value]="cartons()" styleClass="p-datatable-sm detail-table mb-4">
              <ng-template pTemplate="header">
                <tr>
                  <th>Carton NO</th>
                  <th>Net Weight</th>
                  <th>Gross Weight</th>
                  <th>Measurements</th>
                  <th class="text-center" style="width: 100px">Action</th>
                </tr>
              </ng-template>
              <ng-template pTemplate="body" let-carton let-index="rowIndex">
                <tr>
                  <td>{{ carton.cartonNo }}</td>
                  <td>{{ carton.netWeight }}</td>
                  <td>{{ carton.grossWeight }}</td>
                  <td>{{ carton.measurements }}</td>
                  <td class="text-center">
                    <p-button
                      icon="pi pi-trash"
                      severity="danger"
                      [text]="true"
                      [rounded]="true"
                      size="small"
                      (onClick)="onRemoveCarton(index)"
                    ></p-button>
                  </td>
                </tr>
              </ng-template>
            </p-table>
          }

          <!-- Add New Carton Form -->
          <div
            class="p-4 bg-white dark:bg-surface-900 border-t border-surface-200 dark:border-surface-700"
          >
            <div class="grid grid-cols-1 md:grid-cols-12 gap-4 items-end">
              <div class="md:col-span-3 flex flex-col gap-2">
                <label class="text-xs font-bold text-surface-500 uppercase">Carton NO</label>
                <input pInputText type="text" [(ngModel)]="newCarton().cartonNo" class="w-full" />
              </div>
              <div class="md:col-span-3 flex flex-col gap-2">
                <label class="text-xs font-bold text-surface-500 uppercase">Net Weight</label>
                <input pInputText type="text" [(ngModel)]="newCarton().netWeight" class="w-full" />
              </div>
              <div class="md:col-span-3 flex flex-col gap-2">
                <label class="text-xs font-bold text-surface-500 uppercase">Gross Weight</label>
                <input
                  pInputText
                  type="text"
                  [(ngModel)]="newCarton().grossWeight"
                  class="w-full"
                />
              </div>
              <div class="md:col-span-2 flex flex-col gap-2">
                <label class="text-xs font-bold text-surface-500 uppercase">Measurements</label>
                <input
                  pInputText
                  type="text"
                  [(ngModel)]="newCarton().measurements"
                  placeholder="00*00*00"
                  class="w-full"
                />
              </div>
              <div class="md:col-span-1">
                <p-button
                  label="Add"
                  severity="danger"
                  styleClass="w-full"
                  (onClick)="onAddCarton()"
                ></p-button>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div footer class="flex justify-end gap-2 no-print">
        <p-button
          label="Close"
          icon="pi pi-times"
          severity="secondary"
          [text]="true"
          (onClick)="showPackingList.set(false)"
        ></p-button>
      </div>
    </app-full-screen-modal>
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
          /* Üst katmandaki window.print artık kullanılmıyor, iframe üzerinden çıktı alıyoruz. */
          /* Ancak genel bir önlem olarak burada stilleri sadeleştirelim. */
          body * {
            display: none !important;
          }
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesDetail implements OnInit {
  protected readonly environment = environment;
  protected readonly Math = Math;
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);
  private productService = inject(ProductService);
  private inventoryService = inject(InventoryService);

  loading = signal(false);
  rows = signal(10);
  searchValue = signal('');
  totalRecords = signal(0);

  // Ürün Seçimi Sinyalleri
  showProductSelector = signal(false);
  availableProducts = signal<ProductDto[]>([]);
  productSelectorLoading = signal(false);
  productSearchTerm = signal('');
  productSelectorPage = signal(0);
  productSelectorRows = signal(8);
  productSelectorTotalCount = signal(0);

  // Filtreleme verileri
  filters = signal<OrdersReportRequest>({
    orderStatus: null as any,
    displayProductPhoto: false,
    startDate: null as any,
    endDate: null as any,
  });

  // Stok kontrolü ve Warehouse seçimi
  showWarehouseSelector = signal(false);
  stockStatusList = signal<ProductStockStatusDto[]>([]);
  warehouseSelections = signal<Record<string, number>>({});

  // Seçenekler

  statusOptions = signal<{ label: string; value: string }[]>([]);
  shipmentOptions = signal<{ label: string; value: string }[]>([
    { label: 'FOB', value: 'FOB' },
    { label: 'EXWORKS', value: 'EXWORKS' },
    { label: 'CIF', value: 'CIF' },
  ]);

  // Edit Status seçenekleri (Load All hariç)
  editStatusOptions = computed(() => {
    return this.statusOptions().filter((opt) => opt.value !== null && opt.value !== undefined);
  });

  // Edit Formu
  editForm = signal<UpdateOrderRequest>({
    orderId: 0,
    paymentType: '',
    discountPercent: 0,
    requestedShipmentDate: null as any,
    shipmentMethod: '',
    status: '',
    orderDescription: '',
  });

  /**
   * Form alanlarını günceller ve sinyali tetikler.
   * Bu sayede computed alanların (editTotals gibi) yeniden hesaplanması sağlanır.
   * @param key Güncellenecek alanın adı
   * @param value Yeni değer
   */
  updateFormField(key: keyof UpdateOrderRequest, value: any) {
    this.editForm.update((prev) => ({
      ...prev,
      [key]: value,
    }));
  }

  saving = signal(false);
  reservationWarning = signal(false);

  // Edit sayfasi canli toplamlari - detailData() signal'inden hesaplanir
  // Boylece urun qty/amount degistiginde veya yeni urun eklendiginde otomatik guncellenir
  editTotals = computed(() => {
    const details = this.detailData();

    if (!details || details.length === 0) {
      return { subTotal: 0, discountAmount: 0, grandTotal: 0, totalQty: 0 };
    }

    let totalQty = 0;
    let subTotal = 0;

    details.forEach((item) => {
      totalQty += item.qty || 0;
      subTotal += (item.qty || 0) * (item.amount || 0);
    });

    const discountPercent = this.editForm().discountPercent || 0;
    const discountAmount = subTotal * (discountPercent / 100);
    const grandTotal = subTotal - discountAmount;

    return {
      subTotal,
      discountAmount,
      grandTotal,
      totalQty,
    };
  });

  ngOnInit() {
    this.loadLookups();
  }

  private getDefaultStatusOptions() {
    return [
      { label: 'Load All', value: null as any },
      { label: 'Pre Order', value: 'PreOrder' },
      { label: 'Pending', value: 'Pending' },
      { label: 'Packed & Waiting Shipment', value: 'PackedAndWaitingShipment' },
      { label: 'Shipped', value: 'Shipped' },
      { label: 'Canceled', value: 'Canceled' },
      { label: 'Draft', value: 'Draft' },
    ];
  }

  private normalizeStatusOptions(raw: any[]): { label: string; value: string | null }[] {
    if (!Array.isArray(raw)) return [];

    return raw
      .map((item) => ({
        label: item?.label ?? item?.Label ?? item?.text ?? item?.Text ?? '',
        value: item?.value ?? item?.Value ?? null,
      }))
      .filter((item) => !!item.label);
  }

  loadLookups() {
    this.reportService.getOrderStatuses().subscribe((res) => {
      const normalized = this.normalizeStatusOptions(res?.data as any[]);
      if (res.success && normalized.length > 0) {
        this.statusOptions.set(normalized as { label: string; value: string }[]);
        return;
      }

      this.statusOptions.set(this.getDefaultStatusOptions() as { label: string; value: string }[]);
    });
    // shipmentOptions art�k statik (g�rsele g�re)
  }

  private ensureStatusOptionsLoaded() {
    if (this.statusOptions().length > 0) return;

    this.statusOptions.set(this.getDefaultStatusOptions() as { label: string; value: string }[]);
    this.reportService.getOrderStatuses().subscribe((res) => {
      const normalized = this.normalizeStatusOptions(res?.data as any[]);
      if (res.success && normalized.length > 0) {
        this.statusOptions.set(normalized as { label: string; value: string }[]);
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

  // Detay verisi
  showDetail = signal(false);
  detailRows = signal(10);
  detailPage = signal(0);
  detailTotalCount = signal(0);
  detailLoading = signal(false);
  selectedOrder = signal<OrderReportItem | null>(null);
  detailData = signal<OrderDetailItem[]>([]);
  printData = signal<OrderDetailItem[]>([]);
  removedItems = signal<OrderDetailItem[]>([]);
  originalProductStates = signal<Record<string, string>>({});
  imageErrors = signal<Set<string>>(new Set());

  /**
   * Görsel yükleme hatası durumunda tetiklenir
   * @param code Ürün kodu veya SKU
   */
  onImageError(code: string) {
    this.imageErrors.update((prev) => {
      const next = new Set(prev);
      next.add(code);
      return next;
    });
  }

  /**
   * Sipariş detaylarını sayfalama ile getirir
   */
  loadOrderDetails() {
    const order = this.selectedOrder();
    if (!order) return;

    this.detailLoading.set(true);

    const request: OrderDetailRequest = {
      orderId: order.orderId,
      brand: order.brand,
      pageNumber: this.detailPage() + 1,
      pageSize: this.detailRows(),
    };

    this.reportService.getOrderDetails(request).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          // Silinmiş ürünleri filtrele
          const filtered = res.data.data.filter(
            (item) => !this.removedItems().some((r) => r.productCode === item.productCode),
          );
          this.detailData.set(filtered);

          // Orijinal hallerini kaydet (Değişiklik takibi için)
          const states: Record<string, string> = {};
          res.data.data.forEach((item) => {
            states[item.productCode] = JSON.stringify({
              qty: item.qty,
              amount: item.amount,
              notes: item.notes || '',
            });
          });
          this.originalProductStates.set(states);

          // Toplam sayıdan silinenleri (sadece bu sayfada olanları değil, tüm listedekileri) düşmek yanıltıcı olabilir.
          // Ama basitlik adına ve UI tutarlılığı için:
          this.detailTotalCount.set(Math.max(0, res.data.totalCount - this.removedItems().length));
        }
        this.detailLoading.set(false);
      },
      error: () => {
        this.detailLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load product details.',
        });
      },
    });
  }

  /**
   * Sayfa değiştiğinde tetiklenir
   */
  onDetailPageChange(event: any) {
    this.detailPage.set(event.page);
    this.detailRows.set(event.rows);
    this.loadOrderDetails();
  }

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
    this.totalRecords.set(0);
    // Reset page to 1 when filters change explicitly
    this.filters.update((f) => ({ ...f, page: 1 }));
    this.loadData();
  }

  onLazyLoad(event: any) {
    // PrimeNG Lazy Load Page Calculation
    const page = (event.first || 0) / (event.rows || 10) + 1;
    this.rows.set(event.rows || 10);

    this.filters.update((prev) => ({
      ...prev,
      page: page,
      pageSize: event.rows || 10,
      sortField: event.sortField,
      sortOrder: event.sortOrder,
    }));

    this.loadData();
  }

  onBrandChange(brand: string) {
    this.selectedBrand.set(brand);
    this.filters.update((prev) => ({ ...prev, brand: brand }));
    this.onReport();
  }

  loadData() {
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
      orderStatus: currentFilters.orderStatus || (null as any),
      page: currentFilters.page, // Use current page from filters
      pageSize: currentFilters.pageSize, // Use current page size from filters
    };

    this.reportService.getOrdersReport(request).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.reportData.set(res.data.data);
          this.totalRecords.set(res.data.totalCount);
          this.brands.set(res.data.brands);

          // If no brand selected (initial load), select first
          if (!this.selectedBrand() && res.data.brands.length > 0) {
            this.selectedBrand.set(res.data.brands[0]);
            // We should probably filter by this brand if server didn't returns filtered data,
            // BUT with server-side filtering we expect data to be already filtered if we sent 'brand'.
            // If we didn't send 'brand' (initial), server might return mixed data.
            // In our Handler implementation, if 'Brand' filter is null, it returns data for all brands?
            // The handler applies filter if !string.IsNullOrEmpty(filters.Brand).
            // So if we fetch All, we get All.
            // The UI design uses Tabs to filter.
            // If we want the UI to start with "All" or first Brand?
            // The original code: tabs for brands. Table showed filtered data.

            // If we want to emulate old behavior:
            // Initial load -> GetAll -> Brands populated.
            // Select First Brand -> Reload with Filter.

            if (!this.filters().brand) {
              this.onBrandChange(res.data.brands[0]);
              return; // onBrandChange will trigger loadData
            }
          }

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

  // Detay görüntüle (Edit Modu)
  onViewDetail(item: OrderReportItem) {
    this.ensureStatusOptionsLoaded();
    this.selectedOrder.set(item);
    const normalizedStatus = this.normalizeStatusValue(item.status);

    // Formu doldur
    this.editForm.set({
      orderId: item.orderId,
      paymentType: item.paymentType || '',
      discountPercent: (item.discount / (item.total + item.discount)) * 100 || 0, // Basit bir hesaplama
      requestedShipmentDate: item.requestedShipmentDate
        ? (new Date(item.requestedShipmentDate) as any)
        : null,
      shipmentMethod: item.shipmentMethod || '',
      status: normalizedStatus,
      orderDescription: item.orderDescription || '',
    });

    this.showDetail.set(true);

    // Silinenleri sıfırla
    this.removedItems.set([]);

    // Sayfalamayı sıfırla ve detayları çek
    this.detailPage.set(0);
    this.showPrintPreview.set(false);
    this.loadOrderDetails();
  }

  onSave() {
    const request = this.editForm();
    if (!request.orderId) return;

    // Eğer statü stok kontrolü gerektiriyorsa (örn: PackedAndWaitingShipment) stok kontrolü yap
    if (STOCK_CHECK_TRIGGER_STATUSES.includes(request.status || '')) {
      this.saving.set(true);
      this.inventoryService.checkOrderStock(request.orderId).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            const stockList = res.data;
            this.stockStatusList.set(stockList);

            // Birden fazla deposu olan ürün var mı kontrol et
            const productsWithMultipleWarehouses = stockList.filter((p) => p.hasMultipleWarehouses);

            if (productsWithMultipleWarehouses.length > 0) {
              // Varsayılan seçimleri hazırla (en çok stok olanı seç)
              const selections: Record<string, number> = {};
              stockList.forEach((p) => {
                if (p.warehouses.length > 0) {
                  // Varsayılan olarak ilkini (en çok stok olan, query'de sort ettik) seç
                  selections[p.productCode] = p.warehouses[0].warehouseId;
                }
              });
              this.warehouseSelections.set(selections);

              this.saving.set(false);
              this.showWarehouseSelector.set(true);
              return;
            }

            // Otomatik seçim yap (tek seçenek varsa)
            const selections: OrderWarehouseSelectionDto[] = [];
            stockList.forEach((p) => {
              // Eğer sadece bir depo varsa onu ekle, yoksa (stok yoksa) ekleme
              if (p.warehouses.length === 1) {
                selections.push({
                  productCode: p.productCode,
                  warehouseId: p.warehouses[0].warehouseId,
                });
              }
            });

            this.processUpdateOrder(selections);
          } else {
            this.saving.set(false);
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: res.error || 'Failed to check stock status.',
            });
          }
        },
        error: () => {
          this.saving.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to check stock status.',
          });
        },
      });
      return;
    }

    // Diğer durumlarda direkt kaydet
    this.processUpdateOrder([]);
  }

  onSaveWarehouseSelection() {
    const selections = Object.entries(this.warehouseSelections()).map(([code, id]) => ({
      productCode: code,
      warehouseId: id,
    }));
    this.processUpdateOrder(selections);
    this.showWarehouseSelector.set(false);
  }

  processUpdateOrder(warehouseSelections: OrderWarehouseSelectionDto[]) {
    const request = this.editForm();
    this.saving.set(true);
    const normalizedStatus = this.normalizeStatusValue(request.status || '');

    // Tarihi ISO formatına çevir ve silinenleri ekle
    const formattedRequest: UpdateOrderRequest = {
      ...request,
      status: normalizedStatus,
      requestedShipmentDate: request.requestedShipmentDate
        ? new Date(request.requestedShipmentDate).toISOString()
        : undefined,
      removedProductCodes: this.removedItems().map((i) => i.productCode),
      warehouseSelections: warehouseSelections,
    };

    this.reportService.updateOrder(formattedRequest).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Order updated successfully.',
          });
          this.removedItems.set([]); // Temizle
          this.showDetail.set(false);
          this.onReport(); // Listeyi yenile
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'Failed to update order.',
          });
        }
      },
      error: () => {
        this.saving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to update order.',
        });
      },
    });
  }

  // --- Ürün Ekleme/Güncelleme/Silme İşlemleri ---

  /**
   * Ürün seçici modalını açar
   */
  onAddNewProduct() {
    this.showProductSelector.set(true);
    this.productSelectorPage.set(0);
    this.loadAvailableProducts();
  }

  /**
   * Seçilebilir ürünleri API'den getirir
   */
  loadAvailableProducts() {
    this.productSelectorLoading.set(true);
    this.productService
      .getVendorProducts({
        pageNumber: this.productSelectorPage() + 1,
        pageSize: this.productSelectorRows(),
        searchTerm: this.productSearchTerm(),
      })
      .subscribe({
        next: (res) => {
          this.availableProducts.set(res.data);
          this.productSelectorTotalCount.set(res.totalCount);
          this.productSelectorLoading.set(false);
        },
        error: () => {
          this.productSelectorLoading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load products.',
          });
        },
      });
  }

  /**
   * Ürün arama değiştiğinde
   */
  onProductSearch() {
    this.productSelectorPage.set(0);
    this.loadAvailableProducts();
  }

  /**
   * Ürün seçildiğinde listeye ekle (Frontend tarafında)
   */
  onSelectProduct(product: ProductDto) {
    const order = this.selectedOrder();
    if (!order) return;

    // Zaten varsa uyarı ver veya ekleme
    if (this.detailData().some((d) => d.productCode === product.sku)) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'This product is already in the list.',
      });
      return;
    }

    const newItem: OrderDetailItem = {
      id: 0,
      productCode: product.sku,
      productName: product.name,
      picture: product.imageUrl || undefined,
      qty: 1,
      amount: product.purchasePrice,
      total: product.purchasePrice,
    };

    this.detailData.update((prev) => [...prev, newItem]);
    this.showProductSelector.set(false);
  }

  /**
   * Tek bir ürün kalemini veri tabanına kaydeder
   */
  onSaveProduct(item: OrderDetailItem) {
    const order = this.selectedOrder();
    if (!order) return;

    const request: UpdateOrderItemRequest = {
      orderId: order.orderId,
      productCode: item.productCode,
      qty: item.qty,
      amount: item.amount,
      notes: item.notes,
    };

    this.reportService.updateOrderItem(request).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `Product ${item.productCode} saved.`,
          });

          // Orijinal hali güncelle ki Save butonu tekrar pasif olsun
          this.originalProductStates.update((prev) => ({
            ...prev,
            [item.productCode]: JSON.stringify({
              qty: item.qty,
              amount: item.amount,
              notes: item.notes || '',
            }),
          }));

          // this.loadOrderDetails(); // Artık listeyi tamamen yenilemeye gerek olmayabilir, durumu güncelledik
          this.onReport(); // Ana listeyi de yenile (toplamlar için)
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'Failed to save product.',
          });
        }
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to save product.',
        });
      },
    });
  }

  /**
   * Ürünü listeden (ve veri tabanından) çıkarır
   */
  onRemoveProduct(item: OrderDetailItem) {
    // Veritabanından hemen silme, local listeye ekle
    this.removedItems.update((prev) => [...prev, item]);

    // Mevcut görünümden kaldır
    this.detailData.update((items) => items.filter((p) => p.productCode !== item.productCode));
    this.detailTotalCount.update((count) => Math.max(0, count - 1));

    this.messageService.add({
      severity: 'info',
      summary: 'Success',
      detail: `${item.productCode} will be removed upon Save.`,
    });
  }

  onProductSelectorPageChange(event: any) {
    this.productSelectorPage.set(event.page);
    this.productSelectorRows.set(event.rows);
    this.loadAvailableProducts();
  }

  /**
   * Ürün bilgilerinde değişiklik olup olmadığını kontrol eder
   */
  isProductChanged(product: OrderDetailItem): boolean {
    const originalState = this.originalProductStates()[product.productCode];
    if (!originalState) return true;

    const currentState = JSON.stringify({
      qty: product.qty,
      amount: product.amount,
      notes: product.notes || '',
    });

    return originalState !== currentState;
  }

  /**
   * Ürün değerini günceller ve detailData signal'ini yeniden tetikler
   * Bu sayede editTotals computed değeri otomatik yeniden hesaplanır
   * @param product Güncellenecek ürün
   * @param field Güncellenecek alan (qty veya amount)
   * @param value Yeni değer
   */
  onProductValueChange(product: OrderDetailItem, field: 'qty' | 'amount', value: number) {
    product[field] = value;
    // Signal'i yeniden tetiklemek için array'i yeniden oluştur
    this.detailData.update((items) => [...items]);
  }

  /**
   * Ürün miktarını artırır veya azaltır ve detailData signal'ini yeniden tetikler
   * @param product Güncellenecek ürün
   * @param delta Değişim miktarı (-1 veya +1)
   */
  onProductQtyChange(product: OrderDetailItem, delta: number) {
    product.qty = Math.max(0, product.qty + delta);
    // Signal'i yeniden tetiklemek için array'i yeniden oluştur
    this.detailData.update((items) => [...items]);
  }

  getSeverity(status: string) {
    return getOrderStatusSeverity(status);
  }

  private normalizeStatusValue(status: string): string {
    if (!status) return '';

    const options = this.editStatusOptions();

    const direct = options.find((opt) => opt.value === status);
    if (direct) return direct.value;

    const byLabel = options.find((opt) => opt.label.toLowerCase() === status.toLowerCase());
    if (byLabel) return byLabel.value;

    const simplify = (value: string) => value.toLowerCase().replace(/[^a-z0-9]/g, '');
    const target = simplify(status);

    const bySimplifiedValue = options.find((opt) => simplify(opt.value) === target);
    if (bySimplifiedValue) return bySimplifiedValue.value;

    const bySimplifiedLabel = options.find((opt) => simplify(opt.label) === target);
    if (bySimplifiedLabel) return bySimplifiedLabel.value;

    return status;
  }

  showPrintPreview = signal(false);
  printLoading = signal(false);
  // Proforma Invoice indirme yükleniyor durumu
  proformaLoading = signal(false);
  invoiceLoading = signal(false);

  /**
   * Seçili sipariş için Invoice Excel dosyasını indirir
   */
  onInvoice() {
    const order = this.selectedOrder();
    if (!order) return;

    this.invoiceLoading.set(true);
    this.reportService.getInvoice(order.orderId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Invoice_${order.orderId}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.invoiceLoading.set(false);

        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Invoice generated successfully.',
        });
      },
      error: () => {
        this.invoiceLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'An error occurred while generating the invoice.',
        });
      },
    });
  }

  /**
   * Seçili sipariş için Proforma Invoice Excel dosyasını indirir
   */
  onProformaInvoice() {
    const order = this.selectedOrder();
    if (!order) return;

    this.proformaLoading.set(true);
    this.reportService.getProformaInvoice(order.orderId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Proforma_Invoice_${order.orderId}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.proformaLoading.set(false);

        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Proforma invoice generated successfully.',
        });
      },
      error: () => {
        this.proformaLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'An error occurred while generating the proforma invoice.',
        });
      },
    });
  }

  onPrint() {
    const order = this.selectedOrder();
    if (!order) return;

    this.printLoading.set(true);
    // Tüm ürünleri çekmek için yüksek bir limit kullanalım
    const request: OrderDetailRequest = {
      orderId: order.orderId,
      brand: order.brand,
      pageNumber: 1,
      pageSize: 1000, // Tüm ürünleri kapsayacak şekilde
    };

    this.reportService.getOrderDetails(request).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          // 1. API'den gelenlerden silinmişleri çıkar
          let allItems = res.data.data.filter(
            (item: any) => !this.removedItems().some((r) => r.productCode === item.productCode),
          );

          // 2. Localde eklenmiş ama henüz kaydedilmemiş (id: 0) olanları ekle
          const localAdds = this.detailData().filter(
            (d) => d.id === 0 && !allItems.some((a: any) => a.productCode === d.productCode),
          );

          this.printData.set([...localAdds, ...allItems]);
          this.printLoading.set(false);
          // DOM'un güncellenmesi için çok kısa bir süre bekleyip yazdır
          setTimeout(() => this.onActualPrint(), 50);
        } else {
          this.printLoading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'Failed to load products for printing.',
          });
        }
      },
      error: () => {
        this.printLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load all products for printing.',
        });
      },
    });
  }

  onActualPrint() {
    const printContent = document.getElementById('master-form-print-section');
    if (!printContent) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Print content not found.',
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
      const baseUrl = document.location.origin + document.location.pathname;
      const links = Array.from(document.head.querySelectorAll('link[rel="stylesheet"]'))
        .map((link) => {
          const newLink = link.cloneNode(true) as HTMLLinkElement;
          if (link instanceof HTMLLinkElement && link.href) {
            newLink.href = link.href;
          }
          return newLink.outerHTML;
        })
        .join('');

      const styles = Array.from(document.head.querySelectorAll('style'))
        .map((style) => style.outerHTML)
        .join('');

      doc.open();
      doc.write(`
        <!DOCTYPE html>
        <html>
          <head>
            <base href="${baseUrl}">
            <title>Master Form Print</title>
            <meta charset="utf-8">
            ${links}
            ${styles}
            <style>
              @page { size: A4; margin: 10mm; }
              body { 
                margin: 0; 
                padding: 0; 
                background-color: white !important; 
                -webkit-print-color-adjust: exact; 
                print-color-adjust: exact;
                font-family: 'Inter', sans-serif;
              }
              .no-print { display: none !important; }
              app-order-master-form-print { display: block; width: 100%; }
              /* Iframe içinde görsellerin yüklendiğinden emin olmak için ek stil */
              img { max-width: 100%; height: auto; }
            </style>
          </head>
          <body>
            ${printContent.outerHTML}
          </body>
        </html>
      `);
      doc.close();

      iframe.onload = () => {
        setTimeout(() => {
          iframe.contentWindow?.focus();
          iframe.contentWindow?.print();
          setTimeout(() => {
            if (document.body.contains(iframe)) {
              document.body.removeChild(iframe);
            }
          }, 2000);
        }, 1000);
      };
    }
  }

  // --- Packing List Logic ---
  showPackingList = signal(false);
  packingListLoading = signal(false);
  packingListSaving = signal(false);
  packingListItems = signal<PackingListItemDto[]>([]);
  cartons = signal<PackingListCartonDto[]>([]);
  newCarton = signal({
    cartonNo: '',
    netWeight: '',
    grossWeight: '',
    measurements: '00*00*00',
  });

  onPackingList() {
    const order = this.selectedOrder();
    if (!order) return;

    this.showPackingList.set(true);
    this.packingListLoading.set(true);

    this.reportService.getPackingList(order.orderId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.packingListItems.set(res.data.items);
          this.cartons.set(res.data.cartons);
        }
        this.packingListLoading.set(false);
      },
      error: () => {
        this.packingListLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load packing list.',
        });
      },
    });
  }

  onAddCarton() {
    const nc = this.newCarton();
    if (!nc.cartonNo) return;

    const newDto: PackingListCartonDto = {
      id: 0,
      cartonNo: nc.cartonNo,
      netWeight: parseFloat(nc.netWeight) || 0,
      grossWeight: parseFloat(nc.grossWeight) || 0,
      measurements: nc.measurements,
    };

    this.cartons.update((prev) => [...prev, newDto]);
    this.newCarton.set({ cartonNo: '', netWeight: '', grossWeight: '', measurements: '00*00*00' });
  }

  onRemoveCarton(index: number) {
    this.cartons.update((prev) => prev.filter((_, i) => i !== index));
  }

  onSavePackingList() {
    const order = this.selectedOrder();
    if (!order) return;

    this.packingListSaving.set(true);

    const command: SavePackingListCommand = {
      orderId: order.orderId,
      items: this.packingListItems().map((i) => ({
        orderItemId: i.orderItemId,
        cartonNo: i.cartonNo,
        qty: i.qty,
      })),
      cartons: this.cartons(),
    };

    this.reportService.savePackingList(command).subscribe({
      next: (res) => {
        this.packingListSaving.set(false);
        if (res.success && res.data) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Packing list saved.',
          });
          this.onPackingList(); // Reload to get IDs
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'Failed to save packing list.',
          });
        }
      },
      error: () => {
        this.packingListSaving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to save packing list.',
        });
      },
    });
  }
}


