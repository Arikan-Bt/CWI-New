import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  signal,
  computed,
  inject,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { TagModule } from 'primeng/tag';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TabsModule } from 'primeng/tabs';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { AccordionModule } from 'primeng/accordion';
import {
  RoleService,
  RoleDto,
  RoleDetailDto,
  PermissionGroupDto,
} from '../../core/services/role.service';
import { MessageService, ConfirmationService } from 'primeng/api';

import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FullScreenModalComponent } from '../../shared/components/full-screen-modal/full-screen-modal.component';
import { finalize, Observable } from 'rxjs';

@Component({
  selector: 'app-role-management',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    FullScreenModalComponent,
    InputTextModule,
    FormsModule,
    TagModule,
    IconFieldModule,
    InputIconModule,
    TabsModule,
    SelectModule,
    TextareaModule,
    CheckboxModule,
    AccordionModule,

    ConfirmDialogModule,
  ],
  providers: [ConfirmationService],
  template: `
    <div class="card">
      <div class="flex items-center justify-between mb-4">
        <div class="font-semibold text-xl">Role Management</div>
        <div class="flex gap-2">
          <p-iconfield iconPosition="left">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              (input)="dt.filterGlobal($any($event.target).value, 'contains')"
              placeholder="Search Roles..."
            />
          </p-iconfield>
          <p-button label="Add Role" icon="pi pi-plus" (onClick)="showDialog()"></p-button>
        </div>
      </div>

      <p-confirmdialog></p-confirmdialog>

      <p-table
        #dt
        [value]="roles()"
        [rows]="10"
        [lazy]="true"
        (onLazyLoad)="loadRoles($event)"
        [paginator]="true"
        [totalRecords]="totalRecords()"
        [loading]="loading()"
        responsiveLayout="scroll"
        [globalFilterFields]="['name', 'description']"
      >
        <ng-template pTemplate="header">
          <tr>
            <th pSortableColumn="name">
              <div class="flex items-center gap-2">
                Role Name <p-sortIcon field="name"></p-sortIcon>
                <p-columnFilter type="text" field="name" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="description">
              <div class="flex items-center gap-2">
                Description <p-sortIcon field="description"></p-sortIcon>
                <p-columnFilter type="text" field="description" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="userCount" class="text-center">
              Active Users <p-sortIcon field="userCount"></p-sortIcon>
            </th>
            <th pSortableColumn="isActive">
              <div class="flex items-center gap-2">
                Status <p-sortIcon field="isActive"></p-sortIcon>
                <p-columnFilter type="text" field="isActive" display="menu"></p-columnFilter>
              </div>
            </th>
            <th style="width: 8rem">Actions</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-role>
          <tr>
            <td>
              <span class="font-bold">{{ role.name }}</span>
            </td>
            <td>{{ role.description }}</td>
            <td class="text-center">
              <p-tag [value]="role.userCount.toString()" severity="secondary"></p-tag>
            </td>
            <td>
              <p-tag
                [value]="role.isActive ? 'Active' : 'Inactive'"
                [severity]="getSeverity(role.isActive ? 'Active' : 'Inactive')"
              ></p-tag>
            </td>
            <td>
              <div class="flex gap-2">
                <p-button
                  icon="pi pi-pencil"
                  [text]="true"
                  severity="secondary"
                  (onClick)="editRole(role)"
                ></p-button>
                <p-button
                  icon="pi pi-trash"
                  [text]="true"
                  severity="danger"
                  (onClick)="deleteRole(role)"
                ></p-button>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>

      <!-- Rol Ekleme/Düzenleme Modalı -->
      <app-full-screen-modal [(visible)]="modalVisible">
        <div header class="flex items-center gap-4 py-2">
          <div
            class="h-12 w-12 rounded-full flex items-center justify-center shadow-sm border bg-gray-50 dark:bg-surface-900 border-gray-100 dark:border-surface-800"
          >
            <i class="pi pi-shield text-2xl text-gray-400 dark:text-muted-color"></i>
          </div>
          <div class="flex flex-col">
            <span class="text-xl font-bold text-gray-800 dark:text-color">
              {{ isEditMode() && roleForm() ? roleForm().name : 'New Role' }}
            </span>
            <span
              class="text-[10px] font-bold uppercase tracking-widest text-gray-400 dark:text-muted-color"
            >
              {{ isEditMode() ? 'EDIT ROLE PERMISSIONS' : 'CREATE NEW ROLE' }}
            </span>
          </div>
        </div>

        @if (roleForm()) {
          <div class="edit-modal-container flex flex-col flex-1 w-full overflow-hidden">
            <p-tabs
              [value]="activeTab()"
              (valueChange)="activeTab.set($any($event).toString())"
              class="flex-1 flex flex-col min-h-0 h-full"
              styleClass="custom-tabs"
              style="height: 100% !important"
            >
              <p-tablist>
                <p-tab value="0">General Information</p-tab>
                <p-tab value="1">Permissions</p-tab>
                <p-tab value="2">Assigned Users</p-tab>
              </p-tablist>

              <p-tabpanels style="height: 94%">
                <p-tabpanel value="0">
                  <div class="h-full overflow-y-auto p-8">
                    <div class="grid grid-cols-1 gap-y-6 max-w-[800px] mx-auto">
                      <div class="grid grid-cols-1 md:grid-cols-4 items-center gap-4">
                        <label class="md:text-right font-bold text-sm text-gray-600"
                          >Role Name</label
                        >
                        <div class="md:col-span-3">
                          <input
                            pInputText
                            [(ngModel)]="roleForm().name"
                            class="w-full"
                            placeholder="Enter role name (e.g. Sales Manager)"
                          />
                        </div>
                      </div>

                      <div class="grid grid-cols-1 md:grid-cols-4 items-center gap-4">
                        <label class="md:text-right font-bold text-sm text-gray-600">Status</label>
                        <div class="md:col-span-3">
                          <p-select
                            [options]="statusOptions"
                            [(ngModel)]="roleForm().status"
                            optionLabel="label"
                            optionValue="value"
                            class="w-full"
                            placeholder="Select Status"
                          />
                        </div>
                      </div>

                      <div class="grid grid-cols-1 md:grid-cols-4 items-start gap-4">
                        <label class="md:text-right font-bold text-sm text-gray-600 mt-2"
                          >Description</label
                        >
                        <div class="md:col-span-3">
                          <textarea
                            pTextarea
                            [(ngModel)]="roleForm().description"
                            rows="5"
                            class="w-full"
                            placeholder="Briefly describe the role's responsibilities and authority level..."
                            [autoResize]="true"
                          ></textarea>
                        </div>
                      </div>
                    </div>
                  </div>
                </p-tabpanel>

                <p-tabpanel value="1">
                  <div class="flex h-full overflow-hidden bg-gray-50/50 dark:bg-surface-900/50">
                    <!-- Sol Sidebar: Gruplar -->
                    <div
                      class="w-64 shrink-0 flex flex-col border-r border-gray-200 dark:border-surface-700 bg-white dark:bg-surface-900 h-full overflow-hidden"
                    >
                      <div
                        class="p-5 border-b border-gray-100 dark:border-surface-800 bg-gray-50/30 dark:bg-surface-900"
                      >
                        <span
                          class="text-xs font-bold uppercase tracking-widest text-gray-400 dark:text-muted-color"
                          >Modules</span
                        >
                      </div>
                      <div class="flex-1 overflow-y-auto p-3 space-y-1">
                        @for (group of permissionGroups(); track group.name) {
                          <button
                            class="w-full flex items-center justify-between p-3 rounded-lg text-left transition-all duration-200 group relative"
                            [class.bg-primary-50]="activePermissionGroup()?.name === group.name"
                            [class.text-primary-700]="activePermissionGroup()?.name === group.name"
                            [class.text-gray-600]="activePermissionGroup()?.name !== group.name"
                            [class.dark:bg-primary-500/10]="
                              activePermissionGroup()?.name === group.name
                            "
                            [class.dark:text-primary-400]="
                              activePermissionGroup()?.name === group.name
                            "
                            [class.dark:text-surface-300]="
                              activePermissionGroup()?.name !== group.name
                            "
                            [class.hover:bg-gray-50]="activePermissionGroup()?.name !== group.name"
                            [class.dark:hover:bg-surface-800]="
                              activePermissionGroup()?.name !== group.name
                            "
                            (click)="activePermissionGroup.set(group)"
                          >
                            <div class="flex items-center gap-3">
                              <i
                                [class]="'pi text-lg ' + getModuleIcon(group.name)"
                                [class.text-primary-500]="
                                  activePermissionGroup()?.name === group.name
                                "
                                [class.text-gray-400]="activePermissionGroup()?.name !== group.name"
                              ></i>
                              <span class="font-medium text-sm">{{ group.name }}</span>
                            </div>

                            @if (getGroupSelectedCount(group) > 0) {
                              <span
                                class="flex items-center justify-center pointer-events-none h-5 min-w-5 px-1 rounded-full text-[10px] font-bold shadow-sm"
                                [class.bg-white]="activePermissionGroup()?.name === group.name"
                                [class.text-primary-600]="
                                  activePermissionGroup()?.name === group.name
                                "
                                [class.bg-primary-50]="activePermissionGroup()?.name !== group.name"
                                [class.text-primary-600]="
                                  activePermissionGroup()?.name !== group.name
                                "
                                [class.dark:bg-surface-800]="
                                  activePermissionGroup()?.name === group.name
                                "
                                [class.dark:text-primary-300]="
                                  activePermissionGroup()?.name === group.name
                                "
                                [class.dark:bg-surface-700]="
                                  activePermissionGroup()?.name !== group.name
                                "
                                [class.dark:text-primary-400]="
                                  activePermissionGroup()?.name !== group.name
                                "
                              >
                                {{ getGroupSelectedCount(group) }}
                              </span>
                            }

                            @if (activePermissionGroup()?.name === group.name) {
                              <div
                                class="absolute left-0 top-1/2 -translate-y-1/2 h-8 w-1 bg-primary-500 rounded-r-full shadow-[0_0_10px_rgba(var(--primary-500-rgb),0.5)]"
                              ></div>
                            }
                          </button>
                        }
                      </div>
                    </div>

                    <!-- Sağ İçerik: İzinler -->
                    <div
                      class="flex-1 flex flex-col h-full overflow-hidden bg-gray-50/30 dark:bg-surface-900/30"
                    >
                      @if (activePermissionGroup(); as group) {
                        <!-- Header -->
                        <div
                          class="flex items-center justify-between px-8 py-6 border-b border-gray-200 dark:border-surface-700 bg-white dark:bg-surface-900"
                        >
                          <div class="flex flex-col">
                            <h3
                              class="text-xl font-bold text-gray-800 dark:text-color flex items-center gap-2"
                            >
                              <i class="pi pi-shield text-primary-500"></i>
                              {{ group.name }} Permissions
                            </h3>
                            <p class="text-sm text-gray-500 dark:text-muted-color mt-1">
                              Manage access rights and capabilities for the {{ group.name }} module.
                            </p>
                          </div>

                          <div
                            class="flex items-center gap-3 bg-gray-50 dark:bg-surface-800 px-4 py-2 rounded-lg border border-gray-100 dark:border-surface-700"
                          >
                            <span class="text-sm font-semibold text-gray-600 dark:text-surface-300"
                              >Select All Permissions</span
                            >
                            <p-checkbox
                              [binary]="true"
                              [ngModel]="isAllSelected(group)"
                              (onChange)="toggleGroup(group, $event.checked)"
                            ></p-checkbox>
                          </div>
                        </div>

                        <!-- İzin Listesi -->
                        <div class="flex-1 overflow-y-auto p-8">
                          <!-- Menus Grubu için Özel Görünüm -->
                          @if (group.name === 'Menus') {
                            <div class="flex flex-col gap-10 max-w-5xl">
                              @for (
                                subGroup of getGroupedMenuPermissions(group.permissions);
                                track subGroup.category
                              ) {
                                <div class="flex flex-col gap-4">
                                  <!-- Kategori Başlığı (Prominent Title) -->
                                  <div
                                    class="flex items-center justify-between pb-3 border-b border-gray-100 dark:border-surface-800"
                                  >
                                    <div class="flex items-center gap-3">
                                      <div
                                        class="h-10 w-10 rounded-lg bg-primary-50 dark:bg-primary-500/10 flex items-center justify-center"
                                      >
                                        <i
                                          [class]="
                                            'pi text-primary-600 dark:text-primary-400 text-xl ' +
                                            getModuleIcon(subGroup.category)
                                          "
                                        ></i>
                                      </div>
                                      <div class="flex flex-col">
                                        <h4
                                          class="font-black text-gray-800 dark:text-gray-100 text-xl tracking-tight"
                                        >
                                          {{ subGroup.category }}
                                        </h4>
                                        <span
                                          class="text-xs text-gray-400 dark:text-muted-color uppercase font-bold tracking-widest"
                                          >MENU CATEGORY</span
                                        >
                                      </div>
                                    </div>
                                  </div>

                                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    @for (perm of subGroup.items; track perm.key) {
                                      @let isRoot = perm.key.split('.').length === 3;

                                      <div
                                        class="group flex items-start gap-4 p-4 rounded-xl border transition-all duration-300 cursor-pointer relative overflow-hidden"
                                        [class.bg-primary-50/20]="
                                          isPermissionSelected(perm.key) && isRoot
                                        "
                                        [class.bg-white]="
                                          !isPermissionSelected(perm.key) || !isRoot
                                        "
                                        [class.dark:bg-primary-500/5]="
                                          isPermissionSelected(perm.key) && isRoot
                                        "
                                        [class.dark:bg-surface-900]="
                                          !isPermissionSelected(perm.key) || !isRoot
                                        "
                                        [class.border-primary-500]="isPermissionSelected(perm.key)"
                                        [class.border-gray-200]="!isPermissionSelected(perm.key)"
                                        [class.dark:border-surface-700]="
                                          !isPermissionSelected(perm.key)
                                        "
                                        [class.shadow-md]="isPermissionSelected(perm.key) && isRoot"
                                        [class.scale-[1.02]]="
                                          isPermissionSelected(perm.key) && isRoot
                                        "
                                        [class.ring-2]="isPermissionSelected(perm.key) && isRoot"
                                        [class.ring-primary-500/30]="
                                          isPermissionSelected(perm.key) && isRoot
                                        "
                                        (click)="togglePermission(perm.key)"
                                      >
                                        <!-- Root Indicator Bar -->
                                        @if (isRoot) {
                                          <div
                                            class="absolute left-0 top-0 bottom-0 w-1.5 bg-primary-500"
                                          ></div>
                                        }

                                        <div class="mt-0.5" (click)="$event.stopPropagation()">
                                          <p-checkbox
                                            [binary]="true"
                                            [ngModel]="isPermissionSelected(perm.key)"
                                            (onChange)="togglePermission(perm.key)"
                                          ></p-checkbox>
                                        </div>
                                        <div class="flex flex-col gap-1">
                                          <span
                                            class="font-bold transition-colors"
                                            [class.text-xl]="isRoot"
                                            [class.text-sm]="!isRoot"
                                            [class.text-primary-700]="
                                              isPermissionSelected(perm.key) && isRoot
                                            "
                                            [class.dark:text-primary-400]="
                                              isPermissionSelected(perm.key) && isRoot
                                            "
                                            [class.text-gray-800]="
                                              !isPermissionSelected(perm.key) || !isRoot
                                            "
                                            [class.dark:text-color]="
                                              !isPermissionSelected(perm.key) || !isRoot
                                            "
                                          >
                                            {{ perm.name }}
                                            @if (isRoot) {
                                              <span
                                                class="ml-2 text-[10px] bg-primary-100 dark:bg-primary-500/20 text-primary-700 dark:text-primary-300 px-2 py-0.5 rounded-full uppercase tracking-tighter"
                                                >Main Menu</span
                                              >
                                            }
                                          </span>
                                          <span
                                            class="text-sm text-gray-500 dark:text-muted-color leading-relaxed"
                                          >
                                            {{ perm.description }}
                                          </span>
                                        </div>

                                        @if (isPermissionSelected(perm.key) && isRoot) {
                                          <i
                                            class="pi pi-check-circle absolute right-4 top-4 text-primary-500 text-xl"
                                          ></i>
                                        }
                                      </div>
                                    }
                                  </div>
                                </div>
                              }
                            </div>
                          } @else {
                            <!-- Diğer Gruplar için Standart Görünüm -->
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 max-w-5xl">
                              @for (perm of group.permissions; track perm.key) {
                                <div
                                  class="group flex items-start gap-4 p-4 rounded-xl border border-gray-200 dark:border-surface-700 bg-white dark:bg-surface-900 hover:border-primary-200 dark:hover:border-primary-500/30 hover:shadow-sm transition-all duration-200 cursor-pointer"
                                  [class.ring-1]="isPermissionSelected(perm.key)"
                                  [class.ring-primary-500]="isPermissionSelected(perm.key)"
                                  [class.border-primary-500]="isPermissionSelected(perm.key)"
                                  [class.bg-primary-50/10]="isPermissionSelected(perm.key)"
                                  (click)="togglePermission(perm.key)"
                                >
                                  <div class="mt-0.5" (click)="$event.stopPropagation()">
                                    <p-checkbox
                                      [binary]="true"
                                      [ngModel]="isPermissionSelected(perm.key)"
                                      (onChange)="togglePermission(perm.key)"
                                    ></p-checkbox>
                                  </div>
                                  <div class="flex flex-col gap-1">
                                    <span
                                      class="font-semibold text-gray-800 dark:text-color group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors"
                                    >
                                      {{ perm.name }}
                                    </span>
                                    <span
                                      class="text-sm text-gray-500 dark:text-muted-color leading-relaxed"
                                    >
                                      {{ perm.description }}
                                    </span>
                                  </div>
                                </div>
                              }
                            </div>
                          }
                        </div>
                      } @else {
                        <div class="flex flex-col items-center justify-center h-full text-gray-400">
                          <i class="pi pi-filter text-4xl mb-3 opacity-50"></i>
                          <span class="text-lg">Select a module to view permissions</span>
                        </div>
                      }
                    </div>
                  </div>
                </p-tabpanel>

                <p-tabpanel value="2">
                  <div class="h-full overflow-y-auto bg-gray-50/50 dark:bg-surface-900/50 p-8">
                    @if (roleForm()?.users?.length > 0) {
                      <div
                        class="max-w-4xl mx-auto bg-white dark:bg-surface-900 rounded-xl border border-gray-200 dark:border-surface-700 shadow-sm overflow-hidden"
                      >
                        <div
                          class="px-6 py-4 border-b border-gray-100 dark:border-surface-800 flex justify-between items-center bg-gray-50/30 dark:bg-surface-800/30"
                        >
                          <h3 class="font-bold text-gray-700 dark:text-gray-200">
                            Assigned Users ({{ roleForm()?.users?.length }})
                          </h3>
                        </div>
                        <div class="divide-y divide-gray-100 dark:divide-surface-800">
                          @for (user of roleForm()?.users; track user.id) {
                            <div
                              class="p-4 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-surface-800 transition-colors"
                            >
                              <div class="flex items-center gap-4">
                                <div
                                  class="w-10 h-10 rounded-full bg-primary-100 dark:bg-primary-500/20 text-primary-600 dark:text-primary-400 flex items-center justify-center font-bold text-sm"
                                >
                                  {{ getInitials(user.fullName) }}
                                </div>
                                <div class="flex flex-col">
                                  <span class="font-medium text-gray-800 dark:text-gray-200">{{
                                    user.fullName
                                  }}</span>
                                  <span class="text-sm text-gray-500 dark:text-gray-400">{{
                                    user.email
                                  }}</span>
                                </div>
                              </div>
                              <div class="flex items-center gap-4">
                                <span
                                  class="px-2.5 py-1 rounded-md text-xs font-semibold"
                                  [class.bg-green-100]="user.isActive"
                                  [class.text-green-700]="user.isActive"
                                  [class.bg-red-100]="!user.isActive"
                                  [class.text-red-700]="!user.isActive"
                                  [class.dark:bg-green-500/20]="user.isActive"
                                  [class.dark:text-green-400]="user.isActive"
                                  [class.dark:bg-red-500/20]="!user.isActive"
                                  [class.dark:text-red-400]="!user.isActive"
                                >
                                  {{ user.isActive ? 'Active' : 'Inactive' }}
                                </span>
                              </div>
                            </div>
                          }
                        </div>
                      </div>
                    } @else {
                      <div class="flex flex-col items-center justify-center h-full text-gray-400">
                        <div
                          class="w-16 h-16 rounded-full bg-gray-100 dark:bg-surface-800 flex items-center justify-center mb-4"
                        >
                          <i class="pi pi-users text-2xl opacity-50"></i>
                        </div>
                        <span class="text-lg font-medium">No users assigned to this role yet</span>
                        <p class="text-sm text-gray-500 mt-1">
                          Users can be assigned to this role from the User Management page.
                        </p>
                      </div>
                    }
                  </div>
                </p-tabpanel>
              </p-tabpanels>
            </p-tabs>
          </div>
        }

        <div footer class="flex justify-end gap-1">
          <p-button
            label="Close"
            severity="secondary"
            [outlined]="true"
            (onClick)="modalVisible.set(false)"
            styleClass="m-0"
          ></p-button>
          <p-button
            [label]="isEditMode() ? 'Update Role' : 'Create Role'"
            severity="primary"
            (onClick)="onSaveRole()"
            [loading]="saving()"
            styleClass="m-0"
          ></p-button>
        </div>
      </app-full-screen-modal>
    </div>
  `,
  styles: [
    `
      /* Modal Container */
      ::ng-deep .edit-modal-container {
        display: flex;
        flex-direction: column;
        height: 100%;
        overflow: hidden;
      }

      /* Tabs Host ve Container */
      ::ng-deep .custom-tabs,
      ::ng-deep p-tabs.custom-tabs {
        display: flex !important;
        flex-direction: column !important;
        height: 100% !important;
        overflow: hidden !important;
      }

      /* Tab Listesi (Header) */
      ::ng-deep .custom-tabs p-tablist,
      ::ng-deep .custom-tabs .p-tablist {
        flex-shrink: 0 !important;
        z-index: 10;
        position: relative;
        background: var(--p-surface-0, #fff);
        border-bottom: 1px solid var(--p-surface-border, #e2e8f0);
      }

      :host-context(.dark) ::ng-deep .custom-tabs p-tablist,
      :host-context(.dark) ::ng-deep .custom-tabs .p-tablist {
        border-bottom-color: var(--p-surface-700);
        background: var(--p-surface-900);
      }

      ::ng-deep .custom-tabs .p-tablist-tab-list {
        background: transparent !important;
        border: none !important;
      }

      /* Tab Butonları */
      ::ng-deep .custom-tabs .p-tab {
        padding: 1rem 1.5rem;
        font-weight: 600;
        color: var(--p-text-muted-color);
        transition: all 0.2s;
        border-bottom: 2px solid transparent;
        cursor: pointer;
      }

      ::ng-deep .custom-tabs .p-tab:hover {
        color: var(--p-primary-color);
        background: var(--p-surface-50);
      }

      :host-context(.dark) ::ng-deep .custom-tabs .p-tab:hover {
        background: var(--p-surface-800);
      }

      ::ng-deep .custom-tabs .p-tab-active {
        color: var(--p-primary-color) !important;
        border-bottom-color: var(--p-primary-color) !important;
      }

      /* Tab Panels Container (Content Area) */
      ::ng-deep .custom-tabs p-tabpanels,
      ::ng-deep .custom-tabs .p-tabpanels {
        flex: 1 1 0px !important; /* Mutlaka 0px olmalı */
        min-height: 0 !important;
        height: 100% !important;
        overflow: hidden !important; /* Scroll'u iç elemente devret */
        padding: 0 !important;
        display: flex !important;
        flex-direction: column !important;
        background-color: var(--p-surface-ground, #f8fafc);
      }

      :host-context(.dark) ::ng-deep .custom-tabs p-tabpanels,
      :host-context(.dark) ::ng-deep .custom-tabs .p-tabpanels {
        background-color: var(--p-surface-950);
      }

      /* Single Tab Panel */
      ::ng-deep .custom-tabs p-tabpanel,
      ::ng-deep .custom-tabs .p-tabpanel {
        height: 100% !important;
        width: 100% !important;
        display: block !important;
        padding: 0 !important;
        overflow: hidden !important;
      }

      :host ::ng-deep .p-tag {
        border-radius: 6px;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleManagement implements OnInit {
  private roleService = inject(RoleService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  // Signals
  roles = signal<RoleDto[]>([]);
  totalRecords = signal(0);
  permissionGroups = signal<PermissionGroupDto[]>([]);
  modalVisible = signal(false);
  isEditMode = signal(false);
  activeTab = signal('0');
  roleForm = signal<any>(null);
  loading = signal(false);
  saving = signal(false);
  expandedGroups = signal<Set<string>>(new Set());
  activePermissionGroup = signal<PermissionGroupDto | null>(null);
  private currentSortField?: string;
  private currentSortOrder = 1;

  constructor() {
    effect(() => {
      if (!this.modalVisible()) {
        this.roleForm.set(null);
        this.activePermissionGroup.set(null);
      }
    });
  }

  statusOptions = [
    { label: 'Active', value: 'Active' },
    { label: 'Inactive', value: 'Inactive' },
  ];

  ngOnInit() {
    this.loadRoles({ first: 0, rows: 10 });
    this.loadPermissions();
  }

  loadRoles(event?: any) {
    this.loading.set(true);
    const page = (event?.first || 0) / (event?.rows || 10) + 1;
    const pageSize = event?.rows || 10;
    const search = event?.globalFilter || undefined;
    this.currentSortField = event?.sortField || this.currentSortField;
    this.currentSortOrder = event?.sortOrder === -1 ? -1 : event?.sortOrder === 1 ? 1 : this.currentSortOrder;

    this.roleService
      .getRoles(page, pageSize, search, this.currentSortField, this.currentSortOrder, {
        filterName: event?.filters?.['name']?.[0]?.value,
        filterDescription: event?.filters?.['description']?.[0]?.value,
        filterStatus: event?.filters?.['isActive']?.[0]?.value,
      })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          this.roles.set(response.items);
          this.totalRecords.set(response.totalCount);
        },
        error: () =>
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load roles.',
          }),
      });
  }

  loadPermissions() {
    this.roleService.getPermissions().subscribe({
      next: (data) => {
        this.permissionGroups.set(data);
        // Varsayılan olarak ilk grubu seç
        if (data.length > 0) {
          this.activePermissionGroup.set(data[0]);
        }
      },
      error: () =>
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load permissions.',
        }),
    });
  }

  showDialog() {
    this.isEditMode.set(false);
    this.activeTab.set('0');
    // Grupları kapat
    this.expandedGroups.set(new Set());
    this.roleForm.set({
      name: '',
      description: '',
      status: 'Active',
      permissions: [],
    });
    this.modalVisible.set(true);
  }

  editRole(role: RoleDto) {
    this.loading.set(true);
    this.roleService
      .getRoleById(role.id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => {
          this.isEditMode.set(true);
          this.activeTab.set('0');
          this.roleForm.set({
            ...data,
            status: data.isActive ? 'Active' : 'Inactive',
          });
          this.modalVisible.set(true);
        },
        error: () =>
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load role details.',
          }),
      });
  }

  onSaveRole() {
    const form = this.roleForm();
    if (!form || !form.name) return;

    this.saving.set(true);
    const payload = {
      ...form,
      isActive: form.status === 'Active',
    };

    const action$ = (
      this.isEditMode()
        ? this.roleService.updateRole(form.id, payload)
        : this.roleService.createRole(payload)
    ) as Observable<any>;

    action$.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Role successfully ${this.isEditMode() ? 'updated' : 'created'}`,
        });
        this.modalVisible.set(false);
        this.loadRoles({ first: 0, rows: 10 });
      },
      error: (err: any) =>
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to save role',
        }),
    });
  }

  deleteRole(role: RoleDto) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete the role "${role.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-secondary p-button-outlined',
      accept: () => {
        this.loading.set(true);
        this.roleService
          .deleteRole(role.id)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Role deleted successfully.',
              });
              this.loadRoles({ first: 0, rows: 10 });
            },
            error: (err: any) =>
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: err.error?.error || 'Failed to delete role',
              }),
          });
      },
    });
  }

  isAllSelected(group: any): boolean {
    const form = this.roleForm();
    if (!form || !form.permissions) return false;
    return group.permissions.every((p: any) => form.permissions.includes(p.key));
  }

  toggleGroup(group: any, checked: boolean) {
    const form = this.roleForm();
    if (!form) return;

    const currentPerms = new Set(form.permissions);
    group.permissions.forEach((p: any) => {
      if (checked) currentPerms.add(p.key);
      else currentPerms.delete(p.key);
    });

    this.roleForm.update((f) => ({ ...f, permissions: Array.from(currentPerms) }));
  }

  togglePermission(key: string) {
    const form = this.roleForm();
    if (!form) return;

    let currentPerms = new Set<string>(form.permissions);

    // Şu anki durumu kontrol et
    const isCurrentlySelected = currentPerms.has(key);

    if (isCurrentlySelected) {
      // Seçimi kaldır
      currentPerms.delete(key);

      // Eğer bu bir 'root' veya 'category' menü ise, tüm alt öğeleri de kaldır
      const keyDot = key + '.';
      this.permissionGroups().forEach((group) => {
        group.permissions.forEach((p) => {
          if (p.key.startsWith(keyDot)) {
            currentPerms.delete(p.key);
          }
        });
      });
    } else {
      // Seç
      currentPerms.add(key);

      // Eğer bu bir 'root' veya 'category' menü ise, tüm alt öğeleri de ekle
      const keyDot = key + '.';
      this.permissionGroups().forEach((group) => {
        group.permissions.forEach((p) => {
          if (p.key.startsWith(keyDot)) {
            currentPerms.add(p.key);
          }
        });
      });

      // Eğer bu bir alt menü/yetki ise, üst hiyerarşiyi otomatik seç
      const parts = key.split('.');
      // Örn: Permissions.Menus.Sales.Order
      if (parts.length > 2) {
        // En az 2 seviye (örn: Permissions.Users) seçilmelidir
        for (let i = 2; i < parts.length; i++) {
          const parentKey = parts.slice(0, i).join('.');
          if (parentKey && parentKey !== 'Permissions') {
            currentPerms.add(parentKey);
          }
        }
      }
    }

    this.roleForm.update((f) => ({ ...f, permissions: Array.from(currentPerms) }));
  }

  getSeverity(status: string) {
    switch (status) {
      case 'Active':
        return 'success';
      case 'Inactive':
        return 'danger';
      default:
        return 'info';
    }
  }

  isPermissionSelected(key: string): boolean {
    const form = this.roleForm();
    if (!form || !form.permissions) return false;
    return form.permissions.includes(key);
  }

  getGroupSelectedCount(group: PermissionGroupDto): number {
    const form = this.roleForm();
    if (!form || !form.permissions) return 0;
    return group.permissions.filter((p) => form.permissions.includes(p.key)).length;
  }

  getModuleIcon(moduleName: string): string {
    switch (moduleName) {
      case 'Users':
        return 'pi-users';
      case 'Roles':
        return 'pi-shield';
      case 'Products':
        return 'pi-box';
      case 'Orders':
        return 'pi-shopping-cart';
      case 'Inventory':
        return 'pi-warehouse';
      case 'Reports':
        return 'pi-chart-bar';
      case 'Menus':
        return 'pi-bars';
      default:
        return 'pi-folder';
    }
  }

  getInitials(name: string): string {
    if (!name) return '';
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  getGroupedMenuPermissions(permissions: any[]): { category: string; items: any[] }[] {
    const grouped = new Map<string, any[]>();
    // Öncelik sırası ve beklenen ana kategoriler
    const orderedCategories = [
      'Dashboard',
      'Sales',
      'Purchase',
      'Inventory',
      'Reports',
      'Settings',
    ];

    permissions.forEach((perm) => {
      const parts = perm.key.split('.');
      // Key format: Permissions.Menus.{Category}.{Action} or Permissions.Menus.{Category}
      // parts[0]=Permissions, parts[1]=Menus, parts[2]=Category
      if (parts.length >= 3) {
        let category = parts[2];

        if (!grouped.has(category)) {
          grouped.set(category, []);
        }
        grouped.get(category)?.push(perm);
      }
    });

    const result: { category: string; items: any[] }[] = [];

    // Belirlenen sırada kategorileri ekle
    orderedCategories.forEach((cat) => {
      if (grouped.has(cat)) {
        result.push({ category: cat, items: grouped.get(cat)! });
        grouped.delete(cat);
      }
    });

    // Kalan kategorileri (eğer varsa) ekle
    grouped.forEach((items, category) => {
      result.push({ category, items });
    });

    return result;
  }
}
