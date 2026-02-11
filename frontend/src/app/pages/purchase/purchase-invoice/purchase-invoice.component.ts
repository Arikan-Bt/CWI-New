import { Component, ChangeDetectionStrategy, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { FluidModule } from 'primeng/fluid';
import { PurchasingService } from '../../../core/services/purchasing.service';

@Component({
  selector: 'app-purchase-invoice',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    DatePickerModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    FileUploadModule,
    ButtonModule,
    CardModule,
    FluidModule,
  ],
  template: `
    <div class="purchase-invoice-page">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Purchase Invoice Entry</div>
          <p class="text-muted-color mb-6">
            Please select the vendor, upload the invoice file and fill in the invoice details.
          </p>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Select Vendor -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="vendor" class="font-medium">Select Vendor</label>
                <p-select
                  id="vendor"
                  [options]="vendorOptions()"
                  [(ngModel)]="invoiceForm().vendorCode"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Select a vendor"
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

              <!-- Select Date -->
              <div class="flex flex-col gap-2">
                <label for="invoiceDate" class="font-medium">Select Date</label>
                <p-datepicker
                  id="invoiceDate"
                  [(ngModel)]="invoiceForm().date"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="Select"
                ></p-datepicker>
              </div>

              <!-- Select Currency -->
              <div class="flex flex-col gap-2">
                <label for="currency" class="font-medium">Select Currency</label>
                <p-select
                  id="currency"
                  [options]="currencyOptions"
                  [(ngModel)]="invoiceForm().currency"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Select currency"
                ></p-select>
              </div>

              <!-- Enter Invoice Amount -->
              <div class="flex flex-col gap-2">
                <label for="amount" class="font-medium">Enter Invoice Amount</label>
                <p-inputnumber
                  id="amount"
                  [(ngModel)]="invoiceForm().amount"
                  [minFractionDigits]="2"
                  placeholder="Enter invoice amount"
                  mode="currency"
                  currency="USD"
                  locale="en-US"
                ></p-inputnumber>
              </div>

              <!-- Enter Invoice Number -->
              <div class="flex flex-col gap-2">
                <label for="invoiceNumber" class="font-medium">Enter Invoice Number</label>
                <input
                  id="invoiceNumber"
                  pInputText
                  [(ngModel)]="invoiceForm().invoiceNumber"
                  placeholder="Enter invoice number"
                />
              </div>

              <!-- Enter Invoice Description -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="description" class="font-medium">Enter Invoice Description</label>
                <textarea
                  id="description"
                  pTextarea
                  [(ngModel)]="invoiceForm().description"
                  rows="3"
                  placeholder="Enter description"
                ></textarea>
              </div>

              <!-- File Upload -->
              <div class="md:col-span-2 flex flex-col gap-2 mt-4">
                <label class="font-medium">Select Invoice File</label>
                <p-fileupload
                  mode="advanced"
                  chooseLabel="Choose File"
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
                        <i class="pi pi-file-pdf text-4xl text-primary mb-3"></i>
                        <span class="font-medium text-muted-color"
                          >Drag and drop invoice file (PDF or Image) here or click to select</span
                        >
                        <span class="text-xs text-muted-color mt-1">PDF, Image (Max 10MB)</span>
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
            <p-button label="Save" icon="pi pi-check" (onClick)="onSave()" [loading]="loading()">
            </p-button>
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
export class PurchaseInvoice implements OnInit {
  private messageService = inject(MessageService);
  private purchasingService = inject(PurchasingService);

  loading = signal(false);
  selectedFile: File | null = null;

  invoiceForm = signal({
    vendorCode: null as string | null,
    date: new Date(),
    currency: 'USD',
    amount: null as number | null,
    invoiceNumber: '',
    description: '',
  });

  vendorOptions = signal<{ label: string; value: string }[]>([]);

  currencyOptions = [
    { label: 'USD', value: 'USD' },
    { label: 'EUR', value: 'EUR' },
    { label: 'TRY', value: 'TRY' },
  ];

  ngOnInit() {
    this.loadVendors();
  }

  loadVendors() {
    this.purchasingService.getVendors().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.vendorOptions.set(res.data);
        }
      },
      error: (err) => {
        console.error('Error loading vendors:', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load vendors.',
        });
      },
    });
  }

  /**
   * Dosya seçildiğinde tetiklenen fonksiyon
   */
  onFileSelect(event: any) {
    this.selectedFile = event.files[0];
    this.messageService.add({
      severity: 'info',
      summary: 'Info',
      detail: this.selectedFile?.name,
    });
  }

  /**
   * Fatura kaydedildiğinde tetiklenen fonksiyon
   */
  onSave() {
    const form = this.invoiceForm();
    if (!form.vendorCode || !form.amount || !form.invoiceNumber) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in the required fields (Vendor, Amount, and Invoice Number).',
      });
      return;
    }

    this.loading.set(true);

    const formData = new FormData();
    formData.append('VendorCode', form.vendorCode);
    formData.append('InvoiceDate', form.date.toISOString());
    formData.append('CurrencyCode', form.currency);
    formData.append('Amount', form.amount.toString());
    formData.append('InvoiceNumber', form.invoiceNumber);
    if (form.description) {
      formData.append('Description', form.description);
    }
    if (this.selectedFile) {
      formData.append('InvoiceFile', this.selectedFile);
    }

    this.purchasingService.createInvoice(formData).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: res.data.message,
          });

          // Reset form
          this.invoiceForm.set({
            vendorCode: null,
            date: new Date(),
            currency: 'USD',
            amount: null,
            invoiceNumber: '',
            description: '',
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
        console.error('Error saving invoice:', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'An error occurred while saving the invoice.',
        });
      },
    });
  }
}
