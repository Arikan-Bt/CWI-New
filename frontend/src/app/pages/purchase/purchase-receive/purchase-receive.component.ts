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
import { DialogModule } from 'primeng/dialog';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { FluidModule } from 'primeng/fluid';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { FileUploadModule } from 'primeng/fileupload';
import { MessageService } from 'primeng/api';
import { FullScreenModalComponent } from '../../../shared/components/full-screen-modal/full-screen-modal.component';
import {
  PurchaseOrderService,
  PurchaseOrderDto,
  PurchaseOrderItemDto,
} from '../../../core/services/purchase-order.service';
import { InventoryService, WarehouseDto } from '../../../core/services/inventory.service';
import { finalize } from 'rxjs';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-purchase-receive',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    DatePickerModule,
    SelectModule,
    TooltipModule,
    FluidModule,
    FullScreenModalComponent,
    InputNumberModule,
    InputTextModule,
    DialogModule,
    TextareaModule,
    FileUploadModule,
  ],
  template: `
    <div class="purchase-receive-page">
      <app-full-screen-modal [(visible)]="isModalOpen">
        <div header class="text-xl font-bold">Purchase Receive Detail</div>

        <!-- Modal Content -->
        <div class="flex flex-col gap-6 p-4">
          <!-- Detail Table -->
          <div class="overflow-hidden">
            <p-table
              [value]="invoiceLines()"
              styleClass="p-datatable-sm custom-modal-table"
              [rowHover]="true"
              [loading]="modalLoading()"
            >
              <ng-template pTemplate="header">
                <tr>
                  <th colspan="2" class="header-group-title text-lg">ORDER DETAILS</th>
                  <th colspan="5" class="header-group-title text-lg text-center">ORDER</th>
                  <th colspan="2" class="header-group-title text-lg text-center">INVOICE</th>
                </tr>
                <!-- Sub Header Information Row (Order Ref & Date) -->
                <tr class="bg-gray-50 dark:bg-surface-900" *ngIf="selectedOrder()">
                  <td
                    colspan="2"
                    class="py-3 px-3 font-semibold text-surface-700 dark:text-surface-0/80"
                  >
                    Order Ref No :
                    <span class="text-primary-500">{{ selectedOrder()?.orderRefNo }}</span>
                  </td>
                  <td
                    colspan="7"
                    class="py-3 px-3 font-semibold text-surface-700 dark:text-surface-0/80"
                  >
                    Order Date : {{ selectedOrder()?.date | date: 'd.MM.yyyy' }}
                  </td>
                </tr>
                <tr>
                  <th style="width: 15%">Product Code</th>
                  <th style="width: 25%">Product Name</th>
                  <th style="width: 10%" class="text-center">QTY (PCS)</th>
                  <th style="width: 10%" class="text-right">Unit Price</th>
                  <th style="width: 10%" class="text-right">Amount</th>
                  <th style="width: 10%" class="text-center">Receive</th>
                  <th style="width: 10%" class="text-center">Balance</th>
                  <th style="width: 100px" class="text-center">QTY (PCS)</th>
                  <th style="width: 100px" class="text-right">Unit Price</th>
                </tr>
              </ng-template>
              <ng-template pTemplate="body" let-line>
                <tr>
                  <td class="font-bold">{{ line.productCode }}</td>
                  <td>{{ line.productName }}</td>
                  <td class="text-center">{{ line.orderQty }}</td>
                  <td class="text-right">{{ line.orderUnitPrice | number: '1.2-2' }}</td>
                  <td class="text-right">
                    {{ line.orderAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                  </td>
                  <td class="text-center">{{ line.receive }}</td>
                  <td class="text-center">{{ line.balance }}</td>
                  <!-- Editable Fields -->
                  <td class="p-1 text-center">
                    @if (isReadOnly()) {
                      <span class="text-sm font-medium">{{
                        line.invoiceQty | number: '1.0-0'
                      }}</span>
                    } @else {
                      <p-inputnumber
                        [(ngModel)]="line.invoiceQty"
                        (ngModelChange)="onLineChange()"
                        locale="en-US"
                        mode="decimal"
                        [minFractionDigits]="0"
                        [maxFractionDigits]="0"
                        inputStyleClass="w-full text-center text-sm p-1"
                        class="w-full"
                      >
                      </p-inputnumber>
                    }
                  </td>
                  <td class="p-1 text-right">
                    @if (isReadOnly()) {
                      <span class="text-sm font-medium">{{
                        line.invoiceUnitPrice | number: '1.2-2'
                      }}</span>
                    } @else {
                      <p-inputnumber
                        [(ngModel)]="line.invoiceUnitPrice"
                        (ngModelChange)="onLineChange()"
                        mode="currency"
                        currency="USD"
                        locale="en-US"
                        inputStyleClass="w-full text-right text-sm p-1"
                        class="w-full"
                      >
                      </p-inputnumber>
                    }
                  </td>
                </tr>
              </ng-template>
              <ng-template pTemplate="footer">
                <tr
                  class="text-surface-700 dark:text-surface-0/80 font-bold bg-surface-50 dark:bg-surface-800"
                >
                  <td colspan="1" class="text-lg">TOTAL</td>
                  <td colspan="1"></td>

                  <!-- Order Totals -->
                  <td colspan="3" class="text-right pr-4">
                    <div class="flex flex-col items-end gap-1">
                      <span>TOTAL QTY: {{ totalOrderQty() | number: '1.2-2' }}</span>
                      <span
                        >TOTAL AMOUNT:
                        {{ totalOrderAmount() | currency: 'USD' : 'symbol' : '1.2-2' }}</span
                      >
                    </div>
                  </td>

                  <!-- Receive / Balance Totals -->
                  <td colspan="2" class="text-right pr-4">
                    <div class="flex flex-col items-end gap-1">
                      <span>Total Receive Qty: {{ totalReceiveQty() | number: '1.2-2' }}</span>
                      <span>Total Balance Qty: {{ totalBalanceQty() | number: '1.2-2' }}</span>
                    </div>
                  </td>

                  <!-- Invoice Totals -->
                  <td colspan="2" class="text-right pr-2">
                    <div
                      class="flex flex-col items-end gap-1 text-primary-600 dark:text-primary-400"
                    >
                      <span>TOTAL QTY: {{ totalInvoiceQty() | number: '1.0-2' }}</span>
                      <span
                        >TOTAL AMOUNT:
                        {{ totalInvoiceAmount() | currency: 'USD' : 'symbol' : '1.2-2' }}</span
                      >
                    </div>
                  </td>
                </tr>
              </ng-template>
            </p-table>
          </div>
        </div>

        <div footer class="flex justify-end items-center gap-3 w-full">
          <p-button
            label="Cancel"
            [outlined]="true"
            severity="secondary"
            styleClass="px-8"
            (onClick)="isModalOpen.set(false)"
          ></p-button>

          <!-- Hidden File Input -->
          <input
            type="file"
            #fileInput
            style="display: none"
            (change)="onFileChange($event)"
            accept=".xlsx, .xls"
          />

          @if (!isReadOnly()) {
            <p-button
              label="Import Excel"
              icon="pi pi-file-excel"
              [outlined]="true"
              severity="success"
              styleClass="px-8"
              (onClick)="fileInput.click()"
            ></p-button>

            <p-button
              label="Download Template"
              icon="pi pi-download"
              [outlined]="true"
              severity="info"
              styleClass="px-8"
              (onClick)="onDownloadTemplate()"
            ></p-button>

            <p-button
              label="Save Invoice"
              styleClass="btn-action-red px-8"
              (onClick)="onSaveInvoice()"
              [loading]="saveLoading()"
            ></p-button>
          }
        </div>
      </app-full-screen-modal>

      <!-- Invoice Entry Dialog -->
      <p-dialog
        [(visible)]="isInvoiceDialogVisible"
        [modal]="true"
        [style]="{ width: '80%' }"
        header="INVOICE NUMBER"
        [draggable]="false"
        [resizable]="false"
        styleClass="custom-invoice-dialog"
      >
        <ng-template pTemplate="header">
          <div class="flex items-center justify-between w-full">
            <span class="text-xl font-bold text-surface-600 dark:text-surface-0/80 uppercase">
              INVOICE NUMBER
            </span>
          </div>
        </ng-template>

        <div class="flex flex-col gap-4 mt-2">
          <!-- Card 1: Invoice Information -->
          <div class="card p-4 flex flex-col gap-6 md:flex-row">
            <div class="flex flex-col gap-2 flex-1">
              <label for="invoiceNumber" class="font-bold text-surface-600 dark:text-surface-0/80">
                Write Invoice Number
              </label>
              <input pInputText id="invoiceNumber" [(ngModel)]="invoiceNumber" class="w-full" />
            </div>

            <div class="flex flex-col gap-2 flex-1">
              <label for="invoiceDate" class="font-bold text-surface-600 dark:text-surface-0/80">
                Invoice Date
              </label>
              <p-datepicker
                id="invoiceDate"
                [(ngModel)]="invoiceDate"
                parserFormatter="dd.mm.yy"
                dateFormat="dd.mm.yy"
                [showIcon]="true"
                styleClass="w-full"
                inputStyleClass="w-full"
                appendTo="body"
              ></p-datepicker>
            </div>
          </div>

          <!-- Card 2: Additional Invoice Info (Vendor, Amount, Desc, File) -->
          <div class="card p-4 flex flex-col gap-4">
            <h3 class="font-bold text-lg">Invoice Details</h3>
            <p-fluid>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <!-- Vendor (Read-Only) -->
                <div class="flex flex-col gap-2">
                  <label for="vendor" class="font-medium text-surface-600 dark:text-surface-0/80"
                    >Vendor</label
                  >
                  <p-select
                    id="vendor"
                    [options]="[
                      { label: selectedOrder()?.customerSvc, value: selectedOrder()?.customerSvc },
                    ]"
                    [ngModel]="selectedOrder()?.customerSvc"
                    optionLabel="label"
                    optionValue="value"
                    [disabled]="true"
                    styleClass="w-full opacity-100"
                    placeholder="Vendor"
                  ></p-select>
                </div>

                <!-- Currency (Read-Only) -->
                <div class="flex flex-col gap-2">
                  <label for="currency" class="font-medium text-surface-600 dark:text-surface-0/80"
                    >Currency</label
                  >
                  <input
                    pInputText
                    id="currency"
                    value="USD"
                    [disabled]="true"
                    class="w-full opacity-100"
                  />
                </div>

                <!-- Amount (Read-Only) -->
                <div class="flex flex-col gap-2">
                  <label
                    for="invoiceAmount"
                    class="font-medium text-surface-600 dark:text-surface-0/80"
                    >Invoice Amount</label
                  >
                  <p-inputnumber
                    id="invoiceAmount"
                    [ngModel]="totalInvoiceAmount()"
                    mode="currency"
                    currency="USD"
                    locale="en-US"
                    [disabled]="true"
                    inputStyleClass="w-full opacity-100"
                    styleClass="w-full"
                  ></p-inputnumber>
                </div>

                <!-- Description -->
                <div class="md:col-span-2 flex flex-col gap-2">
                  <label
                    for="description"
                    class="font-medium text-surface-600 dark:text-surface-0/80"
                    >Description</label
                  >
                  <textarea
                    id="description"
                    pTextarea
                    [(ngModel)]="invoiceDescription"
                    rows="3"
                    placeholder="Enter description"
                    class="w-full"
                  ></textarea>
                </div>

                <!-- File Upload -->
                <div class="md:col-span-2 flex flex-col gap-2">
                  <label class="font-medium text-surface-600 dark:text-surface-0/80"
                    >Invoice File</label
                  >
                  <p-fileupload
                    mode="advanced"
                    chooseLabel="Choose File"
                    accept="image/*,.pdf"
                    maxFileSize="10000000"
                    (onSelect)="onInvoiceFileSelect($event)"
                    (onRemove)="onInvoiceFileClear()"
                    (onClear)="onInvoiceFileClear()"
                    [showUploadButton]="false"
                    [showCancelButton]="false"
                  >
                    <ng-template
                      pTemplate="content"
                      let-files
                      let-removeFileCallback="removeFileCallback"
                    >
                      @if (files.length === 0) {
                        <div
                          class="flex flex-col items-center justify-center py-6 border-2 border-dashed border-surface-200 dark:border-surface-700 rounded-xl bg-surface-50 dark:bg-surface-900/10"
                        >
                          <i class="pi pi-file-arrow-up text-3xl text-primary mb-2"></i>
                          <span class="text-sm text-muted-color"
                            >Drag & drop or click to select</span
                          >
                        </div>
                      }
                      @for (file of files; track file.name) {
                        <div
                          class="flex items-center justify-between p-3 border rounded-lg bg-surface-0 dark:bg-surface-800 mt-2"
                        >
                          <span class="truncate text-sm">{{ file.name }}</span>
                          <p-button
                            icon="pi pi-times"
                            [text]="true"
                            severity="danger"
                            size="small"
                            (onClick)="removeFileCallback($index)"
                          ></p-button>
                        </div>
                      }
                    </ng-template>
                  </p-fileupload>
                </div>
              </div>
            </p-fluid>
          </div>

          <div class="card p-4">
            <h3 class="font-bold text-lg mb-4">Order Details For Invoice</h3>
            <p-table [value]="filteredInvoiceLines()" styleClass="p-datatable-sm" [rowHover]="true">
              <ng-template pTemplate="header">
                <tr>
                  <th>Product Code</th>
                  <th>Product Name</th>
                  <th class="text-center">Invoice Qty</th>
                  <th>Warehouse</th>
                  <th>Shelf Number</th>
                  <th>Pack List</th>
                  <th>Receiving Date</th>
                </tr>
              </ng-template>
              <ng-template pTemplate="body" let-line>
                <tr>
                  <td class="font-bold">{{ line.productCode }}</td>
                  <td>{{ line.productName }}</td>
                  <td class="text-center">{{ line.invoiceQty }}</td>
                  <td>
                    <p-select
                      [options]="warehouses()"
                      [(ngModel)]="line.warehouseId"
                      optionLabel="name"
                      optionValue="id"
                      placeholder="Select Warehouse"
                      styleClass="w-full"
                      appendTo="body"
                    ></p-select>
                  </td>
                  <td>
                    <input pInputText [(ngModel)]="line.shelfNumber" class="w-full p-1 text-sm" />
                  </td>
                  <td>
                    <input pInputText [(ngModel)]="line.packList" class="w-full p-1 text-sm" />
                  </td>
                  <td>
                    <p-datepicker
                      [(ngModel)]="line.receivingDate"
                      dateFormat="dd.mm.yy"
                      [showIcon]="false"
                      styleClass="w-full"
                      inputStyleClass="w-full p-1 text-sm"
                      appendTo="body"
                    ></p-datepicker>
                  </td>
                </tr>
              </ng-template>
            </p-table>
          </div>
        </div>

        <ng-template pTemplate="footer">
          <div class="flex justify-end gap-2 mt-6">
            <p-button
              label="Close"
              [outlined]="true"
              severity="secondary"
              (onClick)="isInvoiceDialogVisible.set(false)"
            ></p-button>
            <p-button
              label="Save Invoice"
              styleClass="btn-action-red"
              (onClick)="onConfirmSaveInvoice()"
              [loading]="saveLoading()"
            ></p-button>
          </div>
        </ng-template>
      </p-dialog>

      <div class="flex flex-col gap-4">
        <!-- Filtre Paneli -->
        <div class="card p-4">
          <div class="font-semibold text-xl mb-4">Purchase Receive</div>
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
                <th style="width: 120px" pSortableColumn="date">
                  Date <p-sortIcon field="date"></p-sortIcon>
                </th>
                <th pSortableColumn="orderRefNo">
                  <div class="flex items-center gap-2">
                    Order Ref No <p-sortIcon field="orderRefNo"></p-sortIcon>
                    <p-columnFilter type="text" field="orderRefNo" display="menu"></p-columnFilter>
                  </div>
                </th>
                <th pSortableColumn="customerSvc">
                  <div class="flex items-center gap-2">
                    Customer Svc <p-sortIcon field="customerSvc"></p-sortIcon>
                    <p-columnFilter type="text" field="customerSvc" display="menu"></p-columnFilter>
                  </div>
                </th>
                <th class="text-right" style="width: 100px" pSortableColumn="qty">
                  QTY (PCS) <p-sortIcon field="qty"></p-sortIcon>
                </th>
                <th class="text-right" style="width: 120px" pSortableColumn="amount">
                  Amount <p-sortIcon field="amount"></p-sortIcon>
                </th>
                <th class="text-center" style="width: 120px">Detail</th>
                <th class="text-center" style="width: 150px">Status</th>
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
                <td class="text-center">
                  <div class="flex justify-center gap-1">
                    <p-button
                      icon="pi pi-pencil"
                      [text]="true"
                      [rounded]="true"
                      severity="danger"
                      (onClick)="onAction('Edit', order)"
                      pTooltip="Edit Invoice"
                    ></p-button>
                    <p-button
                      icon="pi pi-search"
                      [text]="true"
                      [rounded]="true"
                      severity="danger"
                      (onClick)="onAction('View', order)"
                      pTooltip="View"
                    ></p-button>
                  </div>
                </td>
                <td>
                  <div class="flex items-center">
                    <p-select
                      [(ngModel)]="order.status"
                      [options]="statusOptions"
                      optionLabel="label"
                      optionValue="value"
                      styleClass="w-full text-xs"
                      appendTo="body"
                      (onChange)="onUpdateStatus(order)"
                    ></p-select>
                  </div>
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
                <td colspan="2"></td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="7" class="text-center p-8 text-surface-400">
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
        .purchase-receive-page {
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
export class PurchaseReceive implements OnInit {
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
  modalLoading = signal(false);
  saveLoading = signal(false);

  // Invoice Dialog State
  isInvoiceDialogVisible = signal(false);
  invoiceNumber = signal('');
  invoiceDate = signal<Date | null>(null);
  invoiceDescription = signal('');

  selectedInvoiceFile: File | null = null;
  vendorOptions = signal<{ label: string; value: string }[]>([]);

  // Modal State
  isModalOpen = signal(false);
  isReadOnly = signal(false);

  selectedOrder = signal<PurchaseOrderDto | null>(null);
  invoiceLines = signal<PurchaseOrderItemDto[]>([]);

  // Computed Totals for Invoice Modal
  totalOrderQty = computed(() =>
    this.invoiceLines().reduce((acc, line) => acc + (line.orderQty || 0), 0),
  );
  totalOrderAmount = computed(() =>
    this.invoiceLines().reduce((acc, line) => acc + (line.orderAmount || 0), 0),
  );
  totalReceiveQty = computed(() =>
    this.invoiceLines().reduce((acc, line) => acc + (line.receive || 0), 0),
  );
  totalBalanceQty = computed(() =>
    this.invoiceLines().reduce((acc, line) => acc + (line.balance || 0), 0),
  );

  // Invoice totals based on editable fields
  totalInvoiceQty = computed(() =>
    this.invoiceLines().reduce((acc, line) => acc + (line.invoiceQty || 0), 0),
  );
  totalInvoiceAmount = computed(() =>
    this.invoiceLines().reduce(
      (acc, line) => acc + (line.invoiceQty || 0) * (line.invoiceUnitPrice || 0),
      0,
    ),
  );

  // Fatura modalı için sadece miktar girilen satırları filtreleyen computed signal
  filteredInvoiceLines = computed(() =>
    this.invoiceLines().filter((line) => (line.invoiceQty || 0) > 0),
  );

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

  // Durum seçenekleri
  statusOptions = [
    { label: 'Active', value: 'Active' },
    { label: 'Inactive', value: 'Inactive' },
  ];

  orders = signal<PurchaseOrderDto[]>([]);
  totalRecords = signal(0);
  warehouses = signal<WarehouseDto[]>([]);

  ngOnInit() {
    this.loadWarehouses();
    this.loadVendors();
  }

  loadVendors() {
    // PurchaseOrderService doesn't have createPurchaseOrder wrapper but has getVendors.
    // We need to cast it or add it if missing in the viewed snippet, but previous view_file of service showed getVendors.
    this.purchaseService.getVendors(true).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.vendorOptions.set(res.data);
        }
      },
    });
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

    // Filters
    const filterOrderRefNo = event?.filters?.orderRefNo?.[0]?.value;
    const filterCustomerSvc = event?.filters?.customerSvc?.[0]?.value;

    // Sort
    const sortField = event?.sortField;
    const sortOrder = event?.sortOrder;

    const request = {
      startDate: this.filters().startDate ?? undefined,
      endDate: this.filters().endDate ?? undefined,
      page,
      pageSize,
      sortField,
      sortOrder,
      filterOrderRefNo,
      filterCustomerSvc,
    };

    this.purchaseService
      .getOrders(request)
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

  /**
   * Edit veya View aksiyonları
   */
  onAction(type: 'Edit' | 'View', order: PurchaseOrderDto) {
    this.openModal(order, type === 'View');
  }

  /**
   * Modal Açma ve Veri Hazırlama
   */
  openModal(order: PurchaseOrderDto, readOnly: boolean = false) {
    this.isReadOnly.set(readOnly);
    this.selectedOrder.set(order);
    this.isModalOpen.set(true);
    this.modalLoading.set(true);
    this.invoiceLines.set([]); // Modal açılmadan önce eski verileri temizle

    const orderId = order.id?.toString();
    if (!orderId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Invalid order ID.',
      });
      this.isModalOpen.set(false);
      this.modalLoading.set(false);
      return;
    }

    this.purchaseService
      .getOrderDetails(orderId)
      .pipe(finalize(() => this.modalLoading.set(false)))
      .subscribe({
        next: (data) => {
          if (data.success && data.data) {
            this.invoiceLines.set(data.data.items || []);
          } else {
            this.messageService.add({
              severity: 'warn',
              summary: 'Warning',
              detail: 'Order details returned empty.',
            });
          }
        },
        error: (err) => {
          console.error('Order detail error:', err);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail:
              err.error?.error || err.message || 'An error occurred while loading order details.',
          });
          this.isModalOpen.set(false);
        },
      });
  }

  /**
   * Satır değiştiğinde çalışır (Hesaplamaları tetikler)
   */
  onLineChange() {
    this.invoiceLines.update((lines) => [...lines]);
  }

  /**
   * Faturayı Kaydet (Dialog Aç)
   */
  onSaveInvoice() {
    if (this.totalInvoiceQty() <= 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter quantity for at least one item.',
      });
      return;
    }
    this.invoiceNumber.set('');
    this.invoiceDate.set(new Date()); // Default to today
    this.invoiceDescription.set('');
    this.selectedInvoiceFile = null;
    this.isInvoiceDialogVisible.set(true);
  }

  onInvoiceFileSelect(event: any) {
    this.selectedInvoiceFile = event.files[0];
  }

  onInvoiceFileClear() {
    this.selectedInvoiceFile = null;
  }

  /**
   * Fatura Onay ve Kayıt
   */
  onConfirmSaveInvoice() {
    const order = this.selectedOrder();
    const invNum = this.invoiceNumber();
    const invDate = this.invoiceDate();

    if (!order) return;
    if (!invNum) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter an invoice number.',
      });
      return;
    }
    if (!invDate) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select an invoice date.',
      });
      return;
    }

    const linesWithoutWarehouse = this.filteredInvoiceLines().filter((l) => !l.warehouseId);
    if (linesWithoutWarehouse.length > 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a warehouse for all products.',
      });
      return;
    }

    this.saveLoading.set(true);
    this.purchaseService
      .saveInvoice(order.id, invNum, invDate, this.filteredInvoiceLines(), this.selectedInvoiceFile)
      .pipe(finalize(() => this.saveLoading.set(false)))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice created successfully.',
          });
          this.isInvoiceDialogVisible.set(false);
          this.isModalOpen.set(false);
          this.loadOrders(); // Listeyi yenile
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while saving the invoice.',
          });
        },
      });
  }

  /**
   * Durum güncelleme işlemi
   */
  onUpdateStatus(order: PurchaseOrderDto) {
    this.purchaseService.updateStatus(order.id, order.status).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Order status for ${order.orderRefNo} updated to "${order.status}".`,
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'An error occurred while updating status.',
        });
      },
    });
  }

  /**
   * Excel Dosyası Yükleme ve İşleme
   */
  onFileChange(event: any) {
    const target: DataTransfer = <DataTransfer>event.target;
    if (target.files.length !== 1) {
      return;
    }

    const reader: FileReader = new FileReader();
    reader.onload = (e: any) => {
      const bstr: string = e.target.result;
      const wb: XLSX.WorkBook = XLSX.read(bstr, { type: 'binary' });

      // İlk sayfayı al
      const wsname: string = wb.SheetNames[0];
      const ws: XLSX.WorkSheet = wb.Sheets[wsname];

      // JSON'a çevir
      const data = XLSX.utils.sheet_to_json(ws);

      this.processExcelData(data);

      // Inputu temizle ki aynı dosyayı tekrar seçebilsin
      event.target.value = '';
    };
    reader.readAsBinaryString(target.files[0]);
  }

  processExcelData(data: any[]) {
    // Mevcut satırları al
    let currentLines = [...this.invoiceLines()];
    let updatedCount = 0;
    const warehouses = this.warehouses();

    data.forEach((row: any) => {
      // Excel kolon isimleri eşleşmesi (Büyük/küçük harf duyarlı olabilir, o yüzden güvenli erişim deneyelim)
      // Beklenen format: SKU NO, QTY, PRICE, WAREHOUSE

      // Normalize keys to upper case for easier matching
      const normalizedRow: any = {};
      Object.keys(row).forEach((key) => {
        normalizedRow[key.trim().toUpperCase()] = row[key];
      });

      const sku = normalizedRow['SKU NO'];

      if (sku) {
        // Ürünü bul
        const lineIndex = currentLines.findIndex(
          (l) => l.productCode?.trim().toUpperCase() === sku.toString().trim().toUpperCase(),
        );

        if (lineIndex !== -1) {
          const qty = parseFloat(normalizedRow['QTY']);
          const price = parseFloat(normalizedRow['UNIT PRICE'] || normalizedRow['PRICE']);
          const warehouseName = normalizedRow['WAREHOUSE'];
          const shelfNumber = normalizedRow['SHELF NUMBER1'];
          const packList = normalizedRow['PACK LIST'];
          const receivingDateStr = normalizedRow['RECEIVING DATE'];

          // Değerleri güncelle
          if (!isNaN(qty)) {
            currentLines[lineIndex] = { ...currentLines[lineIndex], invoiceQty: qty };
          }

          if (!isNaN(price)) {
            currentLines[lineIndex] = { ...currentLines[lineIndex], invoiceUnitPrice: price };
          }

          if (warehouseName) {
            const matchedWarehouse = warehouses.find(
              (w) => w.name?.trim().toUpperCase() === warehouseName.toString().trim().toUpperCase(),
            );
            if (matchedWarehouse) {
              currentLines[lineIndex] = {
                ...currentLines[lineIndex],
                warehouseId: matchedWarehouse.id,
              };
            }
          }

          if (shelfNumber) {
            currentLines[lineIndex] = {
              ...currentLines[lineIndex],
              shelfNumber: shelfNumber.toString(),
            };
          }

          if (packList) {
            currentLines[lineIndex] = { ...currentLines[lineIndex], packList: packList.toString() };
          }

          if (receivingDateStr) {
            let dateVal: Date | null = null;

            if (typeof receivingDateStr === 'number') {
              // Use XLSX SSF to parse serial date safely to {y, m, d}
              const dateInfo = XLSX.SSF.parse_date_code(receivingDateStr);
              if (dateInfo) {
                // Create Local Date at Noon to avoid timezone shifts
                dateVal = new Date(dateInfo.y, dateInfo.m - 1, dateInfo.d, 12, 0, 0);
              }
            } else if (receivingDateStr instanceof Date) {
              dateVal = receivingDateStr;
            } else {
              const strVal = receivingDateStr.toString().trim();
              // Try d.m.y or d/m/y
              const parts = strVal.split(/[./-]/);
              if (parts.length === 3) {
                const d = parseInt(parts[0]);
                const m = parseInt(parts[1]);
                const y = parseInt(parts[2]);
                if (!isNaN(d) && !isNaN(m) && !isNaN(y)) {
                  const fullYear = y < 100 ? 2000 + y : y;
                  // Set to Noon
                  dateVal = new Date(fullYear, m - 1, d, 12, 0, 0);
                }
              } else {
                // Fallback
                const d = new Date(strVal);
                if (!isNaN(d.getTime())) {
                  dateVal = d;
                }
              }
            }

            if (dateVal && !isNaN(dateVal.getTime())) {
              currentLines[lineIndex] = { ...currentLines[lineIndex], receivingDate: dateVal };
            }
          }

          updatedCount++;
        }
      }
    });

    if (updatedCount > 0) {
      this.invoiceLines.set(currentLines);
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: `${updatedCount} items updated from Excel file.`,
      });
    } else {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'No matching items found in Excel file or format is invalid.',
      });
    }
  }

  /**
   * Excel Şablonu İndir
   */
  onDownloadTemplate() {
    // Excel başlıkları
    const headers = [
      'SUPPLIER',
      'SKU NO',
      'QTY',
      'UNIT PRICE',
      'CURRENCY',
      'WAREHOUSE',
      'SHELF NUMBER1',
      'PACK LIST',
      'RECEIVING DATE',
    ];

    // Boş veri ama başlıklarla birlikte sheet oluştur
    const ws: XLSX.WorkSheet = XLSX.utils.aoa_to_sheet([headers]);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Template');

    // Dosyayı indir
    XLSX.writeFile(wb, 'PurchaseReceiveTemplate.xlsx');
  }
}
