import {
  Component,
  model,
  input,
  output,
  inject,
  signal,
  effect,
  computed,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TabsModule } from 'primeng/tabs';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { FullScreenModalComponent } from '../full-screen-modal/full-screen-modal.component';
import { RoleService, RoleDto } from '../../../core/services/role.service';
import { ReportService } from '../../../core/services/report.service';
import { UserActivityTableComponent } from '../user-activity-table/user-activity-table.component';
import { finalize } from 'rxjs';

/**
 * Reusable modal for displaying and editing user details.
 * Can be used for Admin User Management and Self Profile Management.
 */
@Component({
  selector: 'app-user-detail-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    TabsModule,
    SelectModule,
    MultiSelectModule,
    FullScreenModalComponent,
    UserActivityTableComponent,
  ],

  template: `
    <app-full-screen-modal [(visible)]="visible">
      <!-- Header İçeriği -->
      <div header class="flex items-center gap-4 py-2">
        <div
          class="user-avatar h-12 w-12 rounded-full flex items-center justify-center shadow-sm border bg-gray-50 dark:bg-surface-900 border-gray-100 dark:border-surface-800"
        >
          <i class="pi pi-user text-2xl text-gray-400 dark:text-muted-color"></i>
        </div>
        <div class="flex flex-col">
          <span class="text-xl font-bold text-gray-800 dark:text-color">{{
            isEditMode() && userForm() ? userForm().name + ' ' + userForm().surname : 'New User'
          }}</span>
          <span
            class="text-[10px] font-bold uppercase tracking-widest text-gray-400 dark:text-muted-color"
          >
            {{
              isSelfProfile()
                ? 'MY PROFILE'
                : isEditMode()
                  ? 'EDIT USER DETAILS'
                  : 'CREATE NEW USER'
            }}
          </span>
        </div>
      </div>

      <!-- Body İçeriği -->
      @if (userForm()) {
        <div class="edit-modal-container flex flex-col flex-1 w-full overflow-hidden">
          <p-tabs
            [value]="activeTab()"
            (valueChange)="activeTab.set($any($event).toString())"
            class="flex-1 flex flex-col min-h-0 h-full"
            styleClass="custom-tabs"
          >
            <p-tablist>
              <p-tab value="0">Dashboard</p-tab>
              <p-tab value="1">Profile</p-tab>
              @if (!isSelfProfile()) {
                <p-tab value="2">Restrictions</p-tab>
              }
            </p-tablist>

            <p-tabpanels style="height: 100%">
              <!-- Dashboard -->
              @if (isEditMode()) {
                <p-tabpanel value="0">
                  <div class="p-6 h-full flex flex-col overflow-hidden">
                    <div class="flex justify-between items-center mb-6 shrink-0">
                      <span class="text-xl font-bold text-gray-800 dark:text-white"
                        >User Activities</span
                      >
                    </div>
                    <!-- User Activity Table -->
                    <div class="flex-1 min-h-0">
                      <app-user-activity-table [userId]="userForm()?.id"></app-user-activity-table>
                    </div>
                  </div>
                </p-tabpanel>
              }

              <!-- Profile -->
              <p-tabpanel value="1">
                <div class="px-6 py-8 h-full overflow-y-auto">
                  <div class="grid grid-cols-1 lg:grid-cols-2 gap-x-20 gap-y-8 w-full pb-10">
                    <!-- SOL KOLON: Şifre ve Temel Bilgiler -->
                    <div class="flex flex-col gap-6 w-full">
                      @if (isSelfProfile()) {
                        <div class="border-b border-gray-100 dark:border-surface-800 pb-2 mb-2">
                          <h3 class="font-bold text-gray-800 dark:text-gray-100">
                            Change Password
                          </h3>
                        </div>

                        <!-- Mevcut Şifre -->
                        <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                          <label
                            class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                            >Current Password</label
                          >
                          <div class="md:col-span-4">
                            <input
                              pInputText
                              type="password"
                              [(ngModel)]="userForm().currentPassword"
                              class="w-full"
                              placeholder="Required to change password"
                            />
                          </div>
                        </div>

                        <!-- Yeni Şifre -->
                        <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                          <label
                            class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                            >Select Password (not displayed)</label
                          >
                          <div class="md:col-span-4">
                            <input
                              pInputText
                              type="password"
                              [(ngModel)]="userForm().newPassword"
                              class="w-full"
                              placeholder="New Password"
                            />
                          </div>
                        </div>

                        <!-- Şifre Onay -->
                        <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                          <label
                            class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                            >Confirm Password</label
                          >
                          <div class="md:col-span-4">
                            <input
                              pInputText
                              type="password"
                              [(ngModel)]="userForm().confirmNewPassword"
                              class="w-full"
                              placeholder="Confirm New Password"
                            />
                            @if (
                              userForm().newPassword &&
                              userForm().confirmNewPassword &&
                              userForm().newPassword !== userForm().confirmNewPassword
                            ) {
                              <small class="text-red-500 block mt-1">Passwords do not match.</small>
                            }
                          </div>
                        </div>
                      } @else {
                        <!-- Admin Modu: Temel Hesap Bilgileri -->
                        <div class="border-b border-gray-100 dark:border-surface-800 pb-2 mb-2">
                          <h3 class="font-bold text-gray-800 dark:text-gray-100">
                            Account Details
                          </h3>
                        </div>

                        <!-- Client Code -->
                        <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                          <label
                            class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                            >Client Code</label
                          >
                          <div class="md:col-span-4">
                            <input
                              pInputText
                              [(ngModel)]="userForm().clientCode"
                              class="w-full"
                              [ngClass]="{ 'bg-gray-50 dark:bg-surface-900': isEditMode() }"
                              [disabled]="isEditMode()"
                              placeholder="Enter client code"
                            />
                          </div>
                        </div>

                        <!-- Admin Password -->
                        <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                          <label
                            class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                            >Password</label
                          >
                          <div class="md:col-span-4">
                            <input
                              pInputText
                              type="password"
                              [(ngModel)]="userForm().password"
                              class="w-full"
                              placeholder="••••••"
                            />
                          </div>
                        </div>

                        <!-- Role -->
                        <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                          <label
                            class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                            >Role</label
                          >
                          <div class="md:col-span-4">
                            <p-select
                              [options]="roles()"
                              [(ngModel)]="userForm().roleId"
                              optionLabel="name"
                              optionValue="id"
                              class="w-full"
                              placeholder="Select Role"
                              [loading]="loadingRoles()"
                            />
                          </div>
                        </div>

                        <!-- Status -->
                        <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                          <label
                            class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                            >Status</label
                          >
                          <div class="md:col-span-4">
                            <p-select
                              [options]="statusOptions"
                              [(ngModel)]="userForm().status"
                              optionLabel="label"
                              optionValue="value"
                              class="w-full"
                              placeholder="Select status"
                            />
                          </div>
                        </div>
                      }
                    </div>

                    <!-- SAĞ KOLON: Kişisel Bilgiler -->
                    <div class="flex flex-col gap-6">
                      <div class="border-b border-gray-100 dark:border-surface-800 pb-2 mb-2">
                        <h3 class="font-bold text-gray-800 dark:text-gray-100">
                          Personal Information
                        </h3>
                      </div>

                      <!-- Ad -->
                      <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                        <label
                          class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                          >First Name</label
                        >
                        <div class="md:col-span-4">
                          <input
                            pInputText
                            [(ngModel)]="userForm().name"
                            class="w-full"
                            placeholder="Enter first name"
                          />
                        </div>
                      </div>

                      <!-- Soyad -->
                      <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                        <label
                          class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                          >Last Name</label
                        >
                        <div class="md:col-span-4">
                          <input
                            pInputText
                            [(ngModel)]="userForm().surname"
                            class="w-full"
                            placeholder="Enter last name"
                          />
                        </div>
                      </div>

                      <!-- E-posta -->
                      <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                        <label
                          class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                          >Email Address</label
                        >
                        <div class="md:col-span-4">
                          <input
                            pInputText
                            [(ngModel)]="userForm().email"
                            class="w-full"
                            placeholder="example@mail.com"
                          />
                        </div>
                      </div>

                      <!-- Telefon -->
                      <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                        <label
                          class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                          >Mobile Phone</label
                        >
                        <div class="md:col-span-4">
                          <input
                            pInputText
                            [(ngModel)]="userForm().mobilePhone"
                            class="w-full"
                            placeholder="+90 ..."
                          />
                        </div>
                      </div>

                      <!-- Cari Hesap (Sadece Admin modunda) -->
                      @if (!isSelfProfile()) {
                        <div class="grid grid-cols-1 md:grid-cols-5 items-center gap-4">
                          <label
                            class="md:text-right font-bold text-sm text-gray-600 dark:text-muted-color"
                            >Current Account</label
                          >
                          <div class="md:col-span-4">
                            <p-select
                              [options]="accountOptions()"
                              [(ngModel)]="userForm().currentAccount"
                              optionLabel="label"
                              optionValue="value"
                              class="w-full"
                              placeholder="Select account"
                            />
                          </div>
                        </div>
                      }
                    </div>
                  </div>
                </div>
              </p-tabpanel>

              <!-- Restrictions (Admin Only) -->
              @if (!isSelfProfile()) {
                <p-tabpanel value="2">
                  <div class="px-6 py-8 w-full h-full overflow-y-auto pb-20">
                    <div class="flex flex-col gap-8 w-full">
                      <!-- Allowed Brands -->
                      <div class="flex flex-col gap-2">
                        <label class="text-xs font-bold text-gray-500 uppercase"
                          >Allowed Brands</label
                        >
                        <p-multiselect
                          [options]="brandOptions()"
                          [(ngModel)]="userForm().allowedBrands"
                          optionLabel="label"
                          optionValue="value"
                          display="chip"
                          filter="true"
                          [maxSelectedLabels]="100"
                          placeholder="Select Allowed Brands"
                          styleClass="w-full restriction-multiselect"
                        />
                      </div>

                      <!-- Restricted Brands -->
                      <div class="flex flex-col gap-2">
                        <label class="text-xs font-bold text-gray-500 uppercase"
                          >Restricted Brands</label
                        >
                        <p-multiselect
                          [options]="brandOptions()"
                          [(ngModel)]="userForm().restrictedBrands"
                          optionLabel="label"
                          optionValue="value"
                          display="chip"
                          filter="true"
                          [maxSelectedLabels]="100"
                          placeholder="Select Restricted Brands"
                          styleClass="w-full restriction-multiselect"
                        />
                      </div>

                      <!-- Allowed Products -->
                      <div class="flex flex-col gap-2">
                        <label class="text-xs font-bold text-gray-500 uppercase"
                          >Allowed Products</label
                        >
                        <p-multiselect
                          [options]="productOptions()"
                          [(ngModel)]="userForm().allowedProducts"
                          optionLabel="label"
                          optionValue="value"
                          display="chip"
                          filter="true"
                          [maxSelectedLabels]="100"
                          placeholder="Select Allowed Products"
                          styleClass="w-full restriction-multiselect"
                        />
                      </div>

                      <!-- Blocked Products -->
                      <div class="flex flex-col gap-2">
                        <label class="text-xs font-bold text-gray-500 uppercase"
                          >Blocked Products</label
                        >
                        <p-multiselect
                          [options]="productOptions()"
                          [(ngModel)]="userForm().blockedProducts"
                          optionLabel="label"
                          optionValue="value"
                          display="chip"
                          filter="true"
                          [maxSelectedLabels]="100"
                          placeholder="Select Blocked Products"
                          styleClass="w-full restriction-multiselect"
                        />
                      </div>

                      <div class="mt-4 flex flex-col gap-1 text-[11px] font-medium text-[#e91e63]">
                        <div>
                          * If brands are entered in the Allowed Brands field, all brands other than
                          those entered will not be displayed.
                        </div>
                        <div>
                          * If only the Restricted Brands field is used, brands other than those
                          entered will continue to be shown.
                        </div>
                      </div>
                    </div>
                  </div>
                </p-tabpanel>
              }
            </p-tabpanels>
          </p-tabs>
        </div>
      }

      <!-- Footer İçeriği -->
      <div footer class="flex justify-end gap-1">
        <p-button
          label="Close"
          severity="secondary"
          [outlined]="true"
          (onClick)="visible.set(false)"
          styleClass="m-0"
        ></p-button>
        @if (activeTab() !== '0') {
          <p-button
            [label]="
              isEditMode() ? (isSelfProfile() ? 'Save Changes' : 'Update User') : 'Create User'
            "
            severity="primary"
            (onClick)="onSave()"
            [disabled]="!isFormValid() || saving()"
            [loading]="saving()"
            styleClass="m-0"
          ></p-button>
        }
      </div>
    </app-full-screen-modal>
  `,
  styles: [
    `
      :host ::ng-deep .modal-body {
        overflow: hidden !important;
      }

      ::ng-deep .edit-modal-container {
        flex: 1 1 auto !important;
        display: flex !important;
        flex-direction: column !important;
        min-height: 0 !important;
        height: 100% !important;
      }

      ::ng-deep {
        p-tabs.custom-tabs {
          display: flex !important;
          flex-direction: column !important;
          flex: 1 1 0% !important;
          height: 100% !important;
          overflow: hidden !important;
        }

        /* Enforce internal structure flex */
        .custom-tabs .p-tablist {
          flex: 0 0 auto !important;
          background: var(--p-surface-0, #fff);
          border-bottom: 1px solid var(--p-surface-border, #f1f5f9);
        }

        .custom-tabs .p-tablist-tab-list {
          border: none;
          padding: 0 1.5rem;
          background: transparent;
        }

        .custom-tabs .p-tabpanels {
          flex: 1 1 0% !important;
          height: 100% !important;
          min-height: 0 !important;
          overflow: hidden !important;
          background-color: var(--p-surface-ground, #f8fafc);
          padding: 0;
          display: flex !important;
          flex-direction: column !important;
        }

        .custom-tabs .p-tabpanel {
          flex: 1 1 auto !important;
          height: 100% !important;
          min-height: 0 !important;
          overflow: hidden !important;
          padding: 0 !important;
          display: flex !important;
          flex-direction: column !important;
        }

        .p-tab {
          padding: 1rem 1.5rem;
          font-weight: 600;
          color: var(--p-text-muted-color, #64748b);
          border-bottom: 2px solid transparent;
          background: transparent;
          transition: all 0.2s;
          cursor: pointer;

          &:hover {
            color: var(--p-primary-color);
          }
          &.p-tab-active {
            color: var(--p-primary-color);
            border-bottom-color: var(--p-primary-color);
          }
        }
      }

      ::ng-deep .restriction-multiselect.p-multiselect {
        height: auto !important;
        min-height: 42px;
        padding: 6px 8px;
        border-radius: 8px;
      }

      ::ng-deep .restriction-multiselect .p-multiselect-label {
        display: flex !important;
        flex-wrap: wrap !important;
        gap: 6px !important;
        padding: 0 !important;
        white-space: normal !important;
        overflow: visible !important;
        align-items: center;
      }

      ::ng-deep .restriction-multiselect .p-chip {
        background-color: var(--p-primary-color) !important;
        color: var(--p-primary-contrast-color) !important;
        border-radius: 4px !important;
        font-size: 11px !important;
        font-weight: 700 !important;
        padding: 4px 10px !important;
        height: 26px !important;
        margin: 0 !important;
        border: none !important;
      }

      ::ng-deep .restriction-multiselect .p-chip-label {
        color: var(--p-primary-contrast-color) !important;
      }
      ::ng-deep .restriction-multiselect .p-chip-remove-icon {
        color: var(--p-primary-contrast-color) !important;
        font-size: 10px !important;
      }
    `,
  ],
})
export class UserDetailModalComponent implements OnInit {
  visible = model<boolean>(false);

  // Inputs
  initialUser = input<any>(null); // Pass null for new user
  isEditMode = input<boolean>(false);
  isSelfProfile = input<boolean>(false);
  saving = input<boolean>(false);

  // Outputs
  save = output<any>();

  // Services
  private roleService = inject(RoleService);
  private reportService = inject(ReportService);

  // State
  userForm = signal<any>(null);
  activeTab = signal('1');

  // Options
  roles = signal<RoleDto[]>([]);
  accountOptions = signal<any[]>([]);
  brandOptions = signal<any[]>([]);
  productOptions = signal<any[]>([]);
  loadingRoles = signal(false);

  statusOptions = [
    { label: 'Active', value: 'Active' },
    { label: 'Inactive', value: 'Inactive' },
    { label: 'Pending', value: 'Pending' },
  ];

  constructor() {
    // Initialize form when visible or initialUser changes
    effect(() => {
      if (this.visible()) {
        this.activeTab.set(this.isEditMode() ? '0' : '1'); // Edit modunda Dashboard, yeni kullanıcıda Profile
        this.initializeForm();
      }
    });
  }

  ngOnInit() {
    // Only load lookup data if not self profile OR if self profile needs it (e.g. displaying read only values)
    // Actually self profile shows read only values so might need them mapped?
    // Name/Surname/Email are plain text.
    // Current Account and Role are selects, needs options to show correct label?
    // PrimeNG Select shows label based on value matching. So YES, we need options loaded.

    this.loadRoles();
    this.loadLookupData();
  }

  initializeForm() {
    if (this.initialUser()) {
      this.userForm.set({
        ...this.initialUser(),
        // Ensure arrays are initialized
        password: '••••••', // Placeholder
        currentPassword: '',
        newPassword: '',
        confirmNewPassword: '',
        allowedBrands: this.initialUser().allowedBrands || [],
        restrictedBrands: this.initialUser().restrictedBrands || [],
        allowedProducts: this.initialUser().allowedProducts || [],
        blockedProducts: this.initialUser().blockedProducts || [],
      });
    } else {
      // New User
      this.userForm.set({
        name: '',
        surname: '',
        email: '',
        roleId: null,
        status: 'Active',
        clientCode: '',
        password: '', // Required for new user
        mobilePhone: '',
        currentAccount: '',
        allowedBrands: [],
        restrictedBrands: [],
        allowedProducts: [],
        blockedProducts: [],
      });
    }
  }

  loadLookupData() {
    this.reportService.getCustomers().subscribe((res) => {
      if (res.success && res.data) this.accountOptions.set(res.data);
    });
    this.reportService.getBrands().subscribe((res) => {
      if (res.success && res.data) {
        const mapped: any[] = res.data.map((b) => ({ label: b.name, value: b.id }));
        this.brandOptions.set(mapped);
      }
    });
    this.reportService.getProducts().subscribe((res) => {
      if (res.success && res.data) {
        this.productOptions.set(res.data);
      }
    });
  }

  loadRoles() {
    this.loadingRoles.set(true);
    this.roleService
      .getAllRoles()
      .pipe(finalize(() => this.loadingRoles.set(false)))
      .subscribe({
        next: (data) => {
          this.roles.set(data);
        },
        error: (err) => console.error('Failed to load roles', err),
      });
  }

  isFormValid(): boolean {
    const form = this.userForm();
    if (!form) return false;

    if (this.isSelfProfile()) {
      const basicValid = !!(form.name?.trim() && form.surname?.trim() && form.email?.trim());
      // If attempting to change password, validate those fields
      if (form.currentPassword || form.newPassword || form.confirmNewPassword) {
        return (
          basicValid &&
          !!form.currentPassword &&
          !!form.newPassword &&
          form.newPassword === form.confirmNewPassword
        );
      }
      return basicValid;
    }

    return !!(
      form.name?.trim() &&
      form.surname?.trim() &&
      form.email?.trim() &&
      form.roleId &&
      form.status &&
      (this.isEditMode() || form.clientCode?.trim()) &&
      (this.isEditMode() || form.password?.trim())
    );
  }

  onSave() {
    if (this.isFormValid()) {
      this.save.emit(this.userForm());
    }
  }
}
