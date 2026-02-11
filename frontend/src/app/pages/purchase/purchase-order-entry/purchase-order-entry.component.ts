import { Component, ChangeDetectionStrategy, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { FileUploadModule } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { FluidModule } from 'primeng/fluid';
import {
  PurchaseOrderService,
  VendorLookupDto,
} from '../../../core/services/purchase-order.service';

/**
 * PurchaseOrderEntry bileşeni, satın alma siparişlerinin toplu girişi için kullanılır.
 * Kullanıcı cari seçebilir, Excel dosyası yükleyebilir ve açıklama ekleyebilir.
 */
@Component({
  selector: 'app-purchase-order-entry',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    DatePickerModule,
    InputTextModule,
    ButtonModule,
    FileUploadModule,
    CardModule,
    FluidModule,
  ],
  template: `
    <div class="purchase-order-entry-page">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Purchase Order Entry</div>
          <p class="text-muted-color mb-6">
            Please select the account, upload the order file and fill in the details.
          </p>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Select Vendor -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="current" class="font-medium">Select Vendor</label>
                <p-select
                  id="current"
                  [options]="currentOptions()"
                  [ngModel]="vendorCode()"
                  (ngModelChange)="vendorCode.set($event)"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Select a vendor"
                  [filter]="true"
                  filterBy="label"
                  [loading]="loadingVendors()"
                >
                  <ng-template pTemplate="selectedItem" let-selectedOption>
                    <div class="flex items-center gap-2">
                      <i class="pi pi-building text-primary"></i>
                      <span>{{ selectedOption?.label }}</span>
                    </div>
                  </ng-template>
                </p-select>
              </div>

              <!-- Order Date -->
              <div class="flex flex-col gap-2">
                <label for="orderDate" class="font-medium">Order Date</label>
                <p-datepicker
                  id="orderDate"
                  [ngModel]="orderDate()"
                  (ngModelChange)="orderDate.set($event)"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="Select Date"
                >
                </p-datepicker>
              </div>

              <!-- Delivery Date -->
              <div class="flex flex-col gap-2">
                <label for="deliveryDate" class="font-medium">Delivery Date</label>
                <p-datepicker
                  id="deliveryDate"
                  [ngModel]="deliveryDate()"
                  (ngModelChange)="deliveryDate.set($event)"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="Select Date"
                >
                </p-datepicker>
              </div>

              <!-- Description -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="description" class="font-medium">Description</label>
                <input
                  id="description"
                  pInputText
                  [ngModel]="description()"
                  (ngModelChange)="description.set($event)"
                  placeholder="Enter description"
                />
              </div>

              <!-- File Upload -->
              <div class="md:col-span-2 flex flex-col gap-2 mt-4">
                <div class="flex items-center justify-between">
                  <label class="font-medium">Select Excel File</label>
                  <a
                    (click)="downloadTemplate()"
                    class="text-primary cursor-pointer flex items-center gap-2 text-sm hover:underline no-underline"
                  >
                    <i class="pi pi-download"></i>
                    <span>Download Template</span>
                  </a>
                </div>
                <p-fileupload
                  mode="advanced"
                  chooseLabel="Choose File"
                  accept=".xlsx, .xls"
                  maxFileSize="1000000"
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
                        <i class="pi pi-file-excel text-4xl text-primary mb-3"></i>
                        <span class="font-medium text-muted-color"
                          >Drag and drop order file (Excel) here or click to select</span
                        >
                        <span class="text-xs text-muted-color mt-1">XLSX, XLS (Max 1MB)</span>
                      </div>
                    }
                    @for (file of files; track file.name) {
                      <div
                        class="flex items-center gap-4 p-4 border border-surface-border rounded-xl bg-surface-card mb-2"
                      >
                        <div
                          class="w-10 h-10 bg-primary/10 rounded-lg flex items-center justify-center"
                        >
                          <i class="pi pi-file-excel text-primary"></i>
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
              (onClick)="onUpload()"
              [loading]="loading()"
            >
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
export class PurchaseOrderEntry implements OnInit {
  private messageService = inject(MessageService);
  private purchaseOrderService = inject(PurchaseOrderService);

  // Form State Signals
  vendorCode = signal<string | null>(null);
  orderDate = signal<Date>(new Date());
  deliveryDate = signal<Date>(new Date());
  description = signal<string>('');

  loading = signal(false);
  loadingVendors = signal(false);
  selectedFile: File | null = null;

  // Cari seçenekleri
  currentOptions = signal<VendorLookupDto[]>([]);

  ngOnInit() {
    this.loadVendors();
  }

  loadVendors() {
    this.loadingVendors.set(true);
    this.purchaseOrderService.getVendors(true).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.currentOptions.set(res.data);
        }
        this.loadingVendors.set(false);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Could not load vendors.',
        });
        this.loadingVendors.set(false);
      },
    });
  }

  /**
   * Dosya seçildiğinde tetiklenen fonksiyon
   * @param event Dosya seçim olayı
   */
  onFileSelect(event: any) {
    this.selectedFile = event.files[0];
  }

  /**
   * Upload butonuna basıldığında tetiklenen fonksiyon
   */
  onUpload() {
    const vendorCode = this.vendorCode();
    const date = this.orderDate();
    const deliveryDate = this.deliveryDate();
    const description = this.description();

    if (!vendorCode || !this.selectedFile) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select an account and Excel file.',
      });
      return;
    }

    this.loading.set(true);

    this.purchaseOrderService
      .uploadExcel({
        vendorCode: vendorCode,
        orderDate: date,
        deliveryDate: deliveryDate,
        description: description,
        file: this.selectedFile,
      })
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: res.data.message || 'Purchase order entry has been completed successfully.',
            });

            // Formu sıfırla
            this.vendorCode.set(null);
            this.orderDate.set(new Date());
            this.deliveryDate.set(new Date());
            this.description.set('');
            this.selectedFile = null;
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: res.error || 'An error occurred during purchase order entry.',
            });
          }
        },
        error: (error) => {
          this.loading.set(false);
          // Backend 'Error' property dönüyor, Frontend 'message' arıyordu. Düzeltildi.
          const errorMessage =
            error.error?.error ||
            error.error?.message ||
            'An error occurred during purchase order entry.';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: errorMessage,
          });
        },
      });
  }

  downloadTemplate() {
    this.purchaseOrderService.downloadTemplate().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'PurchaseOrderTemplate.xlsx';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to download template.',
        });
      },
    });
  }
}
