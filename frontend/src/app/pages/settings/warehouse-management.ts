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
  WarehouseService,
  CreateWarehouseDto,
  UpdateWarehouseDto,
  WarehouseDto,
} from '../../core/services/warehouse.service';
import { WarehouseDetailModalComponent } from '../../shared/components/warehouse-detail-modal/warehouse-detail-modal.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-warehouse-management',
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
    WarehouseDetailModalComponent,
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="card">
      <div class="flex items-center justify-between mb-4">
        <div class="font-semibold text-xl">Warehouse Management</div>
        <div class="flex gap-2">
          <p-iconfield iconPosition="left">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              (input)="onSearch($event)"
              placeholder="Search Warehouses..."
              class="w-full"
            />
          </p-iconfield>
          <p-button
            label="Add Warehouse"
            icon="pi pi-plus"
            (onClick)="openCreateModal()"
          ></p-button>
        </div>
      </div>

      <p-confirmDialog></p-confirmDialog>
      <p-toast></p-toast>

      <p-table
        #dt
        [value]="warehouses()"
        [lazy]="true"
        (onLazyLoad)="loadWarehouses($event)"
        [paginator]="true"
        [rows]="10"
        [totalRecords]="totalRecords()"
        [loading]="loading()"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
        responsiveLayout="scroll"
        styleClass="p-datatable-sm"
        [globalFilterFields]="['code', 'name', 'address', 'isActive', 'isDefault']"
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
                Warehouse Name <p-sortIcon field="name"></p-sortIcon>
                <p-columnFilter type="text" field="name" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="address">
              <div class="flex items-center gap-2">
                Address <p-sortIcon field="address"></p-sortIcon>
                <p-columnFilter type="text" field="address" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="isActive">
              <div class="flex items-center gap-2">
                Status <p-sortIcon field="isActive"></p-sortIcon>
                <p-columnFilter type="text" field="isActive" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="isDefault">
              <div class="flex items-center gap-2">
                Default <p-sortIcon field="isDefault"></p-sortIcon>
                <p-columnFilter type="text" field="isDefault" display="menu"></p-columnFilter>
              </div>
            </th>
            <th style="width: 8rem" class="text-center">Actions</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-warehouse>
          <tr>
            <td>
              <span class="font-bold">{{ warehouse.code }}</span>
            </td>
            <td>
              <span class="font-medium">{{ warehouse.name }}</span>
            </td>
            <td>{{ warehouse.address || '-' }}</td>
            <td>
              <p-tag
                [value]="warehouse.isActive ? 'Active' : 'Inactive'"
                [severity]="warehouse.isActive ? 'success' : 'danger'"
              ></p-tag>
            </td>
            <td>
              @if (warehouse.isDefault) {
                <p-tag value="Default" severity="info"></p-tag>
              } @else {
                -
              }
            </td>
            <td class="text-center">
              <div class="flex justify-center gap-2">
                <p-button
                  icon="pi pi-pencil"
                  [text]="true"
                  severity="secondary"
                  (onClick)="openEditModal(warehouse)"
                ></p-button>
                <p-button
                  icon="pi pi-trash"
                  [text]="true"
                  severity="danger"
                  (onClick)="deleteWarehouse(warehouse)"
                  [disabled]="warehouse.isDefault"
                ></p-button>
              </div>
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="6" class="text-center p-8">
              <div class="flex flex-col items-center gap-2 text-gray-400">
                <i class="pi pi-warehouse text-4xl opacity-20"></i>
                <span>No warehouses found.</span>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>

      <app-warehouse-detail-modal #detailModal></app-warehouse-detail-modal>
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
export class WarehouseManagement {
  warehouses = signal<WarehouseDto[]>([]);
  totalRecords = signal(0);
  loading = signal(false);

  @ViewChild('detailModal') detailModal!: WarehouseDetailModalComponent;

  private warehouseService = inject(WarehouseService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  // Pagination state
  private currentPage = 1;
  private pageSize = 10;
  private currentSearch = '';
  private currentSortField?: string;
  private currentSortOrder = 1;
  private tableFilters: any = {};

  loadWarehouses(event: any) {
    this.currentPage = Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1;
    this.pageSize = event.rows || 10;
    this.currentSortField = event.sortField || undefined;
    this.currentSortOrder = event.sortOrder === -1 ? -1 : 1;
    this.tableFilters = {
      filterCode: event?.filters?.code?.[0]?.value,
      filterName: event?.filters?.name?.[0]?.value,
      filterAddress: event?.filters?.address?.[0]?.value,
      filterStatus: event?.filters?.isActive?.[0]?.value,
      filterDefault: event?.filters?.isDefault?.[0]?.value,
    };
    this.fetchData();
  }

  onSearch(event: any) {
    this.currentSearch = event.target.value;
    this.currentPage = 1; // Reset ilk sayfaya
    this.fetchData();
  }

  fetchData() {
    this.loading.set(true);
    this.warehouseService
      .getWarehouses(
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
            this.warehouses.set(response.data.items);
            this.totalRecords.set(response.data.totalCount);
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load warehouses',
          });
        },
      });
  }

  openCreateModal() {
    this.detailModal.open(undefined, (data) => this.createWarehouse(data as CreateWarehouseDto));
  }

  openEditModal(warehouse: WarehouseDto) {
    this.detailModal.open(warehouse, (data) =>
      this.updateWarehouse(warehouse.id, data as UpdateWarehouseDto),
    );
  }

  createWarehouse(data: CreateWarehouseDto) {
    this.detailModal.saving.set(true);
    this.warehouseService
      .createWarehouse(data)
      .pipe(finalize(() => this.detailModal.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Warehouse created successfully.',
            });
            this.detailModal.close();
            this.fetchData();
          }
        },
        error: (err) => {
          const errorMessage = err.error?.error || 'Failed to create warehouse';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: errorMessage,
          });
        },
      });
  }

  updateWarehouse(id: number, data: UpdateWarehouseDto) {
    this.detailModal.saving.set(true);
    this.warehouseService
      .updateWarehouse(id, data)
      .pipe(finalize(() => this.detailModal.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Warehouse updated successfully.',
            });
            this.detailModal.close();
            this.fetchData();
          }
        },
        error: (err) => {
          const errorMessage = err.error?.error || 'Failed to update warehouse';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: errorMessage,
          });
        },
      });
  }

  deleteWarehouse(warehouse: WarehouseDto) {
    if (warehouse.isDefault) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Cannot delete the default warehouse.',
      });
      return;
    }

    this.confirmationService.confirm({
      message: `Are you sure you want to delete ${warehouse.name}?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loading.set(true);
        this.warehouseService
          .deleteWarehouse(warehouse.id)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: (res) => {
              if (res.success) {
                this.messageService.add({
                  severity: 'success',
                  summary: 'Success',
                  detail: 'Warehouse deleted successfully.',
                });
                this.fetchData();
              }
            },
            error: (err) => {
              const errorMessage = err.error?.error || 'Failed to delete warehouse';
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: errorMessage,
              });
            },
          });
      },
    });
  }
}
