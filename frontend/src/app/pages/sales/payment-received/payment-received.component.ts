import { Component, ChangeDetectionStrategy, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';

import { MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { FluidModule } from 'primeng/fluid';
import { TextareaModule } from 'primeng/textarea';
import { PaymentService } from '../../../core/services/payment.service';
import { ReportService } from '../../../core/services/report.service';
import { AuthService } from '../../../core/services/auth.service';

interface PaymentForm {
  customerCode: string | null;
  referenceCode: string | null;
  amount: number | null;
  receiptNumber: string;
  date: Date;
  notes: string;
}

@Component({
  selector: 'app-payment-received',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    InputTextModule,
    InputNumberModule,
    DatePickerModule,
    FileUploadModule,
    ButtonModule,

    CardModule,
    IconFieldModule,
    InputIconModule,
    FluidModule,
    TextareaModule,
  ],

  template: `
    <div class="payment-received-page">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Payment Received</div>
          <p class="text-muted-color mb-6">
            Please enter the payment details and upload your receipt below.
          </p>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Customer Selection -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="customer" class="font-medium">Customer Account</label>
                <p-select
                  id="customer"
                  [options]="customerOptions()"
                  [(ngModel)]="paymentForm().customerCode"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Select a customer account"
                  [filter]="true"
                  filterBy="label"
                  [disabled]="isCustomerFixed()"
                >
                  <ng-template pTemplate="selectedItem" let-selectedOption>
                    <div class="flex items-center gap-2">
                      <i class="pi pi-building text-primary"></i>
                      <span>{{ selectedOption.label }}</span>
                    </div>
                  </ng-template>
                </p-select>
              </div>

              <!-- Reference Code -->
              <div class="flex flex-col gap-2">
                <label for="reference" class="font-medium">Reference Code / Method</label>
                <p-select
                  id="reference"
                  [options]="referenceOptions"
                  [(ngModel)]="paymentForm().referenceCode"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Select"
                ></p-select>
              </div>

              <!-- Payment Amount -->
              <div class="flex flex-col gap-2">
                <label for="amount" class="font-medium">Payment Amount</label>
                <p-inputnumber
                  id="amount"
                  [(ngModel)]="paymentForm().amount"
                  mode="currency"
                  currency="USD"
                  locale="en-US"
                  placeholder="0.00"
                ></p-inputnumber>
              </div>

              <!-- Receipt Number -->
              <div class="flex flex-col gap-2">
                <label for="receipt" class="font-medium">Receipt / Document No</label>
                <p-iconfield iconPosition="left">
                  <p-inputicon class="pi pi-file-edit"></p-inputicon>
                  <input
                    id="receipt"
                    pInputText
                    [(ngModel)]="paymentForm().receiptNumber"
                    placeholder="Enter document number"
                  />
                </p-iconfield>
              </div>

              <!-- Date -->
              <div class="flex flex-col gap-2">
                <label for="date" class="font-medium">Transaction Date</label>
                <p-datepicker
                  id="date"
                  [(ngModel)]="paymentForm().date"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="Select date"
                ></p-datepicker>
              </div>

              <!-- Notes -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="notes" class="font-medium">Notes / Description</label>
                <textarea
                  id="notes"
                  pTextarea
                  [(ngModel)]="paymentForm().notes"
                  rows="3"
                  placeholder="Enter any additional information..."
                ></textarea>
              </div>

              <!-- File Upload -->
              <div class="md:col-span-2 flex flex-col gap-2 mt-4">
                <label class="font-medium">Receipt File</label>
                <p-fileupload
                  mode="advanced"
                  chooseLabel="Choose File"
                  uploadLabel="Upload"
                  cancelLabel="Cancel"
                  [accept]="acceptStr()"
                  maxFileSize="10000000"
                  (onSelect)="onFileSelect($event)"
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
                        class="flex flex-col items-center justify-center py-10 border-2 border-dashed border-surface-200 dark:border-surface-700 rounded-xl bg-surface-50 dark:bg-surface-900/10"
                      >
                        <i class="pi pi-cloud-upload text-4xl text-primary mb-3"></i>
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

          <div
            class="flex justify-end gap-2 mt-8 py-4 border-t border-surface-200 dark:border-surface-700"
          >
            <p-button
              label="Upload"
              icon="pi pi-upload"
              (onClick)="onSubmit()"
              [loading]="loading()"
            ></p-button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      :host ::ng-deep {
        .p-datepicker {
          width: 100%;
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentReceived {
  private messageService = inject(MessageService);
  private paymentService = inject(PaymentService);
  private reportService = inject(ReportService);
  private authService = inject(AuthService);

  loading = signal(false);
  selectedFile: File | null = null;
  isCustomerFixed = signal(false);
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

  customerOptions = signal<{ label: string; value: string }[]>([]);

  referenceOptions = [
    { label: 'Bank Transfer (EFT/Wire)', value: 'EFT' },
    { label: 'Credit Card (Online)', value: 'CC' },
    { label: 'Check / Bill', value: 'CHECK' },
  ];

  constructor() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.reportService.getCustomers().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const customers = res.data;
          this.customerOptions.set(customers);

          // Eğer sadece bir müşteri varsa veya kullanıcı vendor ise otomatik seç
          const user = this.authService.getUser();
          if (customers.length === 1 && user?.linkedCustomerId) {
            this.paymentForm.update((prev) => ({
              ...prev,
              customerCode: customers[0].value,
            }));
            this.isCustomerFixed.set(true);
          } else if (customers.length === 1) {
            this.paymentForm.update((prev) => ({
              ...prev,
              customerCode: customers[0].value,
            }));
          }
        }
      },
      error: (err) => {
        console.error('Müşteriler yüklenirken hata:', err);
      },
    });
  }

  onFileSelect(event: any) {
    this.selectedFile = event.files[0];
  }

  onSubmit() {
    const form = this.paymentForm();
    if (!form.customerCode || !form.amount) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in the required fields (Customer and Amount).',
      });
      return;
    }

    this.loading.set(true);

    const formData = new FormData();
    formData.append('CustomerCode', form.customerCode);
    formData.append('PaymentMethodCode', form.referenceCode || '');
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
        this.loading.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Payment notification saved successfully.',
          });

          this.resetForm();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: res.error || 'An error occurred.',
          });
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'An error occurred while communicating with the server.',
        });
      },
    });
  }

  resetForm() {
    this.paymentForm.update((prev) => ({
      ...prev,
      customerCode: this.isCustomerFixed() ? prev.customerCode : null,
      referenceCode: null,
      amount: null,
      receiptNumber: '',
      date: new Date(),
      notes: '',
    }));
    this.selectedFile = null;
  }
}
