import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { FluidModule } from 'primeng/fluid';
import { OrderService } from '../../../core/services/order.service';
import { ReportService } from '../../../core/services/report.service';
import { AuthService } from '../../../core/services/auth.service';
import { ImportService } from '../../../core/services/import.service';

interface OrderForm {
  customerCode: string | null;
  type: string;
}

@Component({
  selector: 'app-sales-order',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    InputTextModule,
    FileUploadModule,
    ButtonModule,
    ToastModule,
    CardModule,
    FluidModule,
  ],

  template: `
    <div class="sales-order-page">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Sales Order Entry</div>
          <p class="text-muted-color mb-6">
            Please select the customer, order type, and upload the order file.
          </p>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Customer Selection -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="customer" class="font-medium">Selected Customer</label>
                <p-select
                  id="customer"
                  [options]="customerOptions()"
                  [(ngModel)]="orderForm().customerCode"
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

              <!-- Order Type -->
              <div class="md:col-span-2 flex flex-col gap-2">
                <label for="orderType" class="font-medium">Order Type</label>
                <p-select
                  id="orderType"
                  [options]="orderTypeOptions"
                  [(ngModel)]="orderForm().type"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Select order type"
                ></p-select>
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
                  uploadLabel="Upload"
                  cancelLabel="Cancel"
                  accept=".csv,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel,application/pdf"
                  maxFileSize="20000000"
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
                          >Drag and drop Excel file here or click to select</span
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
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesOrder {
  private messageService = inject(MessageService);
  private orderService = inject(OrderService);
  private reportService = inject(ReportService);
  private authService = inject(AuthService);
  private importService = inject(ImportService);

  loading = signal(false);
  selectedFile: File | null = null;
  isCustomerFixed = signal(false);

  orderForm = signal<OrderForm>({
    customerCode: null,
    type: 'Order',
  });

  customerOptions = signal<{ label: string; value: string }[]>([]);

  orderTypeOptions = [
    { label: 'Order', value: 'Order' },
    { label: 'PreOrder', value: 'PreOrder' },
    { label: 'Shipped', value: 'Shipped' },
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
            this.orderForm.update((prev) => ({
              ...prev,
              customerCode: customers[0].value,
            }));
            this.isCustomerFixed.set(true);
          } else if (customers.length === 1) {
            this.orderForm.update((prev) => ({
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
    const form = this.orderForm();
    if (!form.customerCode || !this.selectedFile) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a customer and upload an order file.',
      });
      return;
    }

    const extension = this.selectedFile.name.split('.').pop()?.toLowerCase();
    if (extension === 'xlsx' || extension === 'xls') {
      this.handleExcelImport();
    } else {
      this.handleFileUpload();
    }
  }

  handleExcelImport() {
    this.loading.set(true);
    const reader = new FileReader();

    reader.onload = (e: any) => {
      const base64Content = e.target.result as string;
      const request = {
        fileContent: base64Content,
        projectType: 'CWI' as const,
        customerCode: this.orderForm().customerCode!,
        orderType: this.orderForm().type,
      };

      this.importService.importOrderFromExcel(request).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            const errorDetails = (res.data.errors || []).slice(0, 3).map((e) => {
              const rowText = e?.row && e.row > 0 ? `Row ${e.row}: ` : '';
              return `${rowText}${e?.message || ''}`.trim();
            });

            if (res.data.successCount > 0) {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: `Imported ${res.data.successCount} rows successfully.`,
              });

              if ((res.data.errorCount || 0) > 0) {
                this.messageService.add({
                  severity: 'warn',
                  summary: 'Warning',
                  detail:
                    errorDetails.join(' | ') ||
                    `${res.data.errorCount} row(s) could not be imported.`,
                });
              }

              this.resetForm();
            } else {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail:
                  errorDetails.join(' | ') ||
                  (res.data.errorCount > 0
                    ? `${res.data.errorCount} row(s) failed to import.`
                    : 'No rows imported.'),
              });
            }
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: res.error || (res.errors && res.errors[0]) || 'Excel import failed.',
            });
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred during Excel import.',
          });
        },
      });
    };

    reader.readAsDataURL(this.selectedFile!);
  }

  handleFileUpload() {
    this.loading.set(true);
    const form = this.orderForm();
    const formData = new FormData();
    formData.append('CustomerCode', form.customerCode!);
    formData.append('OrderType', form.type);
    formData.append('OrderFile', this.selectedFile!);

    this.orderService.uploadSalesOrder(formData).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Sales order uploaded successfully.',
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

  downloadTemplate() {
    this.importService.downloadOrderTemplate().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'OrderTemplate.xlsx';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to download template.',
        });
      },
    });
  }

  resetForm() {
    this.orderForm.update((prev) => ({
      ...prev,
      customerCode: this.isCustomerFixed() ? prev.customerCode : null,
      type: 'Order',
    }));
    this.selectedFile = null;
  }
}
