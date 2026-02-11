import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { FileUploadModule } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { FluidModule } from 'primeng/fluid';
import { InventoryService } from '../../../core/services/inventory.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

/**
 * StockAdjustment bileşeni, envanter düzenlemeleri için kullanılır.
 * Kullanıcı Excel dosyası yükleyebilir, tarih seçebilir ve açıklama ekleyebilir.
 */
@Component({
  selector: 'app-stock-adjustment',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePickerModule,
    InputTextModule,
    ButtonModule,
    FileUploadModule,
    CardModule,
    FluidModule,
  ],
  template: `
    <div class="stock-adjustment-page">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Stock Adjustment</div>
          <p class="text-muted-color mb-6">
            Please upload the inventory adjustment file and fill in the required information.
          </p>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Select Date -->
              <div class="flex flex-col gap-2">
                <label for="adjustmentDate" class="font-medium">Selection Date</label>
                <p-datepicker
                  id="adjustmentDate"
                  [(ngModel)]="adjustmentForm().date"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="Select"
                >
                </p-datepicker>
              </div>

              <!-- Description -->
              <div class="flex flex-col gap-2">
                <label for="description" class="font-medium">Description</label>
                <input
                  id="description"
                  pInputText
                  [(ngModel)]="adjustmentForm().description"
                  placeholder="Enter description"
                />
              </div>

              <!-- File Upload -->
              <div class="md:col-span-2 flex flex-col gap-2 mt-4">
                <div class="flex items-center justify-between">
                  <label class="font-medium">Select Excel File</label>
                  <p-button
                    label="Download Template"
                    icon="pi pi-download"
                    [text]="true"
                    size="small"
                    (onClick)="onDownloadTemplate()"
                    [loading]="templateLoading()"
                  ></p-button>
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
export class StockAdjustment {
  private messageService = inject(MessageService);
  private inventoryService = inject(InventoryService);

  // Form durumu için Angular Signals kullanımı
  adjustmentForm = signal({
    date: new Date(),
    description: '',
  });

  loading = signal(false);
  templateLoading = signal(false);
  selectedFile: File | null = null;

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
    if (!this.selectedFile) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select an Excel file.',
      });
      return;
    }

    this.loading.set(true);

    this.inventoryService
      .createStockAdjustment({
        adjustmentDate: this.adjustmentForm().date,
        description: this.adjustmentForm().description,
        file: this.selectedFile,
      })
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail:
                `Processed: ${res.data.processedItemsCount}, Skipped: ${res.data.skippedItemsCount}. ` +
                (res.data.message || 'Stock adjustment has been completed successfully.'),
            });

            if (res.data.warnings?.length) {
              const warningPreview = res.data.warnings
                .slice(0, 5)
                .map((w) => `Row ${w.row} (${w.productCode}): ${w.reason}`)
                .join(' | ');

              this.messageService.add({
                severity: 'warn',
                summary: `${res.data.warnings.length} row(s) skipped`,
                detail: warningPreview,
                life: 10000,
              });
            }

            // Formu sıfırla
            this.adjustmentForm.set({
              date: new Date(),
              description: '',
            });
            this.selectedFile = null;
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: res.error || 'An error occurred during stock adjustment.',
            });
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.message || 'An error occurred during stock adjustment.',
          });
        },
      });
  }

  /**
   * Şablon dosyasını indirir
   */
  onDownloadTemplate() {
    this.templateLoading.set(true);
    this.inventoryService.downloadTemplate().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'StockAdjustmentTemplate.xlsx';
        link.click();
        window.URL.revokeObjectURL(url);
        this.templateLoading.set(false);
      },
      error: () => {
        this.templateLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to download template.',
        });
      },
    });
  }
}
