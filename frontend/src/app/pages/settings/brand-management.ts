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
import { Select } from 'primeng/select';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { Checkbox } from 'primeng/checkbox';
import { MessageService, ConfirmationService } from 'primeng/api';
import {
  BrandService,
  BrandDetailDto,
  CreateBrandDto,
  UpdateBrandDto,
  BrandProductDto,
  ProjectType,
} from '../../core/services/brand.service';
import { AuthService } from '../../core/services/auth.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-brand-management',
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
    Select,
    DialogModule,
    InputNumberModule,
    Checkbox,
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="card">
      <div class="flex items-center justify-between mb-4">
        <div class="font-semibold text-xl">Brand Management</div>
        <div class="flex gap-2">
          <!-- ProjectType Filtresi (sadece admin için) -->
          @if (isAdmin()) {
            <p-select
              [options]="projectTypeOptions"
              [(ngModel)]="selectedProjectType"
              (onChange)="onProjectTypeChange()"
              placeholder="All Project Types"
              [showClear]="true"
              styleClass="w-40"
            ></p-select>
          }
          <p-iconfield iconPosition="left">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              (input)="onSearch($event)"
              placeholder="Search Brands..."
              class="w-full"
            />
          </p-iconfield>
          <p-button label="Add Brand" icon="pi pi-plus" (onClick)="openCreateModal()"></p-button>
        </div>
      </div>

      <p-confirmDialog></p-confirmDialog>
      <p-toast></p-toast>

      <p-table
        #dt
        [value]="brands()"
        [lazy]="true"
        (onLazyLoad)="loadBrands($event)"
        [paginator]="true"
        [rows]="10"
        [totalRecords]="totalRecords()"
        [loading]="loading()"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
        responsiveLayout="scroll"
        styleClass="p-datatable-sm"
        [globalFilterFields]="['code', 'name', 'projectType', 'sortOrder', 'isActive']"
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
                Brand Name <p-sortIcon field="name"></p-sortIcon>
                <p-columnFilter type="text" field="name" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="projectType">
              <div class="flex items-center gap-2">
                Project Type <p-sortIcon field="projectType"></p-sortIcon>
                <p-columnFilter type="text" field="projectType" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="sortOrder">
              <div class="flex items-center gap-2">
                Sort Order <p-sortIcon field="sortOrder"></p-sortIcon>
                <p-columnFilter type="numeric" field="sortOrder" display="menu"></p-columnFilter>
              </div>
            </th>
            <th pSortableColumn="isActive">
              <div class="flex items-center gap-2">
                Status <p-sortIcon field="isActive"></p-sortIcon>
                <p-columnFilter type="text" field="isActive" display="menu"></p-columnFilter>
              </div>
            </th>
            <th style="width: 10rem" class="text-center">Actions</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-brand>
          <tr>
            <td>
              <span class="font-bold">{{ brand.code }}</span>
            </td>
            <td>
              <span class="font-medium">{{ brand.name }}</span>
            </td>
            <td>
              <p-tag
                [value]="brand.projectTypeName"
                [severity]="brand.projectType === 0 ? 'info' : 'warn'"
              ></p-tag>
            </td>
            <td>{{ brand.sortOrder }}</td>
            <td>
              <p-tag
                [value]="brand.isActive ? 'Active' : 'Inactive'"
                [severity]="brand.isActive ? 'success' : 'danger'"
              ></p-tag>
            </td>
            <td class="text-center">
              <div class="flex justify-center gap-2">
                <p-button
                  icon="pi pi-pencil"
                  [text]="true"
                  severity="secondary"
                  (onClick)="openEditModal(brand)"
                  pTooltip="Edit Brand"
                  tooltipPosition="bottom"
                ></p-button>
                <p-button
                  icon="pi pi-box"
                  [text]="true"
                  severity="info"
                  (onClick)="openProductsModal(brand)"
                  pTooltip="Manage Products"
                  tooltipPosition="bottom"
                ></p-button>
                <p-button
                  icon="pi pi-trash"
                  [text]="true"
                  severity="danger"
                  (onClick)="deleteBrand(brand)"
                ></p-button>
              </div>
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="6" class="text-center p-8">
              <div class="flex flex-col items-center gap-2 text-gray-400">
                <i class="pi pi-tags text-4xl opacity-20"></i>
                <span>No brands found.</span>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>

      <!-- Marka Detay Modal -->
      <p-dialog
        [(visible)]="modalVisible"
        [header]="editingBrand ? 'Edit Brand' : 'Add Brand'"
        [modal]="true"
        [style]="{ width: '500px' }"
        [closable]="!saving()"
      >
        <div class="flex flex-col gap-4 pt-2">
          <div class="flex flex-col gap-2">
            <label for="code" class="font-medium">Brand Code *</label>
            <input
              id="code"
              pInputText
              [(ngModel)]="formData.code"
              placeholder="Enter brand code"
              [disabled]="saving()"
            />
          </div>

          <div class="flex flex-col gap-2">
            <label for="name" class="font-medium">Brand Name *</label>
            <input
              id="name"
              pInputText
              [(ngModel)]="formData.name"
              placeholder="Enter brand name"
              [disabled]="saving()"
            />
          </div>

          <div class="flex flex-col gap-2">
            <label for="projectType" class="font-medium">Project Type *</label>
            <p-select
              id="projectType"
              [options]="projectTypeOptions"
              [(ngModel)]="formData.projectType"
              placeholder="Select project type"
              [disabled]="saving()"
            ></p-select>
          </div>

          <div class="flex flex-col gap-2">
            <label for="logoUrl" class="font-medium">Logo URL</label>
            <input
              id="logoUrl"
              pInputText
              [(ngModel)]="formData.logoUrl"
              placeholder="Enter logo URL (optional)"
              [disabled]="saving()"
            />
          </div>

          <div class="flex flex-col gap-2">
            <label for="sortOrder" class="font-medium">Sort Order</label>
            <p-inputNumber
              id="sortOrder"
              [(ngModel)]="formData.sortOrder"
              [min]="0"
              [disabled]="saving()"
            ></p-inputNumber>
          </div>

          <div class="flex items-center gap-2">
            <p-checkbox
              id="isActive"
              [(ngModel)]="formData.isActive"
              [binary]="true"
              [disabled]="saving()"
            ></p-checkbox>
            <label for="isActive" class="font-medium">Active</label>
          </div>
        </div>

        <ng-template pTemplate="footer">
          <div class="flex justify-end gap-2">
            <p-button
              label="Cancel"
              severity="secondary"
              [text]="true"
              (onClick)="closeModal()"
              [disabled]="saving()"
            ></p-button>
            <p-button
              [label]="editingBrand ? 'Update' : 'Create'"
              icon="pi pi-check"
              (onClick)="saveBrand()"
              [loading]="saving()"
              [disabled]="!isFormValid()"
            ></p-button>
          </div>
        </ng-template>
      </p-dialog>
      <!-- Marka Ürünleri Modal -->
      <p-dialog
        [(visible)]="productsModalVisible"
        [header]="'Manage Products for ' + (selectedBrandForProducts?.name || '')"
        [modal]="true"
        [style]="{ width: '100vw', height: '100vh' }"
        [closable]="!savingProducts()"
        [maximizable]="true"
        contentStyleClass="h-full"
      >
        <div class="flex flex-col gap-4 pt-2">
          <!-- Arama -->
          <p-iconfield iconPosition="left">
            <p-inputicon class="pi pi-search"></p-inputicon>
            <input
              pInputText
              type="text"
              [(ngModel)]="productSearch"
              placeholder="Search products by code or name..."
              class="w-full"
            />
          </p-iconfield>

          <p-table
            [value]="filteredProducts()"
            [scrollable]="true"
            scrollHeight="400px"
            styleClass="p-datatable-sm p-datatable-striped"
            [virtualScroll]="true"
            [virtualScrollItemSize]="46"
          >
            <ng-template pTemplate="header">
              <tr>
                <th style="width: 4rem" class="text-center">
                  <p-checkbox
                    [binary]="true"
                    (onChange)="toggleAllProducts($event)"
                    [disabled]="savingProducts()"
                  ></p-checkbox>
                </th>
                <th pSortableColumn="sku">SKU <p-sortIcon field="sku"></p-sortIcon></th>
                <th pSortableColumn="name">Product Name <p-sortIcon field="name"></p-sortIcon></th>
                <th>Current Brand</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-product>
              <tr>
                <td class="text-center">
                  <p-checkbox
                    [(ngModel)]="product.isSelected"
                    [binary]="true"
                    [disabled]="savingProducts()"
                  ></p-checkbox>
                </td>
                <td>{{ product.sku }}</td>
                <!-- İstenen: Product Name olarak brand adı gösterilecek -->
                <td>
                  <div class="flex flex-col">
                    <span class="font-medium">{{ product.brandName || product.name }}</span>
                    @if (product.brandName && product.brandName !== product.name) {
                      <span class="text-xs text-gray-500">({{ product.name }})</span>
                    }
                  </div>
                </td>
                <td>
                  @if (product.brandName) {
                    <p-tag [value]="product.brandName" severity="info"></p-tag>
                  } @else {
                    <span class="text-gray-400">-</span>
                  }
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="4" class="text-center p-4">No products found.</td>
              </tr>
            </ng-template>
          </p-table>
        </div>

        <ng-template pTemplate="footer">
          <div class="flex justify-end gap-2">
            <p-button
              label="Cancel"
              severity="secondary"
              [text]="true"
              (onClick)="closeProductsModal()"
              [disabled]="savingProducts()"
            ></p-button>
            <p-button
              label="Save Changes"
              icon="pi pi-check"
              (onClick)="saveBrandProducts()"
              [loading]="savingProducts()"
            ></p-button>
          </div>
        </ng-template>
      </p-dialog>
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
export class BrandManagement {
  // Signal'lar
  brands = signal<BrandDetailDto[]>([]);
  totalRecords = signal(0);
  loading = signal(false);
  saving = signal(false);

  // Product Assignment State
  productsModalVisible = false;
  brandProducts = signal<BrandProductDto[]>([]);
  productSearch = '';
  savingProducts = signal(false);
  selectedBrandForProducts: BrandDetailDto | null = null;

  // Services
  private brandService = inject(BrandService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private authService = inject(AuthService);

  // Pagination state
  private currentPage = 1;
  private pageSize = 10;
  private currentSearch = '';
  private currentSortField?: string;
  private currentSortOrder = 1;
  private tableFilters: any = {};
  selectedProjectType: ProjectType | null = null;

  // Modal state
  modalVisible = false;
  editingBrand: BrandDetailDto | null = null;
  formData: CreateBrandDto | UpdateBrandDto = this.getEmptyFormData();

  // ProjectType options
  projectTypeOptions = [
    { label: 'CWI', value: ProjectType.CWI },
    { label: 'AWC', value: ProjectType.AWC },
  ];

  /**
   * Admin kontrolü
   */
  isAdmin(): boolean {
    const user = this.authService.getUser();
    return user?.isAdministrator ?? false;
  }

  /**
   * Marka listesini yükler
   */
  loadBrands(event: any) {
    this.currentPage = Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1;
    this.pageSize = event.rows || 10;
    this.currentSortField = event.sortField || undefined;
    this.currentSortOrder = event.sortOrder === -1 ? -1 : 1;
    this.tableFilters = {
      filterCode: event?.filters?.code?.[0]?.value,
      filterName: event?.filters?.name?.[0]?.value,
      filterProjectType: event?.filters?.projectType?.[0]?.value,
      filterSortOrder: event?.filters?.sortOrder?.[0]?.value,
      filterStatus: event?.filters?.isActive?.[0]?.value,
    };
    this.fetchData();
  }

  /**
   * Arama yapıldığında
   */
  onSearch(event: any) {
    this.currentSearch = event.target.value;
    this.currentPage = 1; // İlk sayfaya dön
    this.fetchData();
  }

  /**
   * ProjectType filtresi değiştiğinde
   */
  onProjectTypeChange() {
    this.currentPage = 1;
    this.fetchData();
  }

  /**
   * Veriyi API'den çeker
   */
  fetchData() {
    this.loading.set(true);
    this.brandService
      .getBrands(
        this.currentPage,
        this.pageSize,
        this.currentSearch,
        this.selectedProjectType ?? undefined,
        this.currentSortField,
        this.currentSortOrder,
        this.tableFilters,
      )
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.brands.set(response.data.data);
            this.totalRecords.set(response.data.totalCount);
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load brands.',
          });
        },
      });
  }

  /**
   * Yeni marka oluşturma modalını açar
   */
  openCreateModal() {
    this.editingBrand = null;
    this.formData = this.getEmptyFormData();
    this.modalVisible = true;
  }

  /**
   * Marka düzenleme modalını açar
   */
  openEditModal(brand: BrandDetailDto) {
    this.editingBrand = brand;
    this.formData = {
      id: brand.id,
      code: brand.code,
      name: brand.name,
      logoUrl: brand.logoUrl,
      sortOrder: brand.sortOrder,
      isActive: brand.isActive,
      projectType: brand.projectType,
    };
    this.modalVisible = true;
  }

  /**
   * Modalı kapatır
   */
  closeModal() {
    this.modalVisible = false;
    this.editingBrand = null;
    this.formData = this.getEmptyFormData();
  }

  /**
   * Form doğrulama
   */
  isFormValid(): boolean {
    return !!(
      this.formData.code?.trim() &&
      this.formData.name?.trim() &&
      this.formData.projectType !== undefined
    );
  }

  /**
   * Markayı kaydeder (oluşturur veya günceller)
   */
  saveBrand() {
    if (!this.isFormValid()) return;

    this.saving.set(true);

    if (this.editingBrand) {
      // Güncelleme
      this.brandService
        .updateBrand(this.editingBrand.id, this.formData as UpdateBrandDto)
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({
          next: (res) => {
            if (res.success) {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Brand updated successfully.',
              });
              this.closeModal();
              this.fetchData();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error?.error || 'Failed to update brand',
            });
          },
        });
    } else {
      // Yeni oluşturma
      this.brandService
        .createBrand(this.formData as CreateBrandDto)
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({
          next: (res) => {
            if (res.success) {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Brand created successfully.',
              });
              this.closeModal();
              this.fetchData();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error?.error || 'Failed to create brand',
            });
          },
        });
    }
  }

  /**
   * Markayı siler
   */
  deleteBrand(brand: BrandDetailDto) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete ${brand.name}?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loading.set(true);
        this.brandService
          .deleteBrand(brand.id)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: (res) => {
              if (res.success) {
                this.messageService.add({
                  severity: 'success',
                  summary: 'Success',
                  detail: 'Brand deleted successfully.',
                });
                this.fetchData();
              }
            },
            error: (err) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: err.error?.error || 'Failed to delete brand',
              });
            },
          });
      },
    });
  }

  /**
   * Ürün yönetimi modalını açar
   */
  openProductsModal(brand: BrandDetailDto) {
    this.selectedBrandForProducts = brand;
    this.productSearch = '';
    this.loading.set(true);

    this.brandService
      .getBrandProducts(brand.id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.brandProducts.set(res.data);
            this.productsModalVisible = true;
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load products.',
          });
        },
      });
  }

  /**
   * Ürün yönetimi modalını kapatır
   */
  closeProductsModal() {
    this.productsModalVisible = false;
    this.selectedBrandForProducts = null;
    this.brandProducts.set([]);
  }

  /**
   * Filtrelenmiş ürün listesi (arama için)
   */
  filteredProducts() {
    const search = this.productSearch.toLowerCase().trim();
    if (!search) return this.brandProducts();

    return this.brandProducts().filter(
      (p) =>
        p.sku.toLowerCase().includes(search) ||
        p.name.toLowerCase().includes(search) ||
        (p.brandName && p.brandName.toLowerCase().includes(search)),
    );
  }

  /**
   * Tüm ürünleri seç/kaldır
   */
  toggleAllProducts(event: any) {
    const isChecked = event.checked;
    const currentProducts = this.filteredProducts();

    // Sadece filtrelenmiş (görünen) ürünleri etkilemeli
    this.brandProducts.update((products) => {
      return products.map((p) => {
        // Eğer ürün filtrelenen listedeyse durumunu güncelle
        if (currentProducts.some((cp) => cp.id === p.id)) {
          return { ...p, isSelected: isChecked };
        }
        return p;
      });
    });
  }

  /**
   * Ürün atamalarını kaydeder
   */
  saveBrandProducts() {
    if (!this.selectedBrandForProducts) return;

    const selectedIds = this.brandProducts()
      .filter((p) => p.isSelected)
      .map((p) => p.id);

    this.savingProducts.set(true);

    this.brandService
      .updateBrandProducts(this.selectedBrandForProducts.id, selectedIds)
      .pipe(finalize(() => this.savingProducts.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Brand products updated successfully.',
            });
            this.closeProductsModal();
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err.error?.error || 'Failed to update brand products',
          });
        },
      });
  }

  /**
   * Boş form verisi oluşturur
   */
  private getEmptyFormData(): CreateBrandDto {
    return {
      code: '',
      name: '',
      logoUrl: '',
      sortOrder: 0,
      isActive: true,
      projectType: ProjectType.CWI,
    };
  }
}
