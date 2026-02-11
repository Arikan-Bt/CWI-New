import {
  Component,
  OnInit,
  signal,
  inject,
  ViewChild,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MessageService, ConfirmationService } from 'primeng/api';
import {
  CustomerService,
  CreateCustomerDto,
  CustomerDto,
} from '../../core/services/customer.service';
import { CustomerDetailModalComponent } from '../../shared/components/customer-detail-modal/customer-detail-modal.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-customer-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    ToastModule,
    ConfirmDialogModule,
    IconFieldModule,
    InputIconModule,
    CustomerDetailModalComponent,
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="card">
      <div class="flex items-center justify-between mb-4">
        <div class="font-semibold text-xl">Customer Management</div>
        <div class="flex gap-2">
          <p-iconfield iconPosition="left">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              (input)="onSearch($event)"
              placeholder="Search Customers..."
              class="w-full"
            />
          </p-iconfield>
          <p-button label="Add Customer" icon="pi pi-plus" (onClick)="openCreateModal()"></p-button>
        </div>
      </div>

      <p-confirmDialog></p-confirmDialog>
      <p-toast></p-toast>

      <p-table
        #dt
        [value]="customers()"
        [lazy]="true"
        (onLazyLoad)="loadCustomers($event)"
        [paginator]="true"
        [rows]="10"
        [totalRecords]="totalRecords()"
        [loading]="loading()"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
        responsiveLayout="scroll"
        styleClass="p-datatable-sm"
        [globalFilterFields]="['code', 'name', 'city', 'phone', 'email', 'isVendor', 'status']"
      >
        <ng-template pTemplate="header">
          <tr>
            <th pSortableColumn="code">
              <div class="flex items-center gap-2">
                Code <p-sortIcon field="code"></p-sortIcon>
                <p-columnFilter type="text" field="code" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="name">
              <div class="flex items-center gap-2">
                Customer Name <p-sortIcon field="name"></p-sortIcon>
                <p-columnFilter type="text" field="name" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="city">
              <div class="flex items-center gap-2">
                City <p-sortIcon field="city"></p-sortIcon>
                <p-columnFilter type="text" field="city" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="phone">
              <div class="flex items-center gap-2">
                Phone <p-sortIcon field="phone"></p-sortIcon>
                <p-columnFilter type="text" field="phone" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="email">
              <div class="flex items-center gap-2">
                Email <p-sortIcon field="email"></p-sortIcon>
                <p-columnFilter type="text" field="email" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="isVendor">
              <div class="flex items-center gap-2">
                Type <p-sortIcon field="isVendor"></p-sortIcon>
                <p-columnFilter type="text" field="isVendor" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="status">
              <div class="flex items-center gap-2">
                Status <p-sortIcon field="status"></p-sortIcon>
                <p-columnFilter type="text" field="status" display="menu"></p-columnFilter>
              </div>
            </th>
            <th style="width: 8rem" class="text-center">Actions</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-customer>
          <tr>
            <td>
              <span class="font-bold">{{ customer.code }}</span>
            </td>
            <td>
              <div class="flex flex-col">
                <span class="font-medium">{{ customer.name }}</span>
                @if (customer.taxNumber) {
                  <span class="text-[10px] text-gray-400 font-bold uppercase tracking-wider"
                    >TAX: {{ customer.taxNumber }}</span
                  >
                }
              </div>
            </td>
            <td>{{ customer.city || '-' }}</td>
            <td>{{ customer.phone || '-' }}</td>
            <td>{{ customer.email || '-' }}</td>
            <td>
              <p-tag
                [value]="customer.isVendor ? 'Vendor' : 'Customer'"
                [severity]="customer.isVendor ? 'warn' : 'info'"
              >
              </p-tag>
            </td>
            <td>
              <p-tag [value]="customer.status" [severity]="getSeverity(customer.status)"></p-tag>
            </td>
            <td class="text-center">
              <div class="flex justify-center gap-2">
                <p-button
                  icon="pi pi-pencil"
                  [text]="true"
                  severity="secondary"
                  (onClick)="openEditModal(customer)"
                ></p-button>
                <p-button
                  icon="pi pi-trash"
                  [text]="true"
                  severity="danger"
                  (onClick)="deleteCustomer(customer)"
                ></p-button>
              </div>
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="8" class="text-center p-8">
              <div class="flex flex-col items-center gap-2 text-gray-400">
                <i class="pi pi-users text-4xl opacity-20"></i>
                <span>No customers found.</span>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>

      <app-customer-detail-modal #detailModal></app-customer-detail-modal>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        width: 100%;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerManagement {
  customers = signal<CustomerDto[]>([]);
  totalRecords = signal(0);
  loading = signal(false);

  @ViewChild('detailModal') detailModal!: CustomerDetailModalComponent;

  private customerService = inject(CustomerService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  // Pagination state
  private currentPage = 1;
  private pageSize = 10;
  private currentSearch = '';
  private currentSortField?: string;
  private currentSortOrder = 1;
  private tableFilters: any = {};

  loadCustomers(event: any) {
    this.currentPage = Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1;
    this.pageSize = event.rows || 10;
    this.currentSortField = event.sortField || undefined;
    this.currentSortOrder = event.sortOrder === -1 ? -1 : 1;
    this.tableFilters = {
      filterCode: event?.filters?.code?.[0]?.value,
      filterName: event?.filters?.name?.[0]?.value,
      filterCity: event?.filters?.city?.[0]?.value,
      filterPhone: event?.filters?.phone?.[0]?.value,
      filterEmail: event?.filters?.email?.[0]?.value,
      filterType: event?.filters?.isVendor?.[0]?.value,
      filterStatus: event?.filters?.status?.[0]?.value,
    };
    this.fetchData();
  }

  onSearch(event: any) {
    this.currentSearch = event.target.value;
    this.currentPage = 1; // Reset to first page
    this.fetchData();
  }

  fetchData() {
    this.loading.set(true);
    this.customerService
      .getCustomers(
        this.currentPage,
        this.pageSize,
        this.currentSearch,
        this.currentSortField,
        this.currentSortOrder,
        this.tableFilters,
      )
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.customers.set(response.data.items);
            this.totalRecords.set(response.data.totalCount);
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load customers.',
          });
        },
      });
  }

  getSeverity(status: string) {
    return status === 'Active' ? 'success' : 'danger';
  }

  openCreateModal() {
    this.detailModal.open(undefined, (data) => this.createCustomer(data));
  }

  openEditModal(customer: CustomerDto) {
    this.detailModal.open(customer, (data) => this.updateCustomer(customer.id, data));
  }

  createCustomer(data: CreateCustomerDto) {
    this.detailModal.saving.set(true);
    this.customerService
      .createCustomer(data)
      .pipe(finalize(() => this.detailModal.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Customer created successfully.',
            });
            this.detailModal.close();
            this.fetchData();
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to create customer.',
          });
        },
      });
  }

  updateCustomer(id: number, data: CreateCustomerDto) {
    this.detailModal.saving.set(true);
    this.customerService
      .updateCustomer(id, data)
      .pipe(finalize(() => this.detailModal.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Customer updated successfully.',
            });
            this.detailModal.close();
            this.fetchData();
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update customer.',
          });
        },
      });
  }

  deleteCustomer(customer: CustomerDto) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete ${customer.name}?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loading.set(true);
        this.customerService
          .deleteCustomer(customer.id)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: (res) => {
              if (res.success) {
                this.messageService.add({
                  severity: 'success',
                  summary: 'Success',
                  detail: 'Customer deleted successfully.',
                });
                this.fetchData();
              }
            },
            error: (err) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: 'Failed to delete customer.',
              });
            },
          });
      },
    });
  }
}
