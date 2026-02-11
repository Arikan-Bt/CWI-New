import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { TagModule } from 'primeng/tag';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MessageService, ConfirmationService } from 'primeng/api';

import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { finalize, Observable } from 'rxjs';

import { UserService, UserDto } from '../../core/services/user.service';
import { UserDetailModalComponent } from '../../shared/components/user-detail-modal/user-detail-modal.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    FormsModule,
    TagModule,
    IconFieldModule,
    InputIconModule,

    ConfirmDialogModule,
    UserDetailModalComponent,
  ],
  providers: [ConfirmationService],
  template: `
    <div class="card">
      <div class="flex items-center justify-between mb-4">
        <div class="font-semibold text-xl">User Management</div>
        <div class="flex gap-2">
          <p-iconfield iconPosition="left">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              (input)="onSearch($event)"
              placeholder="Search Users..."
            />
          </p-iconfield>
          <p-button label="Add User" icon="pi pi-plus" (onClick)="showDialog()"></p-button>
        </div>
      </div>

      <p-confirmdialog></p-confirmdialog>

      <p-table
        #dt
        [value]="users()"
        [lazy]="true"
        (onLazyLoad)="loadUsers($event)"
        [rows]="10"
        [paginator]="true"
        [totalRecords]="totalRecords()"
        [loading]="loading()"
        responsiveLayout="scroll"
        [rowsPerPageOptions]="[10, 20, 50]"
        [globalFilterFields]="['name', 'email', 'role', 'status']"
      >
        <ng-template pTemplate="header">
          <tr>
            <th pSortableColumn="name">
              <div class="flex items-center gap-2">
                Full Name <p-sortIcon field="name"></p-sortIcon>
                <p-columnFilter type="text" field="name" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="email">
              <div class="flex items-center gap-2">
                Email <p-sortIcon field="email"></p-sortIcon>
                <p-columnFilter type="text" field="email" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="role">
              <div class="flex items-center gap-2">
                Role <p-sortIcon field="role"></p-sortIcon>
                <p-columnFilter type="text" field="role" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="status">
              <div class="flex items-center gap-2">
                Status <p-sortIcon field="status"></p-sortIcon>
                <p-columnFilter type="text" field="status" display="menu"></p-columnFilter>
              </div>
            </th>
            <th style="width: 8rem">Actions</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-user>
          <tr>
            <td>{{ user.name }} {{ user.surname }}</td>
            <td>{{ user.email }}</td>
            <td>
              <span [class]="'role-badge ' + getRoleClass(user.role)">{{ user.role }}</span>
            </td>
            <td>
              <p-tag [value]="user.status" [severity]="getSeverity(user.status)"></p-tag>
            </td>
            <td>
              <div class="flex gap-2">
                <p-button
                  icon="pi pi-pencil"
                  [text]="true"
                  severity="secondary"
                  (onClick)="editUser(user)"
                ></p-button>
                <p-button
                  icon="pi pi-trash"
                  [text]="true"
                  severity="danger"
                  (onClick)="deleteUser(user)"
                ></p-button>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>

      <app-user-detail-modal
        [(visible)]="modalVisible"
        [initialUser]="selectedUser()"
        [isEditMode]="isEditMode()"
        [saving]="saving()"
        (save)="onSaveUser($event)"
      ></app-user-detail-modal>
    </div>
  `,
  // ... Keep styles as they were, but remove modal specific styles if they are not used elsewhere.
  // Actually, wait. The styles were scoped to the component. The modal has its own styles now.
  // But UserManagement might still need some styles for the table.
  styles: [
    `
      .role-badge {
        padding: 4px 12px;
        border-radius: 6px;
        font-size: 10px;
        font-weight: 800;
        text-transform: uppercase;
        letter-spacing: 0.05em;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 80px;
      }
      .role-admin {
        background-color: #e0e7ff;
        color: #3730a3;
      }
      .role-vendor {
        background-color: #ffedd5;
        color: #9a3412;
      }
      .role-manager {
        background-color: #dcfce7;
        color: #166534;
      }
      .role-user {
        background-color: #f1f5f9;
        color: #475569;
      }
      .role-customer {
        background-color: #e0f2fe;
        color: #075985;
      }
      .role-default {
        background-color: #f3f4f6;
        color: #374151;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserManagement implements OnInit {
  private userService = inject(UserService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  // Signals for state
  users = signal<UserDto[]>([]);
  totalRecords = signal(0);
  loading = signal(false);
  saving = signal(false);
  modalVisible = signal(false);
  isEditMode = signal(false);
  searchTerm = signal<string | undefined>(undefined);
  selectedUser = signal<any>(null);
  private currentSortField?: string;
  private currentSortOrder = 1;

  ngOnInit() {
    this.loadUsers({ first: 0, rows: 10 });
  }

  onSearch(event: any) {
    const value = event.target.value;
    this.searchTerm.set(value);
    this.loadUsers({ first: 0, rows: 10 });
  }

  loadUsers(event: TableLazyLoadEvent) {
    this.loading.set(true);
    const page = (event.first || 0) / (event.rows || 10) + 1;
    const pageSize = event.rows || 10;
    const filters = (event as any)?.filters;
    const getFilterValue = (field: string) => filters?.[field]?.value ?? filters?.[field]?.[0]?.value;
    this.currentSortField = (event.sortField as string) || this.currentSortField;
    this.currentSortOrder = event.sortOrder === -1 ? -1 : event.sortOrder === 1 ? 1 : this.currentSortOrder;

    this.userService
      .getUsers(
        page,
        pageSize,
        this.searchTerm(),
        this.currentSortField,
        this.currentSortOrder,
        {
          filterName: getFilterValue('name'),
          filterEmail: getFilterValue('email'),
          filterRole: getFilterValue('role'),
          filterStatus: getFilterValue('status'),
        },
      )
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            this.users.set(result.data.items);
            this.totalRecords.set(result.data.totalCount);
          }
        },
        error: () =>
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load users.',
          }),
      });
  }

  getRoleClass(role: string) {
    if (!role) return 'role-default';
    switch (role.toLowerCase()) {
      case 'admin':
        return 'role-admin';
      case 'manager':
        return 'role-manager';
      case 'user':
        return 'role-user';
      case 'vendor':
        return 'role-vendor';
      case 'customer':
        return 'role-customer';
      default:
        return 'role-default';
    }
  }

  getSeverity(status: string) {
    switch (status) {
      case 'Active':
        return 'success';
      case 'Inactive':
        return 'danger';
      case 'Pending':
        return 'warn';
      default:
        return 'info';
    }
  }

  showDialog() {
    this.isEditMode.set(false);
    this.selectedUser.set(null);
    this.modalVisible.set(true);
  }

  editUser(user: UserDto) {
    this.loading.set(true);
    this.userService
      .getUserById(user.id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => {
          this.isEditMode.set(true);
          this.selectedUser.set(data);
          this.modalVisible.set(true);
        },
        error: () =>
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load user details.',
          }),
      });
  }

  deleteUser(user: UserDto) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete the user "${user.name} ${user.surname}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-secondary p-button-outlined',
      accept: () => {
        this.loading.set(true);
        this.userService
          .deleteUser(user.id)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'User successfully deleted',
              });
              this.loadUsers({ first: 0, rows: 10 });
            },
            error: () =>
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: 'Failed to delete user.',
              }),
          });
      },
    });
  }

  onSaveUser(formData: any) {
    this.saving.set(true);
    const action$ = this.isEditMode()
      ? (this.userService.updateUser(formData.id, formData) as Observable<any>)
      : (this.userService.createUser(formData) as Observable<any>);

    action$.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `User successfully ${this.isEditMode() ? 'updated' : 'created'}`,
        });
        this.modalVisible.set(false);
        this.loadUsers({ first: 0, rows: 10 });
      },
      error: (err: any) =>
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error || 'An error occurred while saving',
        }),
    });
  }
}
