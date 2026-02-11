import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { FluidModule } from 'primeng/fluid';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ReportService } from '../../../core/services/report.service';
import {
  ItemOrderCheckResponse,
  ItemOrderSummary,
  ItemMovement,
} from '../../../core/models/item-order-check.models';
import { getOrderStatusSeverity } from '../../../core/utils/status-utils';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-item-order-check',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    ButtonModule,
    ToastModule,
    IconFieldModule,
    InputIconModule,
    FluidModule,
    TableModule,
    TagModule,
  ],

  template: `
    <div class="item-order-check-page p-4">
      <div class="flex flex-col gap-6">
        <!-- Arama Bölümü -->
        <div class="card border-0 shadow-sm overflow-hidden">
          <div class="p-6">
            <div class="font-bold text-xl mb-2 text-gray-800 dark:text-gray-100">
              Item Order Check
            </div>
            <p class="text-gray-500 mb-6">
              Enter the SKU number to check the order status of the item.
            </p>

            <div class="grid grid-cols-1 md:grid-cols-4 items-end gap-4">
              <div class="md:col-span-3">
                <p-fluid>
                  <label for="sku" class="font-bold text-xs uppercase text-gray-400 mb-2 block"
                    >SKU / Item Code</label
                  >
                  <p-iconfield>
                    <p-inputicon class="pi pi-barcode"></p-inputicon>
                    <input
                      id="sku"
                      pInputText
                      [(ngModel)]="skuNumber"
                      placeholder="Enter SKU number (e.g. BP3816X.350)"
                      (keyup.enter)="onCheck()"
                      class="h-11"
                    />
                  </p-iconfield>
                </p-fluid>
              </div>
              <p-button
                label="Check Status"
                icon="pi pi-search"
                styleClass="h-11 w-full"
                (onClick)="onCheck()"
                [loading]="loading()"
                [disabled]="!skuNumber.trim()"
              ></p-button>
            </div>
          </div>
        </div>

        @if (showResult() && checkData()) {
          <!-- Item Orders And Quantities -->
          <div class="card border-0 shadow-sm overflow-hidden animate-fade-in">
            <div class="p-6">
              <div class="font-bold text-xl mb-4 text-gray-800 dark:text-gray-100">
                Item Orders and Quantities
              </div>

              <p-table
                [value]="[checkData()!.summary]"
                styleClass="p-datatable-sm custom-report-table"
              >
                <ng-template pTemplate="header">
                  <tr>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">ItemCode</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">PurchaseQty</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">SalesQty</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">WareHouseQty</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">ReserveQty</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">IncomingStock</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">AvailableQty</th>
                  </tr>
                </ng-template>
                <ng-template pTemplate="body" let-summary>
                  <tr>
                    <td class="border-0 font-medium">{{ summary.itemCode }}</td>
                    <td class="border-0 font-medium">{{ summary.purchaseQty | number: '1.0-0' }}</td>
                    <td class="border-0 font-medium">{{ summary.salesQty | number: '1.0-0' }}</td>
                    <td class="border-0 font-medium">{{ summary.wareHouseQty | number: '1.0-0' }}</td>
                    <td class="border-0 font-medium">{{ summary.reserveQty | number: '1.0-0' }}</td>
                    <td class="border-0 font-medium text-blue-500">
                      {{ summary.incomingStock | number: '1.0-0' }}
                    </td>
                    <td class="border-0 font-bold text-primary">
                      {{ summary.availableQty | number: '1.0-0' }}
                    </td>
                  </tr>
                </ng-template>
              </p-table>
            </div>
          </div>

          <!-- Item Check Status -->
          <div class="card border-0 shadow-sm overflow-hidden animate-fade-in">
            <div class="p-6">
              <div class="font-bold text-xl mb-4 text-gray-800 dark:text-gray-100">
                Item Check Status
              </div>

              <p-table
                [value]="checkData()!.movements"
                [lazy]="true"
                (onLazyLoad)="loadData($event)"
                [rows]="10"
                [paginator]="true"
                [totalRecords]="totalRecords()"
                [rowsPerPageOptions]="[10, 20, 50]"
                styleClass="p-datatable-sm custom-report-table"
                responsiveLayout="scroll"
              >
                <ng-template pTemplate="header">
                  <tr>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">ItemCode</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">Status</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">Account</th>
                    <th class="bg-transparent border-0 text-gray-400 font-bold">Qty</th>
                  </tr>
                </ng-template>
                <ng-template pTemplate="body" let-movement>
                  <tr>
                    <td class="border-0">{{ movement.itemCode }}</td>
                    <td class="border-0">
                      <p-tag
                        [value]="movement.status"
                        [severity]="getSeverity(movement.status)"
                      ></p-tag>
                    </td>
                    <td class="border-0">{{ movement.account }}</td>
                    <td class="border-0 font-medium">{{ movement.qty | number: '1.0-0' }}</td>
                  </tr>
                </ng-template>
                <ng-template pTemplate="emptymessage">
                  <tr>
                    <td colspan="4" class="text-center py-8 text-gray-400">
                      No movements found for this item.
                    </td>
                  </tr>
                </ng-template>
              </p-table>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [
    `
      ::ng-deep .custom-report-table {
        .p-datatable-thead > tr > th {
          border-bottom: 1px solid #f1f5f9 !important;
          padding: 1rem 0.5rem;
        }
        .p-datatable-tbody > tr > td {
          padding: 1rem 0.5rem;
          border-bottom: 1px solid #f8fafc !important;
        }
      }

      .animate-fade-in {
        animation: fadeIn 0.4s ease-out forwards;
      }

      @keyframes fadeIn {
        from {
          opacity: 0;
          transform: translateY(10px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemOrderCheck {
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);

  loading = signal(false);
  skuNumber = '';
  showResult = signal(false);
  checkData = signal<ItemOrderCheckResponse | null>(null);
  totalRecords = signal(0);

  onCheck() {
    if (!this.skuNumber.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter an SKU number to check.',
      });
      return;
    }
    this.loadData({ first: 0, rows: 10 });
  }

  loadData(event: any) {
    if (!this.skuNumber.trim()) return;

    this.loading.set(true);
    const page = (event.first || 0) / (event.rows || 10) + 1;
    const pageSize = event.rows || 10;

    this.reportService
      .getItemOrderCheckData({ sku: this.skuNumber.trim(), page, pageSize })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.checkData.set(res.data);
            this.totalRecords.set(res.data.totalMovements);
            this.showResult.set(true);

            // Only show success toast on initial load (page 1) to avoid spamming on pagination
            if (page === 1) {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Item information retrieved successfully.',
              });
            }
          }
        },
        error: (err) => {
          console.error(err);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to retrieve item information.',
          });
        },
      });
  }

  getSeverity(
    status: string,
  ): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | undefined {
    return getOrderStatusSeverity(status);
  }
}
