import { Component, ChangeDetectionStrategy, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { FluidModule } from 'primeng/fluid';
import { MessageService } from 'primeng/api';
import { PurchasingService } from '../../../core/services/purchasing.service';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { environment } from '../../../../environments/environment';

/**
 * Satıcı Bakiyesi (Vendor Balance) öğesi için arayüz tanımı
 */
export interface VendorBalanceItem {
  currAccCode: string;
  currAccDescription: string;
  invoiceNo: string;
  invoiceDate: Date;
  currency: string;
  description: string;
  totalAmount: number;
  paymentTotal: number;
  balance: number;
  invoiceFilePath?: string;
  paymentFilePath?: string;
  paymentHistory?: PaymentHistoryItem[];
}

/**
 * Ödeme Geçmişi Kaydı
 */
export interface PaymentHistoryItem {
  id: number;
  date: Date;
  amount: number;
  currency: string;
  receiptNumber?: string;
  filePath?: string;
  description?: string;
  referenceNumber?: string;
}

@Component({
  selector: 'app-vendor-balance',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePickerModule,
    ButtonModule,
    TableModule,
    FluidModule,
    TooltipModule,
    DialogModule,
    FileUploadModule,
    SelectModule,
    InputTextModule,
    InputNumberModule,
    IconFieldModule,
    InputIconModule,
  ],

  template: `
    <div class="vendor-balance-page">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Vendor Balance</div>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
              <!-- Başlangıç Tarihi -->
              <div class="flex flex-col gap-2 md:col-span-1">
                <label for="startDate" class="font-medium text-sm text-surface-600"
                  >Starting Date</label
                >
                <p-datepicker
                  id="startDate"
                  [ngModel]="startDate()"
                  (ngModelChange)="startDate.set($event)"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="30.12.2025"
                ></p-datepicker>
              </div>

              <!-- Bitiş Tarihi -->
              <div class="flex flex-col gap-2 md:col-span-1">
                <label for="endDate" class="font-medium text-sm text-surface-600"
                  >Ending Date</label
                >
                <p-datepicker
                  id="endDate"
                  [ngModel]="endDate()"
                  (ngModelChange)="endDate.set($event)"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="06.01.2026"
                ></p-datepicker>
              </div>

              <!-- Göster Butonu -->
              <div class="flex items-end md:col-span-2">
                <p-button
                  label="Show Details"
                  severity="danger"
                  (onClick)="onShowDetails()"
                  [loading]="loading()"
                  styleClass="w-full md:w-auto ml-auto"
                ></p-button>
              </div>
            </div>
          </p-fluid>
        </div>

        <div class="card p-0 overflow-hidden">
          <p-table
            [value]="balanceItems()"
            [rows]="rows()"
            [paginator]="true"
            [lazy]="true"
            (onLazyLoad)="onShowDetails($event)"
            [totalRecords]="totalRecords()"
            [rowsPerPageOptions]="[10, 20, 50]"
            [showCurrentPageReport]="true"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
            paginatorDropdownAppendTo="body"
            styleClass="p-datatable-gridlines p-datatable-sm"
            [responsiveLayout]="'scroll'"
            (onPage)="rows.set($event.rows)"
            [loading]="loading()"
          >
            <ng-template pTemplate="header">
              <tr>
                <th class="text-xs uppercase bg-surface-50" pSortableColumn="currAccCode">
                  <div class="flex items-center gap-2">
                    CurrAccCode <p-sortIcon field="currAccCode"></p-sortIcon>
                    <p-columnFilter type="text" field="currAccCode" display="menu"></p-columnFilter>
                  </div>
                </th>
                <th class="text-xs uppercase bg-surface-50" pSortableColumn="currAccDescription">
                  <div class="flex items-center gap-2">
                    CurrAccDescription <p-sortIcon field="currAccDescription"></p-sortIcon>
                    <p-columnFilter
                      type="text"
                      field="currAccDescription"
                      display="menu"
                    ></p-columnFilter>
                  </div>
                </th>
                <th class="text-xs uppercase bg-surface-50" pSortableColumn="invoiceNo">
                  <div class="flex items-center gap-2">
                    InvoiceNo <p-sortIcon field="invoiceNo"></p-sortIcon>
                    <p-columnFilter type="text" field="invoiceNo" display="menu"></p-columnFilter>
                  </div>
                </th>
                <th class="text-xs uppercase bg-surface-50" pSortableColumn="invoiceDate">
                  InvoiceDate <p-sortIcon field="invoiceDate"></p-sortIcon>
                </th>
                <th class="text-xs uppercase bg-surface-50">Currency</th>
                <th class="text-xs uppercase bg-surface-50" pSortableColumn="description">
                  <div class="flex items-center gap-2">
                    Description <p-sortIcon field="description"></p-sortIcon>
                    <p-columnFilter type="text" field="description" display="menu"></p-columnFilter>
                  </div>
                </th>
                <th
                  class="text-xs uppercase bg-surface-50 text-right"
                  pSortableColumn="totalAmount"
                >
                  Total Amount <p-sortIcon field="totalAmount"></p-sortIcon>
                </th>
                <th
                  class="text-xs uppercase bg-surface-50 text-right"
                  pSortableColumn="paymentTotal"
                >
                  Payment Total <p-sortIcon field="paymentTotal"></p-sortIcon>
                </th>
                <th class="text-xs uppercase bg-surface-50 text-right" pSortableColumn="balance">
                  Balance <p-sortIcon field="balance"></p-sortIcon>
                </th>
                <th class="text-xs uppercase bg-surface-50 text-center">Vendor Invoice</th>
                <th class="text-xs uppercase bg-surface-50 text-center">Payment Made</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-item>
              <tr>
                <td>{{ item.currAccCode }}</td>
                <td>{{ item.currAccDescription }}</td>
                <td>{{ item.invoiceNo }}</td>
                <td>{{ item.invoiceDate | date: 'dd.MM.yyyy' }}</td>
                <td class="text-center">{{ item.currency }}</td>
                <td class="text-xs">{{ item.description }}</td>
                <td class="text-right">
                  {{ item.totalAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right">
                  {{ item.paymentTotal | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td
                  class="text-right"
                  [ngClass]="{
                    'text-red-500': item.balance > 0,
                    'text-green-500': item.balance <= 0,
                  }"
                >
                  {{ item.balance | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-center">
                  <p-button
                    icon="pi pi-image"
                    [text]="true"
                    [rounded]="true"
                    [severity]="item.invoiceFilePath ? 'danger' : 'secondary'"
                    [disabled]="!item.invoiceFilePath"
                    (onClick)="onViewInvoice(item)"
                    [pTooltip]="item.invoiceFilePath ? 'View Invoice' : 'No Document Attached'"
                  ></p-button>
                </td>
                <td class="text-center">
                  <p-button
                    icon="pi pi-list"
                    [text]="true"
                    [rounded]="true"
                    severity="info"
                    (onClick)="onEditPayment(item)"
                    pTooltip="View/Add Payments"
                  ></p-button>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="11" class="text-center p-4 text-muted-color">
                  No data found. Please use filters.
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>

      <!-- Ödeme Düzenleme Dialogu -->
      <p-dialog
        [(visible)]="isUploadDialogVisible"
        [modal]="true"
        header="Payment Details & History"
        [style]="{ width: '900px' }"
        styleClass="p-fluid"
      >
        <div class="flex flex-col gap-6 mt-2">
          <!-- Ödeme Listesi Tablosu -->
          <div
            class="card p-0 border border-surface-200 dark:border-surface-700 rounded-xl overflow-hidden mb-4"
          >
            <div
              class="bg-surface-50 dark:bg-surface-900 p-3 border-b border-surface-200 dark:border-surface-700 font-semibold"
            >
              Payment History
            </div>
            <p-table [value]="selectedItem()?.paymentHistory || []" styleClass="p-datatable-sm">
              <ng-template pTemplate="header">
                <tr>
                  <th class="text-xs">Date</th>
                  <th class="text-xs">Amount</th>
                  <th class="text-xs">Currency</th>
                  <th class="text-xs">Receipt No</th>
                  <th class="text-xs">Description</th>
                  <th class="text-xs text-center">File</th>
                </tr>
              </ng-template>
              <ng-template pTemplate="body" let-payment>
                <tr>
                  <td class="text-xs">{{ payment.paidAt | date: 'dd.MM.yyyy' }}</td>
                  <td class="text-xs font-semibold">
                    {{ payment.amount | currency: payment.currency : 'symbol' : '1.2-2' }}
                  </td>
                  <td class="text-xs">{{ payment.currency }}</td>
                  <td class="text-xs">{{ payment.referenceNumber }}</td>
                  <td class="text-xs">{{ payment.description }}</td>
                  <td class="text-center">
                    <p-button
                      icon="pi pi-file"
                      [text]="true"
                      [rounded]="true"
                      [severity]="payment.filePath ? 'danger' : 'secondary'"
                      [disabled]="!payment.filePath"
                      (onClick)="onViewAttachment(payment.filePath!)"
                      size="small"
                    ></p-button>
                  </td>
                </tr>
              </ng-template>
            </p-table>
          </div>

          <div class="font-semibold text-lg border-b pb-2">Add New Payment</div>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Vendor Account (Disabled) -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label class="font-medium text-surface-600">Vendor Account</label>
                <input
                  pInputText
                  [value]="selectedItem()?.currAccDescription"
                  [disabled]="true"
                  class="bg-surface-100 dark:bg-surface-800"
                />
              </div>

              <!-- Reference / Invoice No (Disabled) -->
              <div class="flex flex-col gap-2">
                <label class="font-medium text-surface-600">Reference / Invoice No</label>
                <p-iconfield iconPosition="left">
                  <p-inputicon class="pi pi-hashtag"></p-inputicon>
                  <input
                    pInputText
                    [value]="selectedItem()?.invoiceNo"
                    [disabled]="true"
                    class="bg-surface-100 dark:bg-surface-800"
                  />
                </p-iconfield>
              </div>

              <!-- Currency Selection -->
              <div class="flex flex-col gap-2">
                <label for="currency" class="font-medium text-surface-600">Currency</label>
                <p-select
                  id="currency"
                  [options]="currencyOptions"
                  [(ngModel)]="paymentForm().currency"
                  optionLabel="label"
                  optionValue="value"
                  appendTo="body"
                ></p-select>
              </div>

              <!-- Payment Amount -->
              <div class="flex flex-col gap-2">
                <label for="amount" class="font-medium text-surface-600">Payment Amount</label>
                <p-inputnumber
                  id="amount"
                  [(ngModel)]="paymentForm().amount"
                  mode="currency"
                  [currency]="paymentForm().currency || 'USD'"
                  locale="en-US"
                ></p-inputnumber>
              </div>

              <!-- Receipt Number -->
              <div class="flex flex-col gap-2">
                <label for="receipt" class="font-medium text-surface-600"
                  >Receipt / Document No</label
                >
                <p-iconfield iconPosition="left">
                  <p-inputicon class="pi pi-file-edit"></p-inputicon>
                  <input
                    pInputText
                    [(ngModel)]="paymentForm().receiptNumber"
                    placeholder="Enter document number"
                  />
                </p-iconfield>
              </div>

              <!-- Date -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="date" class="font-medium text-surface-600">Transaction Date</label>
                <p-datepicker
                  [(ngModel)]="paymentForm().date"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  appendTo="body"
                ></p-datepicker>
              </div>

              <!-- File Upload -->
              <div class="md:col-span-2 flex flex-col gap-2 mt-2">
                <label class="font-medium text-surface-600">Receipt File</label>
                <p-fileupload
                  mode="advanced"
                  chooseLabel="Choose File"
                  [showUploadButton]="false"
                  [showCancelButton]="false"
                  accept="image/*,.pdf"
                  maxFileSize="10000000"
                  (onSelect)="onFileSelect($event)"
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
                        <i class="pi pi-cloud-upload text-3xl text-primary mb-2"></i>
                        <span class="text-sm font-medium text-muted-color"
                          >Drag and drop receipt file here or click to choose</span
                        >
                      </div>
                    }
                    @for (file of files; track file.name) {
                      <div
                        class="flex items-center gap-4 p-3 border border-surface-border rounded-xl bg-surface-card"
                      >
                        <i class="pi pi-file text-primary"></i>
                        <div class="flex-1 min-w-0">
                          <p class="text-sm font-semibold truncate">{{ file.name }}</p>
                          <p class="text-xs text-muted-color">
                            {{ (file.size / 1024).toFixed(2) }} KB
                          </p>
                        </div>
                        <p-button
                          icon="pi pi-times"
                          [text]="true"
                          severity="danger"
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

        <ng-template pTemplate="footer">
          <p-button
            label="Cancel"
            [text]="true"
            severity="secondary"
            (onClick)="isUploadDialogVisible.set(false)"
          ></p-button>
          <p-button
            label="Save Payment"
            icon="pi pi-plus"
            [loading]="uploadLoading()"
            (onClick)="onUpload()"
          ></p-button>
        </ng-template>
      </p-dialog>
    </div>
  `,
  styles: [
    `
      :host ::ng-deep {
        .p-datepicker {
          width: 100%;
        }
        .p-datatable-gridlines .p-datatable-thead > tr > th {
          border-width: 1px;
          padding: 0.75rem 0.5rem;
          white-space: nowrap;
        }
        .p-datatable-gridlines .p-datatable-tbody > tr > td {
          border-width: 1px;
          padding: 0.5rem;
          font-size: 0.825rem;
        }
        .p-button.p-button-sm {
          font-size: 0.7rem;
          padding: 0.3rem 0.5rem;
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorBalance implements OnInit {
  private messageService = inject(MessageService);
  private purchasingService = inject(PurchasingService);

  // Yükleme durumu
  loading = signal(false);

  // Sayfalamada gösterilecek satır sayısı
  rows = signal(10);

  // Filtreleme verileri - Signals
  startDate = signal<Date | null>(null);
  endDate = signal<Date | null>(null);

  // Tablo verisi
  balanceItems = signal<VendorBalanceItem[]>([]);
  totalRecords = signal(0);

  // Upload Dialog State
  isUploadDialogVisible = signal(false);
  selectedItem = signal<VendorBalanceItem | null>(null);
  uploadLoading = signal(false);
  selectedFile: File | null = null;

  paymentForm = signal({
    vendorCode: '',
    referenceCode: '',
    currency: 'USD',
    amount: null as number | null,
    receiptNumber: '',
    date: new Date(),
  });

  currencyOptions = [
    { label: 'TRY', value: 'TRY' },
    { label: 'USD', value: 'USD' },
    { label: 'EUR', value: 'EUR' },
  ];

  ngOnInit() {
    // Sayfa açıldığında otomatik yükleme yapılmıyor.
  }

  /**
   * Detayları getir butonuna basıldığında (veya sayfalama yapıldığında) çalışır
   */
  onShowDetails(event?: any) {
    this.loading.set(true);

    const page = (event?.first || 0) / (event?.rows || 10) + 1;
    const pageSize = event?.rows || 10;

    const request = {
      startDate: this.startDate() ?? undefined,
      endDate: this.endDate() ?? undefined,
      page,
      pageSize,
      sortField: event?.sortField,
      sortOrder: event?.sortOrder,
    };

    this.purchasingService.getVendorBalanceReport(request).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const data = res.data;
          this.balanceItems.set(data.data);
          this.totalRecords.set(data.totalCount);
          if (page === 1 && data.data.length === 0) {
            this.messageService.add({
              severity: 'info',
              summary: 'Info',
              detail: 'No records found for the selected date range.',
              life: 3000,
            });
          } else if (page === 1) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: `Loaded ${data.data.length} records.`,
              life: 3000,
            });
          }
        } else {
          this.balanceItems.set([]);
        }

        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.title || err.message || 'Failed to load data.',
          sticky: true,
        });
      },
    });
  }

  /**
   * Faturayı görüntüle
   */
  onViewInvoice(item: VendorBalanceItem) {
    if (item.invoiceFilePath) {
      const baseUrl = environment.apiUrl.replace('/api', '');
      window.open(`${baseUrl}/${item.invoiceFilePath}`, '_blank');
    } else {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'No attached document found for this invoice.',
      });
    }
  }

  /**
   * Ödemeyi görüntüle
   */
  onViewPayment(item: VendorBalanceItem) {
    if (item.paymentFilePath) {
      const baseUrl = environment.apiUrl.replace('/api', '');
      window.open(`${baseUrl}/${item.paymentFilePath}`, '_blank');
    } else {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'No attached payment document found for this transaction.',
      });
    }
  }

  /**
   * Ödeme belgesini düzenle/yükle (Ana tablodan tetiklenir)
   */
  onEditPayment(item: VendorBalanceItem) {
    this.selectedItem.set(item);
    this.selectedFile = null;

    // Formu sıfırla/başlat
    this.paymentForm.set({
      vendorCode: item.currAccCode,
      referenceCode: item.invoiceNo,
      currency: item.currency || 'USD',
      amount: null,
      receiptNumber: '',
      date: new Date(),
    });

    this.isUploadDialogVisible.set(true);
  }

  /**
   * Dosya seçildiğinde
   */
  onFileSelect(event: any) {
    if (event.files && event.files.length > 0) {
      this.selectedFile = event.files[0];
    }
  }

  /**
   * Ödeme belgesini yükle
   */
  onUpload() {
    if (!this.selectedItem()) return;

    this.uploadLoading.set(true);
    const item = this.selectedItem()!;
    const form = this.paymentForm();

    if (!form.amount || form.amount <= 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter a valid amount.',
      });
      return;
    }

    this.uploadLoading.set(true);

    const formData = new FormData();
    formData.append('VendorCode', item.currAccCode);
    formData.append('Amount', form.amount.toString());
    formData.append('CurrencyCode', form.currency);
    formData.append('ReferenceNumber', item.invoiceNo);
    formData.append('Description', form.receiptNumber || `Payment for Invoice: ${item.invoiceNo}`);
    formData.append('PaymentDate', form.date.toISOString());

    if (this.selectedFile) {
      formData.append('PaymentFile', this.selectedFile);
    }

    // API Call
    this.purchasingService.createPayment(formData).subscribe({
      next: (res) => {
        this.uploadLoading.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Payment processed successfully.',
            life: 3000,
          });

          // BALANCE GÜNCELLEME MANTIĞI (Frontend simülasyonu)
          const newAmount = form.amount || 0;

          // Ana tablodaki veriyi güncelle
          this.balanceItems.update((items) =>
            items.map((row) => {
              if (row.invoiceNo === item.invoiceNo) {
                const updatedPaymentTotal = row.paymentTotal + newAmount;
                return {
                  ...row,
                  paymentTotal: updatedPaymentTotal,
                  balance: row.totalAmount - updatedPaymentTotal,
                  paymentHistory: [
                    ...(row.paymentHistory || []),
                    {
                      id: 0, // Geçici ID
                      date: form.date,
                      amount: newAmount,
                      currency: form.currency,
                      receiptNumber: form.receiptNumber,
                      description: form.receiptNumber || `Payment for Invoice: ${item.invoiceNo}`,
                      // File path'i hemen gösteremeyiz çünkü sunucudan dönen path'i bilmiyoruz (response body'de yoksa).
                      // Eğer API response path dönüyorsa onu kullanabiliriz. Şimdilik boş geçelim veya simüle edelim.
                      // Backend response 'Id' dönüyor ama path dönmüyor gibi.
                    },
                  ],
                };
              }
              return row;
            }),
          );

          // Update selected item as well to reflect in the open dialog if we keep it open (though we close it here)
          // this.selectedItem.update(...) - but we close dialog below.

          this.isUploadDialogVisible.set(false);
          this.onShowDetails(); // Refresh from server to get correct file paths and IDs
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'Operation failed.',
          });
        }
      },
      error: (err) => {
        this.uploadLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Server error occurred.',
        });
      },
    });
  }

  /**
   * Ek dosya görüntüleme
   */
  onViewAttachment(path: string) {
    const baseUrl = environment.apiUrl.replace('/api', '');
    window.open(`${baseUrl}/${path}`, '_blank');
  }
}
