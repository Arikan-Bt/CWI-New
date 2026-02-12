import { Component, ChangeDetectionStrategy, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { FluidModule } from 'primeng/fluid';
import { PurchasingService } from '../../../core/services/purchasing.service';

@Component({
  selector: 'app-payments-made',
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
  ],
  template: `
    <div class="payments-made-page">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Payment Made</div>
          <p class="text-muted-color mb-6">
            Please enter payment details and upload the receipt file below.
          </p>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Account Selection -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="account" class="font-medium">Vendor Account</label>
                <p-select
                  id="account"
                  [options]="accountOptions()"
                  [(ngModel)]="paymentForm().vendorCode"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Select a vendor account"
                  [filter]="true"
                  filterBy="label"
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

              <!-- File Upload -->
              <div class="md:col-span-2 flex flex-col gap-2 mt-4">
                <label class="font-medium">Receipt File</label>
                <p-fileupload
                  mode="advanced"
                  chooseLabel="Choose File"
                  uploadLabel="Upload"
                  cancelLabel="Cancel"
                  accept="image/*,.pdf"
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
              label="Save Payment"
              icon="pi pi-save"
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
export class PaymentsMade implements OnInit {
  private messageService = inject(MessageService);
  private purchasingService = inject(PurchasingService);

  loading = signal(false);
  selectedFile: File | null = null;

  paymentForm = signal({
    vendorCode: null as string | null,
    referenceCode: null as string | null,
    amount: null as number | null,
    receiptNumber: '',
    date: new Date(),
  });

  accountOptions = signal<{ label: string; value: string }[]>([]);

  referenceOptions = [
    { label: 'Bank Transfer (EFT/Wire)', value: 'EFT' },
    { label: 'Credit Card', value: 'CC' },
    { label: 'Check / Bill', value: 'CHECK' },
  ];

  ngOnInit() {
    this.loadVendors();
  }

  loadVendors() {
    this.purchasingService.getVendors().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.accountOptions.set(res.data);
        }
      },
      error: (err) => {
        console.error('Error loading vendors:', err);
      },
    });
  }

  onFileSelect(event: any) {
    this.selectedFile = event.files[0];
  }

  onSubmit() {
    const form = this.paymentForm();
    if (!form.vendorCode || !form.amount) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in the required fields (Account and Amount).',
      });
      return;
    }

    this.loading.set(true);

    const formData = new FormData();
    formData.append('VendorCode', form.vendorCode);
    formData.append('Amount', form.amount.toString());
    formData.append('CurrencyCode', 'USD');
    formData.append('PaymentDate', form.date.toISOString());
    if (form.referenceCode) formData.append('ReferenceNumber', form.referenceCode);
    if (form.receiptNumber) formData.append('Description', form.receiptNumber); // Using receiptNumber as description for now or could be separate

    if (this.selectedFile) {
      formData.append('PaymentFile', this.selectedFile);
    }

    this.purchasingService.createPayment(formData).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: res.data.message,
          });

          this.paymentForm.set({
            vendorCode: null,
            referenceCode: null,
            amount: null,
            receiptNumber: '',
            date: new Date(),
          });
          this.selectedFile = null;
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
        console.error('Error saving payment:', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'An error occurred while saving the payment notification.',
        });
      },
    });
  }
}
