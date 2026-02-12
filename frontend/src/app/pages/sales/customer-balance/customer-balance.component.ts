import {
  Component,
  ChangeDetectionStrategy,
  signal,
  inject,
  OnInit,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { CardModule } from 'primeng/card';
import { FluidModule } from 'primeng/fluid';

import { MessageService } from 'primeng/api';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { ReportService } from '../../../core/services/report.service';
import { PaymentService } from '../../../core/services/payment.service';
import {
  CustomerBalanceReportItem,
  CustomerReferenceDto,
  CancelledInvoiceOption,
  CreateDebitNoteRequest,
} from '../../../core/models/customer-balance.report.models';
import { finalize } from 'rxjs';

// Ödeme formu için arayüz tanımı
interface PaymentForm {
  customerCode: string | null;
  referenceCode: string | null;
  amount: number | null;
  receiptNumber: string;
  date: Date;
  notes: string;
}

interface DebitNoteForm {
  customerCode: string | null;
  orderId: number | null;
  invoiceNo: string | null;
  amount: number | null;
  date: Date;
  notes: string;
}

@Component({
  selector: 'app-customer-balance',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePickerModule,
    ButtonModule,
    TableModule,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    CardModule,
    FluidModule,

    TagModule,
    ProgressSpinnerModule,
    DialogModule,
    FileUploadModule,
    SelectModule,
    InputNumberModule,
    TextareaModule,
  ],

  template: `
    <div class="customer-balance-page relative z-0">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Customer Balance</div>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6 items-end">
              <!-- Starting Date -->
              <div class="flex flex-col gap-2">
                <label for="startDate" class="font-medium text-sm text-surface-600"
                  >Starting Date</label
                >
                <p-datepicker
                  id="startDate"
                  [ngModel]="startDate()"
                  (ngModelChange)="startDate.set($event)"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="01.01.2024"
                ></p-datepicker>
              </div>

              <!-- Ending Date -->
              <div class="flex flex-col gap-2">
                <label for="endDate" class="font-medium text-sm text-surface-600"
                  >Ending Date</label
                >
                <p-datepicker
                  id="endDate"
                  [ngModel]="endDate()"
                  (ngModelChange)="endDate.set($event)"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="05.01.2026"
                ></p-datepicker>
              </div>

              <!-- Spacer -->
              <div class="hidden md:block"></div>

              <!-- Action Buttons -->
              <div class="flex justify-end gap-2">
                <p-button
                  label="Debit Note"
                  icon="pi pi-file-edit"
                  severity="warn"
                  (onClick)="openDebitNoteDialog()"
                ></p-button>
                <p-button
                  label="Add Payment"
                  icon="pi pi-plus"
                  severity="success"
                  (onClick)="openPaymentDialog()"
                ></p-button>
                <p-button
                  label="Show Details"
                  severity="danger"
                  (onClick)="loadData()"
                  [loading]="isLoading()"
                ></p-button>
              </div>
            </div>
          </p-fluid>
        </div>

        <div class="card p-0 overflow-hidden relative">
          @if (isLoading()) {
            <div class="absolute inset-0 flex items-center justify-center bg-white/50 z-10">
              <p-progressspinner styleClass="w-12 h-12" strokeWidth="4"></p-progressspinner>
            </div>
          }

          <p-table
            #dt
            [value]="data()"
            [rows]="10"
            [lazy]="true"
            (onLazyLoad)="loadData($event)"
            [paginator]="true"
            [totalRecords]="totalRecords()"
            [globalFilterFields]="['currAccCode', 'currAccDescription', 'referenceId']"
            styleClass="p-datatable-gridlines p-datatable-sm"
            [responsiveLayout]="'scroll'"
            [rowsPerPageOptions]="[10, 20, 50]"
            paginatorDropdownAppendTo="body"
          >
            <ng-template pTemplate="caption">
              <div class="flex justify-between items-center">
                <span class="text-xl font-bold">Transaction History</span>
                <p-iconfield iconPosition="left">
                  <p-inputicon class="pi pi-search"></p-inputicon>
                  <input
                    pInputText
                    type="text"
                    (input)="onGlobalFilter(dt, $event)"
                    placeholder="Search..."
                  />
                </p-iconfield>
              </div>
            </ng-template>
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="currAccCode">
                  CurrAccCode <p-sortIcon field="currAccCode"></p-sortIcon>
                </th>
                <th pSortableColumn="currAccDescription">
                  CurrAccDescription <p-sortIcon field="currAccDescription"></p-sortIcon>
                </th>
                <th pSortableColumn="date">Dates <p-sortIcon field="date"></p-sortIcon></th>
                <th pSortableColumn="referenceId">
                  ReferenceId <p-sortIcon field="referenceId"></p-sortIcon>
                </th>
                <th pSortableColumn="totalAmount" class="text-right">
                  TotalAmount <p-sortIcon field="totalAmount"></p-sortIcon>
                </th>
                <th pSortableColumn="totalPayment" class="text-right">
                  TotalPayment <p-sortIcon field="totalPayment"></p-sortIcon>
                </th>
                <th pSortableColumn="balance" class="text-right">
                  Balance <p-sortIcon field="balance"></p-sortIcon>
                </th>
                <th pSortableColumn="orderStatus">
                  Type <p-sortIcon field="orderStatus"></p-sortIcon>
                </th>
                <th pSortableColumn="status">Status <p-sortIcon field="status"></p-sortIcon></th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-item>
              <tr>
                <td>{{ item.currAccCode }}</td>
                <td>{{ item.currAccDescription }}</td>
                <td>{{ item.date | date: 'dd.MM.yyyy' }}</td>
                <td>{{ item.referenceId }}</td>
                <td class="text-right">
                  {{ item.totalAmount | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right">
                  {{ item.totalPayment | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td
                  class="text-right font-bold"
                  [ngClass]="{
                    'text-amber-400': item.orderStatus === 'DebitNote',
                    'text-red-500': item.balance < 0,
                    'text-green-500': item.balance > 0 && item.orderStatus !== 'DebitNote',
                  }"
                >
                  {{ item.balance | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td>{{ item.orderStatus }}</td>
                <td>
                  <p-tag
                    [value]="item.status"
                    [severity]="item.status === 'Open' ? 'success' : 'secondary'"
                  >
                  </p-tag>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="9" class="text-center p-4">No data found.</td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>

      <!-- Payment Received Modal -->
      <p-dialog
        [(visible)]="isPaymentDialogVisible"
        [modal]="true"
        header="Payment Received"
        [style]="{ width: '80%', 'max-width': '1200px' }"
        styleClass="p-fluid"
        [appendTo]="'body'"
      >
        <p class="text-muted-color mb-4">
          Please enter the payment details and upload your receipt below.
        </p>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Customer Selection -->
          <div class="md:col-span-2 flex flex-col gap-2">
            <label for="modal-customer" class="font-medium">Customer Account</label>
            <p-select
              id="modal-customer"
              [options]="customerOptions()"
              [(ngModel)]="paymentForm().customerCode"
              (ngModelChange)="onCustomerChange($event)"
              optionLabel="label"
              optionValue="value"
              placeholder="Select a customer account"
              [filter]="true"
              filterBy="label"
              appendTo="body"
            ></p-select>
          </div>

          <!-- Reference Code -->
          <div class="flex flex-col gap-2">
            <label for="modal-reference" class="font-medium">Invoice Num</label>
            <p-select
              id="modal-reference"
              [options]="referenceOptions()"
              [(ngModel)]="paymentForm().referenceCode"
              (ngModelChange)="onReferenceChange($event)"
              optionLabel="label"
              optionValue="value"
              placeholder="Select an invoice"
              appendTo="body"
              [loading]="loadingReferences()"
            ></p-select>
          </div>

          <!-- Payment Amount -->
          <div class="flex flex-col gap-2">
            <label for="modal-amount" class="font-medium">Payment Amount</label>
            <p-inputnumber
              id="modal-amount"
              [(ngModel)]="paymentForm().amount"
              mode="currency"
              currency="USD"
              locale="en-US"
              placeholder="0.00"
            ></p-inputnumber>
          </div>

          <!-- Receipt Number -->
          <div class="flex flex-col gap-2">
            <label for="modal-receipt" class="font-medium">Receipt / Document No</label>
            <p-iconfield iconPosition="left" class="w-full">
              <p-inputicon class="pi pi-file-edit"></p-inputicon>
              <input
                id="modal-receipt"
                pInputText
                [(ngModel)]="paymentForm().receiptNumber"
                placeholder="Enter document number"
                class="w-full"
              />
            </p-iconfield>
          </div>

          <!-- Date -->
          <div class="flex flex-col gap-2">
            <label for="modal-date" class="font-medium">Transaction Date</label>
            <p-datepicker
              id="modal-date"
              [(ngModel)]="paymentForm().date"
              [showIcon]="true"
              dateFormat="dd.mm.yy"
              placeholder="Select date"
              appendTo="body"
              [fluid]="true"
            ></p-datepicker>
          </div>

          <!-- Notes -->
          <div class="md:col-span-2 flex flex-col gap-2">
            <label for="modal-notes" class="font-medium">Notes / Description</label>
            <textarea
              id="modal-notes"
              pTextarea
              [(ngModel)]="paymentForm().notes"
              rows="3"
              placeholder="Enter any additional information..."
            ></textarea>
          </div>

          <!-- File Upload -->
          <div class="md:col-span-2 flex flex-col gap-2 mt-2">
            <label class="font-medium">Receipt File</label>
            <p-fileupload
              mode="advanced"
              chooseLabel="Choose File"
              [accept]="acceptStr()"
              maxFileSize="10000000"
              (onSelect)="onPaymentFileSelect($event)"
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
                    class="flex flex-col items-center justify-center py-8 border-2 border-dashed border-surface-200 dark:border-surface-700 rounded-xl bg-surface-50 dark:bg-surface-900/10"
                  >
                    <i class="pi pi-cloud-upload text-3xl text-primary mb-2"></i>
                    <span class="font-medium text-muted-color"
                      >Drag and drop receipt file here or click to choose</span
                    >
                    <span class="text-xs text-muted-color mt-1">PDF, PNG, JPG (Max. 10MB)</span>
                  </div>
                }
                @for (file of files; track file.name) {
                  <div
                    class="flex items-center gap-4 p-4 border border-surface-border rounded-xl bg-surface-card mb-2"
                  >
                    <div
                      class="w-10 h-10 bg-primary/10 rounded-lg flex items-center justify-center"
                    >
                      <i class="pi pi-file text-primary"></i>
                    </div>
                    <div class="flex-1 min-w-0">
                      <p class="font-semibold truncate">{{ file.name }}</p>
                      <p class="text-xs text-muted-color">{{ (file.size / 1024).toFixed(2) }} KB</p>
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

        <ng-template pTemplate="footer">
          <p-button
            label="Cancel"
            [text]="true"
            severity="secondary"
            (onClick)="isPaymentDialogVisible.set(false)"
          ></p-button>
          <p-button
            label="Upload"
            icon="pi pi-upload"
            (onClick)="submitPayment()"
            [loading]="paymentLoading()"
          ></p-button>
        </ng-template>
      </p-dialog>

      <!-- Debit Note Modal -->
      <p-dialog
        [(visible)]="isDebitNoteDialogVisible"
        [modal]="true"
        header="Debit Note"
        [style]="{ width: '70%', 'max-width': '900px' }"
        styleClass="p-fluid"
        [appendTo]="'body'"
      >
        <p class="text-muted-color mb-4">
          Please select cancelled invoice and enter debit note amount.
        </p>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="md:col-span-2 flex flex-col gap-2">
            <label for="debit-customer" class="font-medium">Customer Account</label>
            <p-select
              id="debit-customer"
              [options]="customerOptions()"
              [(ngModel)]="debitNoteForm().customerCode"
              (ngModelChange)="onDebitNoteCustomerChange($event)"
              optionLabel="label"
              optionValue="value"
              placeholder="Select a customer account"
              [filter]="true"
              filterBy="label"
              appendTo="body"
            ></p-select>
          </div>

          <div class="flex flex-col gap-2">
            <label for="debit-invoice" class="font-medium">Invoice No</label>
            <p-select
              id="debit-invoice"
              [options]="debitNoteInvoiceOptions()"
              [(ngModel)]="debitNoteForm().orderId"
              (ngModelChange)="onDebitNoteInvoiceChange($event)"
              optionLabel="label"
              optionValue="value"
              placeholder="Select an invoice"
              appendTo="body"
              [loading]="loadingCancelledInvoices()"
              [disabled]="!debitNoteForm().customerCode"
            ></p-select>
          </div>

          <div class="flex flex-col gap-2">
            <label for="debit-amount" class="font-medium">Debit Note Amount</label>
            <p-inputnumber
              id="debit-amount"
              [(ngModel)]="debitNoteForm().amount"
              mode="currency"
              currency="USD"
              locale="en-US"
              placeholder="0.00"
            ></p-inputnumber>
          </div>

          <div class="flex flex-col gap-2">
            <label for="debit-date" class="font-medium">Debit Note Date</label>
            <p-datepicker
              id="debit-date"
              [(ngModel)]="debitNoteForm().date"
              [showIcon]="true"
              dateFormat="dd.mm.yy"
              placeholder="Select date"
              appendTo="body"
              [fluid]="true"
            ></p-datepicker>
          </div>

          <div class="md:col-span-2 flex flex-col gap-2">
            <label for="debit-notes" class="font-medium">Notes / Description</label>
            <textarea
              id="debit-notes"
              pTextarea
              [(ngModel)]="debitNoteForm().notes"
              rows="3"
              placeholder="Enter any additional information..."
            ></textarea>
          </div>
        </div>

        <ng-template pTemplate="footer">
          <p-button
            label="Cancel"
            [text]="true"
            severity="secondary"
            (onClick)="isDebitNoteDialogVisible.set(false)"
          ></p-button>
          <p-button
            label="Create Debit Note"
            icon="pi pi-file-excel"
            severity="warn"
            (onClick)="submitDebitNote()"
            [loading]="debitNoteLoading()"
            [disabled]="debitNoteLoading()"
          ></p-button>
        </ng-template>
      </p-dialog>
    </div>
  `,
  styles: [
    `
      :host ::ng-deep {
        p-datepicker,
        .p-datepicker {
          width: 100% !important;
          display: flex !important;
        }
        .p-datepicker-input {
          flex: 1 1 auto;
          width: 1% !important; /* Flex container içinde tam yayılması için */
          min-width: 0;
        }
        .p-datepicker .p-inputtext {
          width: 100% !important;
        }
        .p-iconfield {
          width: 100% !important;
          display: block !important;
        }
      }
      .customer-balance-page {
        background-color: var(--surface-card);
        padding: 2rem;
        border-radius: var(--content-border-radius);
        margin-bottom: 2rem;
        position: relative;
        z-index: 0;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerBalance implements OnInit {
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);
  private paymentService = inject(PaymentService);

  isLoading = signal(false);
  data = signal<CustomerBalanceReportItem[]>([]);
  totalRecords = signal(0);

  startDate = signal<Date | null>(null);
  endDate = signal<Date | null>(null);

  // Payment Modal için signal'ler
  isPaymentDialogVisible = signal(false);
  paymentLoading = signal(false);
  selectedFile: File | null = null;
  customerOptions = signal<{ label: string; value: string }[]>([]);
  customerReferences = signal<CustomerReferenceDto[]>([]);
  loadingReferences = signal(false);
  isDebitNoteDialogVisible = signal(false);
  debitNoteLoading = signal(false);
  loadingCancelledInvoices = signal(false);
  cancelledInvoiceOptions = signal<CancelledInvoiceOption[]>([]);
  allowedExtensions = signal(['.pdf', '.jpg', '.jpeg', '.png']);
  acceptStr = computed(() => this.allowedExtensions().join(','));

  paymentForm = signal<PaymentForm>({
    customerCode: null,
    referenceCode: null,
    amount: null,
    receiptNumber: '',
    date: new Date(),
    notes: '',
  });

  // Dinamik referans listesi - müşteri seçildiğinde yüklenir
  referenceOptions = signal<{ label: string; value: string; balance?: number }[]>([]);
  debitNoteInvoiceOptions = computed(() =>
    this.cancelledInvoiceOptions().map((item) => ({
      label: `${item.invoiceNo} - Paid: $${item.paidAmount.toFixed(2)}`,
      value: item.orderId,
    })),
  );
  debitNoteForm = signal<DebitNoteForm>({
    customerCode: null,
    orderId: null,
    invoiceNo: null,
    amount: null,
    date: new Date(),
    notes: '',
  });
  canSubmitDebitNote = computed(() => {
    const form = this.debitNoteForm();
    const normalizedOrderId = Number(form.orderId ?? 0);
    const customerCode = (form.customerCode ?? '').trim();
    const selectedOption = this.cancelledInvoiceOptions().find(
      (x) => Number(x.orderId) === normalizedOrderId,
    );
    const invoiceNo = (form.invoiceNo ?? selectedOption?.invoiceNo ?? '').trim();
    const amount = Number(form.amount ?? 0);

    return !!form.customerCode && normalizedOrderId > 0 && invoiceNo.length > 0 && amount > 0;
  });

  ngOnInit() {
    // Sayfa açıldığında müşteri listesini yükle
    this.loadCustomers();
  }

  /**
   * Müşteri listesini yükler
   */
  loadCustomers() {
    this.reportService.getCustomers().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.customerOptions.set(res.data);
        }
      },
      error: (err) => {
        console.error('Müşteriler yüklenirken hata:', err);
      },
    });
  }

  /**
   * Payment modal'ını açar
   */
  openPaymentDialog() {
    // Formu sıfırla
    this.paymentForm.set({
      customerCode: null,
      referenceCode: null,
      amount: null,
      receiptNumber: '',
      date: new Date(),
      notes: '',
    });
    this.selectedFile = null;
    this.referenceOptions.set([]);
    this.isPaymentDialogVisible.set(true);
  }

  openDebitNoteDialog() {
    this.debitNoteForm.set({
      customerCode: null,
      orderId: null,
      invoiceNo: null,
      amount: null,
      date: new Date(),
      notes: '',
    });
    this.cancelledInvoiceOptions.set([]);
    this.isDebitNoteDialogVisible.set(true);
  }

  onDebitNoteCustomerChange(customerCode: string | null) {
    this.debitNoteForm.update((f) => ({
      ...f,
      customerCode,
      orderId: null,
      invoiceNo: null,
    }));
    this.cancelledInvoiceOptions.set([]);

    if (!customerCode) {
      return;
    }

    this.loadingCancelledInvoices.set(true);
    this.reportService.getCustomerCancelledInvoices(customerCode).subscribe({
      next: (res) => {
        this.loadingCancelledInvoices.set(false);
        const options = res.success && res.data?.data ? res.data.data : [];
        this.cancelledInvoiceOptions.set(options);
      },
      error: (err) => {
        this.loadingCancelledInvoices.set(false);
        console.error('Cancel invoice listesi yuklenirken hata:', err);
      },
    });
  }

  onDebitNoteInvoiceChange(orderId: number | string | null) {
    const normalizedOrderId = Number(orderId ?? 0);
    if (!normalizedOrderId || Number.isNaN(normalizedOrderId)) {
      this.debitNoteForm.update((f) => ({ ...f, orderId: null, invoiceNo: null }));
      return;
    }

    const selectedOption = this.cancelledInvoiceOptions().find(
      (x) => Number(x.orderId) === normalizedOrderId,
    );
    this.debitNoteForm.update((f) => ({
      ...f,
      orderId: normalizedOrderId,
      invoiceNo: selectedOption?.invoiceNo?.trim() || null,
    }));
  }

  /**
   * Müşteri değiştiğinde referans listesini yükler
   */
  onCustomerChange(customerCode: string | null) {
    // Referans listesini temizle
    this.referenceOptions.set([]);
    this.paymentForm.update((f) => ({ ...f, referenceCode: null, amount: null }));

    if (!customerCode) return;

    this.loadingReferences.set(true);
    this.reportService.getCustomerReferences(customerCode).subscribe({
      next: (res) => {
        this.loadingReferences.set(false);
        if (res.success && res.data?.data) {
          // Balance > 0 olan referansları dropdown'a ekle
          const options = res.data.data.map((ref) => ({
            label: `${ref.referenceId} - Balance: $${ref.balance.toFixed(2)}`,
            value: ref.referenceId,
            balance: ref.balance,
          }));
          this.referenceOptions.set(options);
        }
      },
      error: (err) => {
        this.loadingReferences.set(false);
        console.error('Referanslar yüklenirken hata:', err);
      },
    });
  }

  /**
   * Referans değiştiğinde bakiye tutarını doldurur
   */
  onReferenceChange(referenceCode: string | null) {
    if (!referenceCode) return;

    const selectedRef = this.referenceOptions().find((r) => r.value === referenceCode);
    if (selectedRef?.balance !== undefined) {
      this.paymentForm.update((f) => ({ ...f, amount: selectedRef.balance ?? null }));
    }
  }

  /**
   * Dosya seçildiğinde çalışır
   */
  onPaymentFileSelect(event: { files: File[] }) {
    this.selectedFile = event.files[0];
  }

  /**
   * Ödeme kaydını gönderir
   */
  submitPayment() {
    const form = this.paymentForm();
    if (!form.customerCode || !form.referenceCode || !form.amount) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in the required fields (Customer, Reference, and Amount).',
      });
      return;
    }

    this.paymentLoading.set(true);

    const formData = new FormData();
    formData.append('CustomerCode', form.customerCode);
    formData.append('ReferenceCode', form.referenceCode || '');
    formData.append('CurrencyCode', 'USD');
    formData.append('Amount', (form.amount || 0).toString());
    formData.append('ReceiptNumber', form.receiptNumber);
    formData.append('PaymentDate', form.date.toISOString());
    formData.append('Notes', form.notes);

    if (this.selectedFile) {
      formData.append('ReceiptFile', this.selectedFile);
    }

    this.paymentService.createPaymentWithFormData(formData).subscribe({
      next: (res) => {
        this.paymentLoading.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Payment notification saved successfully.',
          });
          this.isPaymentDialogVisible.set(false);
          // Tabloyu yenile
          this.loadData();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'An error occurred.',
          });
        }
      },
      error: (err) => {
        this.paymentLoading.set(false);
        const serverMessage =
          err?.error?.error ||
          err?.error?.message ||
          err?.error?.title ||
          err?.message ||
          'An error occurred while communicating with the server.';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: serverMessage,
        });
      },
    });
  }

  submitDebitNote() {
    const form = this.debitNoteForm();
    const normalizedOrderId = Number(form.orderId ?? 0);
    const customerCode = (form.customerCode ?? '').trim();
    const selectedOption = this.cancelledInvoiceOptions().find(
      (x) => Number(x.orderId) === normalizedOrderId,
    );
    const normalizedInvoiceNo = (form.invoiceNo ?? selectedOption?.invoiceNo ?? '').trim();
    const amount = Number(form.amount ?? 0);
    const missingFields: string[] = [];
    if (!customerCode) missingFields.push('Customer');
    if (normalizedOrderId <= 0 || !normalizedInvoiceNo) missingFields.push('Invoice');
    if (!(amount > 0)) missingFields.push('Amount');

    if (missingFields.length > 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: `Please fill in the required fields: ${missingFields.join(', ')}.`,
      });
      return;
    }

    const request: CreateDebitNoteRequest = {
      customerCode,
      orderId: normalizedOrderId,
      invoiceNo: normalizedInvoiceNo,
      amount,
      debitNoteDate: form.date,
      notes: form.notes || undefined,
    };

    this.debitNoteLoading.set(true);
    this.reportService
      .createDebitNoteAndExport(request)
      .pipe(finalize(() => this.debitNoteLoading.set(false)))
      .subscribe({
        next: (blob) => {
          const downloadName = `Debit_Note_${normalizedInvoiceNo}_${new Date()
            .toISOString()
            .slice(0, 16)
            .replace(/[-:T]/g, '')}.xlsx`;
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = downloadName;
          link.click();
          window.URL.revokeObjectURL(url);

          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Debit note generated successfully.',
          });
          this.isDebitNoteDialogVisible.set(false);
          this.loadData();
        },
        error: (err) => {
          const serverMessage =
            err?.error?.error ||
            err?.error?.message ||
            err?.error?.title ||
            err?.message ||
            'An error occurred while creating debit note.';

          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: serverMessage,
          });
        },
      });
  }

  onGlobalFilter(table: any, event: Event) {
    table.filterGlobal((event.target as HTMLInputElement).value, 'contains');
  }

  loadData(event?: any) {
    this.isLoading.set(true);

    const page = (event?.first || 0) / (event?.rows || 10) + 1;
    const pageSize = event?.rows || 10;
    const sortField = event?.sortField;
    const sortOrder = event?.sortOrder;

    const request = {
      startDate: this.startDate() || undefined,
      endDate: this.endDate() || undefined,
      page,
      pageSize,
      sortField,
      sortOrder,
    };

    this.reportService
      .getCustomerBalanceData(request)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            const data = res.data.data;
            this.data.set(data);
            this.totalRecords.set(res.data.totalCount);

            if (page === 1) {
              // Show message only on first page load or explicit button click (if event is from button, it might be partial)
              if (data.length > 0) {
                this.messageService.add({
                  severity: 'success',
                  summary: 'Success',
                  detail: 'Data loaded successfully.',
                });
              } else {
                this.messageService.add({
                  severity: 'warn',
                  summary: 'Warning',
                  detail: 'No data found for selected criteria.',
                });
              }
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
}
