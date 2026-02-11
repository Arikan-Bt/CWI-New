import { Component, ChangeDetectionStrategy, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { FluidModule } from 'primeng/fluid';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';

import { MessageService } from 'primeng/api';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ReportService } from '../../../core/services/report.service';
import {
  CustomerPaymentDetailItem,
  CustomerPaymentDetailReportRequest,
} from '../../../core/models/customer-payment-detail.models';
import { finalize } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-customer-payment-details',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    DatePickerModule,
    ButtonModule,
    TableModule,
    CardModule,
    FluidModule,
    IconFieldModule,
    InputIconModule,

    ProgressSpinnerModule,
  ],

  template: `
    <div class="customer-payment-details-page">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Customer Payment Details</div>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <!-- Cari Seçimi -->
              <div class="flex flex-col gap-2">
                <label for="customer" class="font-medium text-sm text-surface-600"
                  >Select Account</label
                >
                <p-select
                  id="customer"
                  [options]="customerOptions()"
                  [(ngModel)]="filters.customerCode"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Select..."
                  [filter]="true"
                  filterBy="label"
                ></p-select>
              </div>

              <!-- Başlangıç Tarihi -->
              <div class="flex flex-col gap-2">
                <label for="startDate" class="font-medium text-sm text-surface-600"
                  >Starting Date</label
                >
                <p-datepicker
                  id="startDate"
                  [(ngModel)]="filters.startDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="01.01.2026"
                ></p-datepicker>
              </div>

              <!-- Bitiş Tarihi -->
              <div class="flex flex-col gap-2">
                <label for="endDate" class="font-medium text-sm text-surface-600"
                  >Ending Date</label
                >
                <p-datepicker
                  id="endDate"
                  [(ngModel)]="filters.endDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="05.01.2026"
                ></p-datepicker>
              </div>
            </div>

            <div class="flex justify-end mt-6">
              <p-button
                label="Show Details"
                severity="danger"
                (onClick)="onShowDetails()"
                [loading]="loading()"
              ></p-button>
            </div>
          </p-fluid>
        </div>

        <div class="card p-0 overflow-hidden relative">
          @if (loading()) {
            <div class="absolute inset-0 flex items-center justify-center bg-white/50 z-10">
              <p-progressspinner styleClass="w-12 h-12" strokeWidth="4"></p-progressspinner>
            </div>
          }

          <p-table
            #dt
            [value]="paymentDetails()"
            [rows]="10"
            [lazy]="true"
            (onLazyLoad)="onLazyLoad($event)"
            [paginator]="true"
            [totalRecords]="totalRecords()"
            [rowsPerPageOptions]="[10, 20, 50]"
            styleClass="p-datatable-gridlines p-datatable-sm"
            [responsiveLayout]="'scroll'"
            paginatorDropdownAppendTo="body"
          >
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="date" class="text-xs uppercase bg-surface-50">
                  Document Date <p-sortIcon field="date"></p-sortIcon>
                </th>
                <th pSortableColumn="refNo1" class="text-xs uppercase bg-surface-50">
                  Ref No <p-sortIcon field="refNo1"></p-sortIcon>
                </th>
                <th pSortableColumn="description" class="text-xs uppercase bg-surface-50">
                  Description <p-sortIcon field="description"></p-sortIcon>
                </th>
                <th pSortableColumn="invoiceNo" class="text-xs uppercase bg-surface-50">
                  InvoiceNo <p-sortIcon field="invoiceNo"></p-sortIcon>
                </th>
                <th pSortableColumn="docType" class="text-xs uppercase bg-surface-50">
                  Document Type <p-sortIcon field="docType"></p-sortIcon>
                </th>
                <th class="text-xs uppercase bg-surface-50">Ref No</th>
                <th pSortableColumn="debit" class="text-xs uppercase bg-surface-50 text-right">
                  Debit (Doc) <p-sortIcon field="debit"></p-sortIcon>
                </th>
                <th pSortableColumn="credit" class="text-xs uppercase bg-surface-50 text-right">
                  Credit (Doc) <p-sortIcon field="credit"></p-sortIcon>
                </th>
                <th pSortableColumn="balance" class="text-xs uppercase bg-surface-50 text-right">
                  Balance (Doc) <p-sortIcon field="balance"></p-sortIcon>
                </th>
                <th class="text-xs uppercase bg-surface-50 text-center">Receipt</th>
                <th class="text-xs uppercase bg-surface-50 text-center">Delete</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-item>
              <tr>
                <td>{{ item.date | date: 'dd.MM.yyyy' }}</td>
                <td>{{ item.refNo1 }}</td>
                <td>{{ item.description }}</td>
                <td>{{ item.invoiceNo }}</td>
                <td>{{ item.docType }}</td>
                <td>{{ item.refNo2 }}</td>
                <td class="text-right">{{ item.debit | currency: 'USD' : 'symbol' : '1.2-2' }}</td>
                <td class="text-right">{{ item.credit | currency: 'USD' : 'symbol' : '1.2-2' }}</td>
                <td
                  class="text-right font-bold"
                  [ngClass]="{
                    'text-red-500': item.balance < 0,
                    'text-green-500': item.balance > 0,
                  }"
                >
                  {{ item.balance | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-center">
                  <p-button
                    icon="pi pi-file"
                    [text]="true"
                    [severity]="item.receiptFilePath ? 'danger' : 'secondary'"
                    [disabled]="!item.receiptFilePath"
                    (onClick)="onViewReceipt(item)"
                    size="small"
                  ></p-button>
                </td>
                <td class="text-center">
                  <p-button
                    icon="pi pi-trash"
                    [text]="true"
                    severity="danger"
                    size="small"
                  ></p-button>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="footer">
              <tr class="font-bold">
                <td colspan="6" class="text-left font-bold bg-surface-50">Total</td>
                <td class="text-right bg-surface-50">
                  {{ totals().debit | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right bg-surface-50">
                  {{ totals().credit | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td
                  class="text-right bg-surface-50"
                  [ngClass]="{
                    'text-red-500': totals().balance < 0,
                    'text-green-500': totals().balance > 0,
                  }"
                >
                  {{ totals().balance | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td colspan="2" class="bg-surface-50"></td>
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

          <div class="flex justify-end p-4">
            <p-button
              label="Save as Excel (2007 Format)"
              severity="danger"
              icon="pi pi-file-excel"
              (onClick)="onExportExcel()"
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
        .p-datatable-gridlines .p-datatable-thead > tr > th {
          border-width: 1px;
          padding: 0.75rem 0.5rem;
          white-space: nowrap;
        }
        .p-datatable-gridlines .p-datatable-tbody > tr > td {
          border-width: 1px;
          padding: 0.5rem;
          font-size: 0.875rem;
        }
        .p-datatable-gridlines .p-datatable-tfoot > tr > td {
          border-width: 1px;
          padding: 0.75rem 0.5rem;
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerPaymentDetails implements OnInit {
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);

  loading = signal(false);
  totalRecords = signal(0);

  // Pagination State
  currentPage = signal(1);
  pageSize = signal(10);
  sortField = signal<string | undefined>(undefined);
  sortOrder = signal<number | undefined>(undefined);

  // Filtreleme verileri
  filters = {
    customerCode: null as string | null,
    startDate: null as Date | null,
    endDate: null as Date | null,
  };

  // Müşteri listesi
  customerOptions = signal<{ label: string; value: any }[]>([]);

  // Tablo verisi
  paymentDetails = signal<CustomerPaymentDetailItem[]>([]);

  // Toplamlar
  totals = signal({
    debit: 0,
    credit: 0,
    balance: 0,
  });

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.reportService.getCustomers().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.customerOptions.set(res.data);
        }
      },
      error: (err) => {
        console.error('Customer load error:', err);
      },
    });
  }

  // Detayları göster butonu tetiklendiğinde
  onShowDetails() {
    this.currentPage.set(1);
    this.loadData();
  }

  onLazyLoad(event: any) {
    const page = (event.first || 0) / (event.rows || 10) + 1;
    this.currentPage.set(page);
    this.pageSize.set(event.rows || 10);
    this.sortField.set(event.sortField);
    this.sortOrder.set(event.sortOrder);

    this.loadData();
  }

  loadData() {
    if (!this.filters.customerCode) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a customer account.',
      });
      return;
    }

    this.loading.set(true);

    const request: CustomerPaymentDetailReportRequest = {
      customerCode: this.filters.customerCode,
      startDate: this.filters.startDate ?? undefined,
      endDate: this.filters.endDate ?? undefined,
      page: this.currentPage(),
      pageSize: this.pageSize(),
      sortField: this.sortField(),
      sortOrder: this.sortOrder(),
    };

    this.reportService
      .getCustomerPaymentDetails(request)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            const data = res.data;
            this.paymentDetails.set(data.data);
            this.totalRecords.set(data.totalCount);
            this.totals.set({
              debit: data.totalDebit,
              credit: data.totalCredit,
              balance: data.totalBalance,
            });

            if (this.currentPage() === 1) {
              if (data.data.length === 0) {
                this.messageService.add({
                  severity: 'info',
                  summary: 'Info',
                  detail: 'No data found matching the selected criteria.',
                });
              } else {
                this.messageService.add({
                  severity: 'success',
                  summary: 'Success',
                  detail: 'Data loaded successfully.',
                });
              }
            }
          }
        },
        error: (err) => {
          console.error('Payment details load error:', err);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while loading payment details.',
          });
        },
      });
  }

  // Excel'e aktar
  onExportExcel() {
    this.messageService.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Excel file is being prepared...',
    });
  }

  onViewReceipt(item: CustomerPaymentDetailItem) {
    if (!item.receiptFilePath) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'No receipt document is attached to this transaction.',
      });
      return;
    }

    const baseUrl = environment.apiUrl.replace('/api', '');
    window.open(`${baseUrl}/${item.receiptFilePath}`, '_blank');
  }
}
