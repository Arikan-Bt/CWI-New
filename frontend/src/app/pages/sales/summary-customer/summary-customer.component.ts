import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { FluidModule } from 'primeng/fluid';

import { MessageService } from 'primeng/api';
import { ReportService } from '../../../core/services/report.service';
import { SummaryCustomerReportRequest } from '../../../core/models/summary-customer.report.models';
import { finalize } from 'rxjs';

export interface SummaryCustomerItem {
  accountDescription: string;
  debit: number;
  credit: number;
  recBalance: number;
  currency: string;
}

@Component({
  selector: 'app-summary-customer',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePickerModule,
    ButtonModule,
    TableModule,
    CardModule,
    FluidModule,
  ],

  template: `
    <div class="summary-customer-page relative z-0">
      <div class="flex flex-col gap-4">
        <div class="card">
          <div class="font-semibold text-xl mb-4">Summary Customer</div>

          <p-fluid>
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6 items-end">
              <!-- Starting Date -->
              <div class="flex flex-col gap-2">
                <label for="startDate" class="font-medium text-sm text-surface-600"
                  >Starting Date</label
                >
                <p-datepicker
                  id="startDate"
                  [(ngModel)]="startDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="29.12.2025"
                ></p-datepicker>
              </div>

              <!-- Ending Date -->
              <div class="flex flex-col gap-2">
                <label for="endDate" class="font-medium text-sm text-surface-600"
                  >Ending Date</label
                >
                <p-datepicker
                  id="endDate"
                  [(ngModel)]="endDate"
                  [showIcon]="true"
                  dateFormat="dd.mm.yy"
                  placeholder="05.01.2026"
                ></p-datepicker>
              </div>

              <!-- Spacer for alignment -->
              <div class="hidden md:block"></div>

              <!-- Show Details Button -->
              <div class="flex justify-end">
                <p-button
                  label="Show Details"
                  severity="danger"
                  (onClick)="onShowDetails()"
                  [loading]="loading()"
                ></p-button>
              </div>
            </div>
          </p-fluid>
        </div>

        <div class="card p-0 overflow-hidden">
          <p-table
            [value]="summaryData()"
            [rows]="10"
            [paginator]="true"
            [lazy]="true"
            (onLazyLoad)="onLazyLoad($event)"
            [totalRecords]="totalRecords()"
            [rowsPerPageOptions]="[10, 20, 50]"
            styleClass="p-datatable-gridlines p-datatable-sm"
            [responsiveLayout]="'scroll'"
            paginatorDropdownAppendTo="body"
          >
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="accountDescription" class="bg-surface-50 text-center">
                  Current Account Description <p-sortIcon field="accountDescription"></p-sortIcon>
                </th>
                <th pSortableColumn="debit" class="bg-surface-50 text-center" style="width: 15rem">
                  Debit <p-sortIcon field="debit"></p-sortIcon>
                </th>
                <th pSortableColumn="credit" class="bg-surface-50 text-center" style="width: 15rem">
                  Credit <p-sortIcon field="credit"></p-sortIcon>
                </th>
                <th
                  pSortableColumn="recBalance"
                  class="bg-surface-50 text-center"
                  style="width: 15rem"
                >
                  RecBalance <p-sortIcon field="recBalance"></p-sortIcon>
                </th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-item>
              <tr>
                <td class="text-center">{{ item.accountDescription }}</td>
                <td class="text-right">{{ item.debit | currency: 'USD' : 'symbol' : '1.2-2' }}</td>
                <td class="text-right">{{ item.credit | currency: 'USD' : 'symbol' : '1.2-2' }}</td>
                <td class="text-right">
                  {{ item.recBalance | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="footer">
              <tr class="font-bold">
                <td class="text-right bg-surface-50 pr-4">TOTAL :</td>
                <td class="text-right bg-surface-50">
                  {{ totals().debit | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right bg-surface-50">
                  {{ totals().credit | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
                <td class="text-right bg-surface-50">
                  {{ totals().recBalance | currency: 'USD' : 'symbol' : '1.2-2' }}
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="4" class="text-center p-4 text-muted-color">
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
          font-weight: 600;
        }
        .p-datatable-gridlines .p-datatable-tbody > tr > td {
          border-width: 1px;
          padding: 0.75rem;
          font-size: 0.875rem;
        }
        .p-datatable-gridlines .p-datatable-tfoot > tr > td {
          border-width: 1px;
          padding: 0.75rem;
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SummaryCustomer {
  private messageService = inject(MessageService);
  private reportService = inject(ReportService);

  loading = signal(false);

  // Filtering data
  startDate = signal<Date | null>(null);
  endDate = signal<Date | null>(null);

  // Table data
  summaryData = signal<SummaryCustomerItem[]>([]);

  // Totals
  totals = signal({
    debit: 0,
    credit: 0,
    recBalance: 0,
  });

  // Pagination & Sort State
  totalRecords = signal(0);
  currentPage = signal(1);
  pageSize = signal(10);
  sortField = signal<string | undefined>(undefined);
  sortOrder = signal<number | undefined>(undefined);

  // When Show Details button is clicked
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
    this.loading.set(true);

    const request: SummaryCustomerReportRequest = {
      startDate: this.startDate() ?? undefined,
      endDate: this.endDate() ?? undefined,
      page: this.currentPage(),
      pageSize: this.pageSize(),
      sortField: this.sortField(),
      sortOrder: this.sortOrder(),
    };

    this.reportService
      .getSummaryCustomerData(request)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            const data = res.data;
            this.summaryData.set(data.data);
            this.totalRecords.set(data.totalCount);

            this.totals.set({
              debit: data.totalDebit,
              credit: data.totalCredit,
              recBalance: data.totalRecBalance,
            });

            if (this.currentPage() === 1) {
              if (!data.data || data.data.length === 0) {
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
          console.error('Summary customer load error:', err);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'An error occurred while loading summary data.',
          });
        },
      });
  }

  // Export to Excel
  onExportExcel() {
    this.messageService.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Excel file is being prepared...',
    });
  }
}
